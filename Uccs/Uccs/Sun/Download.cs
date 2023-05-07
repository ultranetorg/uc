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
	public enum HubStatus
	{
		Null, Estimating, Bad
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
			public int			Failures { get; set; }
			public int			Succeses { get; set; }
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

		public class Hub
		{
			public Peer					Peer;
			public Seed[]				Seeds = new Seed[]{};
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

											Seeds = lr.Seeders.Where(i => !Download.Seeds.Any(j => j.IP.Equals(i))).Select(i => new Seed {IP = i}).ToArray();

											Download.Seeds.AddRange(Seeds);
										}

										Called = DateTime.UtcNow;
									}
									catch(Exception ex) when (ex is ConnectionFailedException || ex is RdcNodeException || ex is RdcEntityException)
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

		public class Piece
		{
			public Seed				Seed;
			public Task				Task;
			public int				I = -1;
			public long				Length => I * DefaultPieceLength + DefaultPieceLength > Download.Length ? Download.Length % DefaultPieceLength : DefaultPieceLength;
			public long				Offset => I * DefaultPieceLength;
			public MemoryStream		Data = new MemoryStream();
			public bool				Succeeded => Data.Length == Length;
			Download				Download;
			Core					Core => Download.Core;

			public Piece(Download download, Seed peer, int piece)
			{
				Download = download;
				Seed = peer;
				I = piece;

				Task = Task.Run(() =>	{
											try
											{
												Download.Core.Connect(Seed.Peer, download.Workflow);

												while(Data.Position < Length)
												{
													var d = Seed.Peer.DownloadRelease(Download.Release, Download.Distributive, Offset + Data.Position, Length - Data.Position).Data;
													Data.Write(d, 0, d.Length);
												}
											}
											catch(Exception ex) when(ex is ConnectionFailedException || ex is OperationCanceledException || ex is RdcNodeException || ex is RdcEntityException)
											{
											}
										}, 
										Download.Workflow.Cancellation.Token);
				}
		}

		public class Seed
		{
			public IPAddress	IP;
			public Peer			Peer;
			public DateTime		Failed;
			public int			Failures;
			public int			Succeses;
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
		public List<Seed>							Seeds = new();
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

										foreach(var i in Hubs.Where(i => i.Seeds.Length < 16 && DateTime.UtcNow - i.Called > TimeSpan.FromSeconds(5)))
										{
											i.Refresh();
										}

										if(Length == 0 || (PiecesTotal - CompletedPieces.Count - CurrentPieces.Count > 0 && CurrentPieces.Count < 8))
										{
											var s = Seeds.OrderBy(i => i.Peer == null).FirstOrDefault(i =>	i.Peer == null ||  /// new candidate
																											i.Peer != null && i.Failed == DateTime.MinValue || /// succeeded previously
																											(DateTime.UtcNow - i.Failed) > TimeSpan.FromSeconds(5)  &&  /// bad node
																											CurrentPieces.All(j => j.Seed != i));
											
											if(s != null)
											{
												if(s.Peer == null)
												{
													s.Peer = core.GetPeer(s.IP);
												}

												if(Manifest == null)
												{
													try
													{
														Manifest = core.Call(s.Peer.IP, p => p.GetManifest(release).Manifest, workflow);
														Manifest.Release = release;

														if(!Manifest.GetOrCalcHash().SequenceEqual(his.Releases.First(i => i.Address == release).Manifest))
														{
															Manifest = null;
															continue;
														}
													}
													catch(Exception ex) when(ex is ConnectionFailedException || ex is RdcNodeException || ex is RdcEntityException)
													{
														s.Failed = DateTime.UtcNow;
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
												
												CurrentPieces.Add(new Piece(this, s, Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.I == i) && !CurrentPieces.Any(j => j.I == i))));
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
											j.Seed.Failures++;
											j.Seed.Failed = DateTime.MinValue;

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
											j.Seed.Succeses++;
											j.Seed.Failed = DateTime.UtcNow;
										}
									}
								}

							end:

								lock(Core.Lock)
								{
									Core.Filebase.AddRelease(Release, Manifest);

									var hubs = Hubs.Where(h => h.Seeds.Any(s => s.Peer != null && s.Failed == DateTime.MinValue)).Select(i => i.Peer);

									foreach(var h in hubs)
										h.HubRank++;

									var seeds = CompletedPieces.Select(i => i.Seed.Peer);

									foreach(var h in seeds)
										h.SeedRank++;

									his.Peer.BaseRank++;

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
