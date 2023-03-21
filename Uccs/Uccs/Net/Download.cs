using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Contracts.QueryHandlers.MultiCall;

namespace UC.Net
{
	public class DownloadStatus
	{
		public Distributive	Distributive { get; set; }
		public long			Length { get; set; }
		public long			CompletedLength { get; set; }
		public int			DependenciesCount { get; set; }
		public bool			AllDependenciesFound { get; set; }
		public int			DependenciesSuccessful { get; set; }
	}

	public class Download
	{
		public const long DefaultPieceLength = 65536;

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
												Data.Write(d, (int)Data.Position, d.Length);
												break;
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

		public enum SeedStatus
		{
			Null, Good, Bad
		}

		public enum HubStatus
		{
			Null, Estimating, Useless
		}

		class Hub
		{
			public Peer					Peer;
			public List<IPAddress>		Seeds = new();
			public HubStatus			Status = HubStatus.Estimating;

			public Hub(Peer peer)
			{
				Peer = peer;
			}
		}

		public ReleaseAddress				Release;
		public Distributive					Distributive { get; protected set; }
		public long							Length { get; protected set; }
		public bool							Successful => Downloaded && AllDependenciesFound && DependenciesCount == DependenciesSuccessfulCount;
		public long							CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																- (CompletedPieces.Any(i => i.I == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																+ Pieces.Sum(i => i.Data != null ? i.Data.Length : 0);
		public int							DependenciesCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesCount);
		public bool							AllDependenciesFound => Manifest != null && Dependencies.All(i => i.AllDependenciesFound);
		public int							DependenciesSuccessfulCount => Dependencies.Count(i => i.Successful) + Dependencies.Sum(i => i.DependenciesSuccessfulCount);
		public object						Lock = new object();

		Core								Core;
		Workflow							Workflow;
		bool								Downloaded;
		List<Piece>							Pieces = new();
		List<Hub>							Hubs = new();
		Dictionary<IPAddress, SeedStatus>	Seeds = new();
		List<Download>						Dependencies = new();
		byte[]								Hash;
		int									PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		List<Piece>							CompletedPieces = new();
		Manifest							Manifest;
		Task								Task;

		public Download(Core core, ReleaseAddress release, Workflow workflow)
		{
			Core = core;
			Release = release;
			Workflow = workflow;

			int hubsgoodmax = 8;

			Task = Task.Run(() =>
							{
								ReleaseHistoryResponse his;

								while(true)
								{
									workflow.ThrowIfAborted();

									his = Core.Call(Role.Base, c => c.GetReleaseHistory(release, false), workflow);

									if(his.Releases.Any(i => i.Release == release))
										break;
									else
										Thread.Sleep(100);
								}
				
								Piece j;

								while(true)
								{
									workflow.ThrowIfAborted();

									Task[] tasks;
									Hub hlast = null;

									lock(Lock)
									{
										for(int i = 0; i < hubsgoodmax - Hubs.Count(i => i.Status == HubStatus.Estimating); i++)
										{
											var p = Core.FindBestPeer(Role.Hub, Hubs.Select(i => i.Peer).ToHashSet());
	
											if(p != null)
											{
												hlast = new Hub(p);
												Hubs.Add(hlast);
	
												Task.Run(() =>	{
																	try
																	{
																		var lr = Core.Call(hlast.Peer.IP, p => p.LocateRelease(release, 16), workflow);

																		lock(Lock)
																		{
																			hlast.Seeds = lr.Seeders.ToList();

																			foreach(var s in lr.Seeders)
																			{
																				if(!Seeds.ContainsKey(s))
																					Seeds.Add(s, SeedStatus.Null);
																			}
																		}
																	}
																	catch(Exception ex) when (ex is ConnectionFailedException || ex is RdcException)
																	{
																	}
																	catch(OperationCanceledException)
																	{
																	}
																},
																workflow.Cancellation.Token);
											}
										}

										if((Length == 0 ? 1 : (PiecesTotal - CompletedPieces.Count - Pieces.Count)) > 0 && Pieces.Count < 8)
										{
											var s = Seeds.FirstOrDefault(i => i.Value != SeedStatus.Bad && !Pieces.Any(j => j.Seed.IP.Equals(i.Key)));
											
											if(s.Key != null)
											{
												if(Manifest == null)
												{
													try
													{
														Manifest = core.Call(s.Key, p => p.GetManifest(release).Manifests.First(), workflow);
														Manifest.Release = release;

														if(!Manifest.GetOrCalcHash().SequenceEqual(his.Releases.First(i => i.Release == release).Manifest))
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
															if(!core.Downloads.Any(j => j.Release == i.Release))
															{
																var dd = core.DownloadRelease(i.Release, workflow);
																
																if(dd != null)
																{
																	Dependencies.Add(dd);
																}
															}
														}
													}
												}
												
												Pieces.Add(new Piece(this, 
																	 Core.GetPeer(s.Key), 
																	 Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.I == i) && !Pieces.Any(j => j.I == i))));
											}
											else
											{
												Thread.Sleep(1);

												foreach(var h in Hubs.Where(i => i.Status == HubStatus.Estimating && i.Seeds.Any()))
												{
													if(h.Seeds.All(i => Seeds[i] == SeedStatus.Bad)) /// all seeds are bad
													{
														h.Status = HubStatus.Useless;
													}
												}

												if(Seeds.All(i => i.Value == SeedStatus.Bad)) /// no good seeds found
												{
													if(Hubs.Count(i => i.Status == HubStatus.Estimating) < hubsgoodmax && hlast == null) /// no more hubs, total restart
													{
														Hubs.Clear();
														Seeds.Clear();
													}
												}
											}
										}
									
										tasks = Pieces.Select(i => i.Task).ToArray();

										if(tasks.Length == 0)
										{
											continue;
										}
									}

									var ti = Task.WaitAny(tasks, workflow.Cancellation.Token);

									lock(Lock)
									{	
										j = Pieces.Find(i => i.Task == tasks[ti]);
										
										Pieces.Remove(j);

										if(j.Succeeded)
										{
											lock(Core.Lock)
												Core.Filebase.WritePackage(Release, Distributive, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												Seeds[j.Seed.IP] = SeedStatus.Good;

												if(Core.Filebase.GetHash(Release, Distributive).SequenceEqual(Hash))
												{	
													Core.Filebase.AddRelease(Release, Manifest);

													//Core.DeclarePackage(new[]{Package}, Workflow);

													var hubs = Hubs.Where(h => Seeds.Any(s => s.Value == SeedStatus.Good && h.Seeds.Any(ip => ip.Equals(s.Key)))).Select(i => i.Peer);

													foreach(var h in hubs)
														h.HubRank++;

													var seeds = CompletedPieces.Select(i => i.Seed);

													foreach(var h in seeds)
														h.SeedRank++;

													his.Peer.ChainRank++;

													Core.UpdatePeers(seeds.Union(hubs).Union(new[]{his.Peer}).Distinct());

													//while(Core.Running && Dependencies.Any(i => !i.Successful))
													//{
													//	Thread.Sleep(1);
													//	workflow.ThrowIfAborted();
													//}
												
													Downloaded = true;

													lock(Core.Lock)
														Core.Downloads.Remove(this);

													return;
												}
												else
												{
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
							},
							workflow.Cancellation.Token);
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
