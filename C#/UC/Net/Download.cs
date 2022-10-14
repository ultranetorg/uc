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
	public enum SeederResult
	{
		Null, Good, Bad
	}

	public class Download
	{
		public const long DefaultPieceLength = 65536;

		public class Job
		{
			public Peer				Peer;
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
				Peer = peer;
				Piece = piece;

				Task = Task.Run(() =>	
								{
									try
									{
										Download.Core.Connect(Peer, download.Workflow);
												
										while(Data.Length < Length)
										{
											for(int e=0; e<3; e++)
											{
												try
												{
													var d = Peer.DownloadPackage(Download.Package, Offset + Data.Length, Length - Data.Length).Data;
													Data.Write(d, 0, d.Length);
													break;
												}
												catch(DistributedCallException)
												{
												}
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

		public Core									Core;
		public Workflow								Workflow;
		public PackageAddress						Package;
		public List<Job>							Jobs = new();
		public Dictionary<Peer, List<IPAddress>>	Hubs = new();
		public Dictionary<IPAddress, SeederResult>	Seeders = new();
		public List<Download>						Dependencies = new();
		public byte[]								Hash;
		public long									Length;
		public int									PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		public bool									Completed;
		public bool									Successful; 
		public List<Job>							CompletedPieces = new();
		Manifest									Manifest;
		Task										Task;
		object										Lock = new object();
		public long									CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																		- (CompletedPieces.Any(i => i.Piece == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																		+ Jobs.Sum(i => i.Data != null ? i.Data.Length : 0);

		public Download(Core core, ReleaseAddress release, Workflow workflow)
		{
			Core = core;
			Workflow = workflow;

			Task = Task.Run(() =>	{
										var his = Core.Call(Role.Chain, c => c.GetReleaseHistory(release, true), workflow);
				
										Job j;

										while(Core.Running)
										{
											Thread.Sleep(1);
											workflow.ThrowIfAborted();

											Task[] tasks;

											lock(Lock)
											{
												for(int i = 0; i < 8 - Hubs.Count; i++)
												{
													var h = Core.FindBestPeer(Role.Hub, Hubs.Keys.ToHashSet());
	
													if(h != null)
													{
														Hubs[h] = new();
	
														Task.Run(() =>	{
																			try
																			{
																				Core.Connect(h, workflow);
		
																				var lp = h.LocateRelease(release, 16);

																				lock(Lock)
																				{
																					if(!Hubs.ContainsKey(h))
																						Hubs.Add(h, lp.Seeders.ToList());

																					foreach(var s in lp.Seeders)
																					{
																						if(!Seeders.ContainsKey(s))
																							Seeders.Add(s, SeederResult.Null);
																					}
																				}
																			}
																			catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException || ex is OperationCanceledException)
																			{
																			}
																		},
																		workflow.Cancellation.Token);
													}
													else
														break;
												}

												if(Jobs.Count < (Length == 0 ? 1 : Math.Min(8, PiecesTotal - CompletedPieces.Count)))
												{
													var s = Seeders.FirstOrDefault(i => i.Value != SeederResult.Bad && !Jobs.Any(j => j.Peer.IP.Equals(i.Key)));
											
													if(s.Key != null)
													{
														if(Manifest == null)
														{
															try
															{
																Manifest = core.Call(s.Key, p => p.GetManifest(release).Manifests.First(), workflow);

																if(!Manifest.GetOrCalcHash().SequenceEqual(his.Registrations.First(i => i.Release == release).Manifest))
																{
																	Manifest = null;
																	continue;
																}
															}
															catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException)
															{
																continue;
															}
															
															Core.Filebase.DetermineDelta(his.Registrations, release, out Distributive d, out List<ReleaseAddress> deps);

															Package = new PackageAddress(release, d);

															deps	= d == Distributive.Incremental ? deps : Manifest.CompleteDependencies.ToList();
															Hash	= d == Distributive.Complete ? Manifest.CompleteHash : Manifest.IncrementalHash;
															Length	= d == Distributive.Complete ? Manifest.CompleteLength : Manifest.IncrementalLength;
								
															lock(core.Lock)
															{
																foreach(var i in deps)
																{
																	if(!core.Downloads.Any(j => (ReleaseAddress)j.Package == i))
																	{
																		Dependencies.Add(core.DownloadRelease(i, workflow));
																	}
																}
															}
														}
														
														Add(Core.GetPeer(s.Key), Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.Piece == i)));
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
														Seeders[j.Peer.IP] = SeederResult.Good;

														if(Core.Filebase.GetHash(Package).SequenceEqual(Hash))
														{	
															Core.Filebase.AddManifest(release, Manifest);

															Core.DeclarePackage(new[]{Package}, Workflow);

															var hubs = Hubs.Where(h => Seeders.Any(s => s.Value == SeederResult.Good && h.Value.Any(ip => ip.Equals(s.Key)))).Select(i => i.Key);

															foreach(var h in hubs)
																h.HubRank++;

															var seeds = CompletedPieces.Select(i => i.Peer);

															foreach(var h in seeds)
																h.SeedRank++;

															his.Peer.ChainRank++;

															Core.UpdatePeers(seeds.Union(hubs).Union(new[]{his.Peer}).Distinct());

															while(Core.Running && Dependencies.Any(i => !i.Completed))
															{
																Thread.Sleep(1);
																workflow.ThrowIfAborted();
															}
												
															Successful = true;
														}
														else
														{
														///	throw new 
														}

														Completed = true;

														return;
													}

													///if(!d.HubsSeeders[h].Contains(s)) /// this hub gave a good seeder
													///	d.HubsSeeders[h].Add(s);
												}
												else
												{	
													Seeders[j.Peer.IP] = SeederResult.Bad;
												}
											}
										}
									},
									workflow.Cancellation.Token);
		}

		public Job Add(Peer peer, int i)
		{
			var j = new Job(this, peer, i);
			Jobs.Add(j);

			return j;
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
