using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using static Uccs.Net.DownloadReport;

namespace Uccs.Net
{
	public enum SeedStatus
	{
		Null, Good, Bad
	}

	public enum HubStatus
	{
		Null, Estimating, Bad
	}

	public class Hub
	{
		public Peer					Peer;
		public List<IPAddress>		Seeds = new();
		public HubStatus			Status = HubStatus.Estimating;
		Download					Download;
		public DateTime				Called;
		public bool					Refreshing =false;

		public Hub(Download download, Peer peer)
		{
			Download = download;
			Peer = peer;

			Refresh();
		}

		public void Refresh()
		{
			if(Refreshing)
				return;
			else
				Refreshing = true;

			Called = DateTime.UtcNow;

			Task.Run(() =>	{

								try
								{
									var lr = Download.Core.Call(Peer.IP, p => p.LocateRelease(Download.Release, 16), Download.Workflow);

									lock(Download.Lock)
									{
										Download.Workflow.Log.Report(this, "Hub gives seeds", $"for {Download.Release}, {string.Join(", ", lr.Seeders.Take(8).Select(i => i.ToString()))}");

										Seeds = lr.Seeders.ToList();

										foreach(var s in lr.Seeders)
										{
											if(!Download.Seeds.ContainsKey(s))
												Download.Seeds.Add(s, SeedStatus.Null);
										}
									}

									Called = DateTime.UtcNow;
								}
								catch(Exception ex) when (ex is ConnectionFailedException || ex is RdcException)
								{
								}
								catch(OperationCanceledException)
								{
								}

								Refreshing = false;
							}, 
							Download.Workflow.Cancellation.Token);
		}
	}

	public class DownloadReport
	{
		public class Dependency
		{
			public ReleaseAddress	Release { get; set; }
			public bool				Exists { get; set; }
		}

		public class Seed
		{
			public IPAddress	IP { get; set; }
			public SeedStatus	Status { get; set; }
		}

		public class Hub
		{
			public IPAddress				IP { get; set; }
			public HubStatus				Status { get; set; }
			public IEnumerable<IPAddress>	Seeds { get; set; }
		}

		public Distributive				Distributive { get; set; }
		public long						Length { get; set; }
		public long						CompletedLength { get; set; }
		public IEnumerable<Dependency>	DependenciesRecursive { get; set; }
		public IEnumerable<Hub>			Hubs { get; set; }
		public IEnumerable<Seed>		Seeds { get; set; }
	}

	public class Download
	{
		public const long DefaultPieceLength = 512 * 1024;

		public class Piece
		{
			public Peer				Seed;
			public Task				Task;
			public int				I = -1;
			public long				Length => I * DefaultPieceLength + DefaultPieceLength > Download.Length ? Download.Length % DefaultPieceLength : DefaultPieceLength;
			public long				Offset => I * DefaultPieceLength;
			public MemoryStream		Data = new MemoryStream();
			public bool				Succeeded => Data.Length == Length;
			Download				Download;
			Core					Core => Download.Core;

			public Piece(Download download, Peer peer, int piece)
			{
				Download = download;
				Seed = peer;
				I = piece;

				Task = Task.Run(() =>	
								{
									try
									{
										Download.Core.Connect(Seed, download.Workflow);
										
										int e = 0;

										while(Data.Position < Length)
										{
											try
											{
												var d = Seed.DownloadRelease(Download.Release, Download.Distributive, Offset + Data.Position, Length - Data.Position).Data;
												Data.Write(d, 0, d.Length);
											}
											catch(RdcException)
											{
												e++;
												
												if(e > 3)
													break;
											}
										}
									}
									catch(ConnectionFailedException)
									{
									}
									catch(OperationCanceledException)
									{
									}
								}, 
								Download.Workflow.Cancellation.Token);
				}
		}

		public ReleaseAddress						Release;
		public Distributive							Distributive { get; protected set; }
		public long									Length { get; protected set; }
		public bool									Succeeded => Downloaded && DependenciesRecursiveFound && DependenciesRecursiveCount == DependenciesRecursiveSuccesses;
		public long									CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																		- (CompletedPieces.Any(i => i.I == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																		+ CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);
		public int									DependenciesRecursiveCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesRecursiveCount);
		public bool									DependenciesRecursiveFound => Manifest != null && Dependencies.All(i => i.DependenciesRecursiveFound);
		public int									DependenciesRecursiveSuccesses => Dependencies.Count(i => i.Succeeded) + Dependencies.Sum(i => i.DependenciesRecursiveSuccesses);
		public object								Lock = new object();

		internal Core								Core;
		internal Workflow							Workflow;
		bool										Downloaded;
		List<Piece>									CurrentPieces = new();
		List<Piece>									CompletedPieces = new();
		public List<Hub>							Hubs = new();
		public Dictionary<IPAddress, SeedStatus>	Seeds = new();
		public List<Download>						Dependencies = new();
		public IEnumerable<Download>				DependenciesRecursive => Dependencies.Union(Dependencies.SelectMany(i => i.DependenciesRecursive)).DistinctBy(i => i.Release);
		byte[]										Hash;
		int											PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		Manifest									Manifest;
		Task										Task;

		public Download(Core core, ReleaseAddress release, Workflow workflow)
		{
			Core = core;
			Release = release;
			Workflow = workflow;

			int hubsgoodmax = 8;

			Task = Task.Run(() =>
							{
								ReleaseHistoryResponse his = null;

								while(!workflow.IsAborted)
								{
									his = Core.Call(Role.Base, c => c.GetReleaseHistory(release.Realization), workflow);

									if(his.Releases.Any(i => i.Address == Release))
										break;
									else
										Thread.Sleep(100);
								}
				
								Piece j;

								while(!workflow.IsAborted)
								{
									Task[] tasks;
									Hub hlast = null;

									lock(Lock)
									{
										for(int i = 0; i < hubsgoodmax - Hubs.Count(i => i.Status == HubStatus.Estimating); i++)
										{
											var h = Core.FindBestPeer(Role.Hub, Hubs.Select(i => i.Peer).ToHashSet());
	
											if(h != null)
											{
												Workflow.Log.Report(this, "Hub found", $"for {release}, {h}");

												hlast = new Hub(this, h);
												Hubs.Add(hlast);
											}
											else
												break;
										}

										foreach(var i in Hubs.Where(i => i.Seeds.Count < 16 && DateTime.UtcNow - i.Called > TimeSpan.FromSeconds(5)))
										{
											i.Refresh();
										}

										if(Length == 0 || (PiecesTotal - CompletedPieces.Count - CurrentPieces.Count > 0 && CurrentPieces.Count < 8))
										{
											var s = Seeds.FirstOrDefault(i => i.Value != SeedStatus.Bad && !CurrentPieces.Any(j => j.Seed.IP.Equals(i.Key)));
											
											if(s.Key != null)
											{
												if(Manifest == null)
												{
													try
													{
														Manifest = core.Call(s.Key, p => p.GetManifest(release).Manifest, workflow);
														Manifest.Release = release;

														if(!Manifest.GetOrCalcHash().SequenceEqual(his.Releases.First(i => i.Address == release).Manifest))
														{
															Manifest = null;
															continue;
														}
													}
													catch(Exception ex) when (ex is ConnectionFailedException || ex is RdcException)
													{
														Seeds[s.Key] = SeedStatus.Bad;
														continue;
													}
															
													Core.Filebase.DetermineDelta(his.Releases, Manifest, out Distributive d, out List<Dependency> deps);

													Distributive = d;

													deps	= d == Distributive.Incremental ? deps : Manifest.CompleteDependencies.ToList();
													Hash	= d == Distributive.Complete ? Manifest.CompleteHash : Manifest.IncrementalHash;
													Length	= d == Distributive.Complete ? Manifest.CompleteLength : Manifest.IncrementalLength;
								
													lock(core.Lock)
													{
														foreach(var i in deps)
														{
															var dd = core.DownloadRelease(i.Release, workflow);
																
															if(dd != null)
															{
																Dependencies.Add(dd);
															}
														}
													}
												}
												
												CurrentPieces.Add(new Piece(this, 
																	 Core.GetPeer(s.Key), 
																	 Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.I == i) && !CurrentPieces.Any(j => j.I == i))));
											}
											else
											{
												//Thread.Sleep(1);
												//
												//foreach(var h in Hubs.Where(i => i.Status == HubStatus.Estimating && i.Seeds.Any()))
												//{
												//	if(h.Seeds.All(i => Seeds[i] == SeedStatus.Bad)) /// all seeds are bad
												//	{
												//		h.Status = HubStatus.Bad;
												//	}
												//}
												//
												//if(Seeds.Any() && Seeds.All(i => i.Value == SeedStatus.Bad)) /// no good seeds found
												//{
												//	if(Hubs.Count(i => i.Status == HubStatus.Estimating) < hubsgoodmax && hlast == null) /// no more hubs, total restart
												//	{
												//		//Hubs.Clear();
												//		//Seeds.Clear();
												//	}
												//}
											}
										}
									
										tasks = CurrentPieces.Select(i => i.Task).ToArray();

										if(tasks.Length == 0)
										{
											continue;
										}
									}

									var ti = Task.WaitAny(tasks, workflow.Cancellation.Token);

									lock(Lock)
									{	
										j = CurrentPieces.Find(i => i.Task == tasks[ti]);
										
										CurrentPieces.Remove(j);

										if(j.Succeeded)
										{
											Seeds[j.Seed.IP] = SeedStatus.Good;

											lock(Core.Lock)
												Core.Filebase.WritePackage(Release, Distributive, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												lock(Core.Lock)
													if(Core.Filebase.GetHash(Release, Distributive).SequenceEqual(Hash))
													{	
														goto end;
													}
													else
													{
														CurrentPieces.Clear();
														CompletedPieces.Clear();
														Hubs.Clear();
														Seeds.Clear();
													}
											}

											///if(!d.HubsSeeders[h].Contains(s)) /// this hub gave a good seeder
											///	d.HubsSeeders[h].Add(s);
										}
										else
										{	
											Seeds[j.Seed.IP] = SeedStatus.Bad;
										}
									}
								}

							end:

								lock(Core.Lock)
								{
									Core.Filebase.AddRelease(Release, Manifest);

									var hubs = Hubs.Where(h => Seeds.Any(s => s.Value == SeedStatus.Good && h.Seeds.Any(ip => ip.Equals(s.Key)))).Select(i => i.Peer);

									foreach(var h in hubs)
										h.HubRank++;

									var seeds = CompletedPieces.Select(i => i.Seed);

									foreach(var h in seeds)
										h.SeedRank++;

									his.Peer.ChainRank++;

									Core.UpdatePeers(seeds.Union(hubs).Union(new[]{his.Peer}).Distinct());
									Core.Downloads.Remove(this);

									Downloaded = true;
								}
									
							},
							workflow.Cancellation.Token);
		}

		public override string ToString()
		{
			return Release.ToString();
		}

		//public Job AddCompleted(int i)
		//{
		//	var j = new Job(this, null);
		//	j.Piece = i;
		//	j.CurrentLength = j.PieceLength;
		//	Jobs.Add(j);
		//	return j;
		//}
	}
}
