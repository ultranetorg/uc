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
		public long		Length { get; set; }
		public long		CompletedLength { get; set; }
		public int		DependenciesCount { get; set; }
		public bool		AllDependenciesFound { get; set; }
		public int		DependenciesSuccessful { get; set; }
	}

	public class Download
	{
		public const long DefaultPieceLength = 65536;

		public class Job
		{
			public Peer				Seed;
			public Task				Task;
			public int				Piece = -1;
			public long				Length => Piece * DefaultPieceLength + DefaultPieceLength > Download.Length ? Download.Length % DefaultPieceLength : DefaultPieceLength;
			public long				Offset => Piece * DefaultPieceLength;
			public MemoryStream		Data = new MemoryStream();
			public bool				Succeeded => Data.Length == Length;
			Download				Download;
			Core					Core => Download.Core;

			public Job(Download download, Peer peer, int piece)
			{
				Download = download;
				Seed = peer;
				Piece = piece;

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
												var d = Seed.DownloadPackage(Download.Package, Offset + Data.Position, Length - Data.Position).Data;
												Data.Write(d, (int)Data.Position, d.Length);
												break;
											}
											catch(DistributedCallException)
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
		public PackageAddress				Package { get; protected set; }
		public long							Length { get; protected set; }
		public bool							Successful => Downloaded && AllDependenciesFound && DependenciesCount == DependenciesSuccessful;
		public long							CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																- (CompletedPieces.Any(i => i.Piece == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																+ Jobs.Sum(i => i.Data != null ? i.Data.Length : 0);
		public int							DependenciesCount => Dependencies.Count + Dependencies.Sum(i => i.DependenciesCount);
		public bool							AllDependenciesFound => Manifest != null && Dependencies.All(i => i.AllDependenciesFound);
		public int							DependenciesSuccessful => Dependencies.Count(i => i.Successful) + Dependencies.Sum(i => i.DependenciesSuccessful);

		Core								Core;
		Workflow							Workflow;
		bool								Downloaded;
		List<Job>							Jobs = new();
		List<Hub>							Hubs = new();
		Dictionary<IPAddress, SeedStatus>	Seeds = new();
		List<Download>						Dependencies = new();
		byte[]								Hash;
		int									PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		List<Job>							CompletedPieces = new();
		Manifest							Manifest;
		Task								Task;
		object								Lock = new object();

		public Download(Core core, ReleaseAddress release, Workflow workflow)
		{
			Core = core;
			Release = release;
			Workflow = workflow;

			int hubsgoodmax = 8;

			Task = Task.Run(() =>	
							{
								var his = Core.Call(Role.Chain, c => c.GetReleaseHistory(release, false), workflow);
				
								Job j;

								while(Core.Running)
								{
									Thread.Sleep(1);
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
																	catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException)
																	{
																	}
																},
																workflow.Cancellation.Token);
											}
										}

										if((Length == 0 ? 1 : (PiecesTotal - CompletedPieces.Count - Jobs.Count)) > 0 && Jobs.Count < 8)
										{
											var s = Seeds.FirstOrDefault(i => i.Value != SeedStatus.Bad && !Jobs.Any(j => j.Seed.IP.Equals(i.Key)));
											
											if(s.Key != null)
											{
												if(Manifest == null)
												{
													try
													{
														Manifest = core.Call(s.Key, p => p.GetManifest(release).Manifests.First(), workflow);
														Manifest.Release = release;

														if(!Manifest.GetOrCalcHash().SequenceEqual(his.Registrations.First(i => i.Release == release).Manifest))
														{
															Manifest = null;
															continue;
														}
													}
													catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException)
													{
														Seeds[s.Key] = SeedStatus.Bad;
														continue;
													}
															
													Core.Filebase.DetermineDelta(his.Registrations, Manifest, out Distributive d, out List<ReleaseAddress> deps);

													Package = new PackageAddress(release, d);

													deps	= d == Distributive.Incremental ? deps : Manifest.CompleteDependencies.ToList();
													Hash	= d == Distributive.Complete ? Manifest.CompleteHash : Manifest.IncrementalHash;
													Length	= d == Distributive.Complete ? Manifest.CompleteLength : Manifest.IncrementalLength;
								
													lock(core.Lock)
													{
														foreach(var i in deps)
														{
															if(!core.Downloads.Any(j => j.Release == i))
															{
																Dependencies.Add(core.DownloadRelease(i, workflow));
															}
														}
													}
												}
												
												Jobs.Add(new Job(this, 
																 Core.GetPeer(s.Key), 
																 Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.Piece == i) && !Jobs.Any(j => j.Piece == i))));
											}
											else
											{
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
									
										tasks = Jobs.Select(i => i.Task).ToArray();

										if(tasks.Length == 0)
										{
											continue;
										}
									}

									var ti = Task.WaitAny(tasks, workflow.Cancellation.Token);

									lock(Lock)
									{	
										j = Jobs.Find(i => i.Task == tasks[ti]);
										
										Jobs.Remove(j);

										if(j.Succeeded)
										{
											lock(Core.Lock)
												Core.Filebase.WritePackage(Package, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												Seeds[j.Seed.IP] = SeedStatus.Good;

												if(Core.Filebase.GetHash(Package).SequenceEqual(Hash))
												{	
													Core.Filebase.AddManifest(release, Manifest);

													Core.DeclarePackage(new[]{Package}, Workflow);

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
