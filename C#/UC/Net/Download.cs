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
									var timeout = Download.Workflow.CreateNested();

									try
									{
										if(!Settings.Dev.DisableTimeouts)
											timeout.Cancellation.CancelAfter(Core.Timeout);
												
										Download.Core.Connect(Peer, timeout);
												
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
		Task										Task;
		object										Lock = new object();
		public long									CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																		- (CompletedPieces.Any(i => i.Piece == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																		+ Jobs.Sum(i => i.Data != null ? i.Data.Length : 0);

		public Download(Core core, ReleaseAddress release, Workflow workflow)
		{
			Core = core;
			Workflow = workflow;

			Task = Task.Run(() =>
							{
								var r = Core.Call(Role.Chain, workflow, c => c.QueryRelease(release, release.Version, VersionQuery.Exact, null, true));
								var his = Core.Call(Role.Chain, workflow, c => c.GetReleaseHistory(release, true));

								Core.Filebase.DetermineDelta(his.Manifests, r.Results.First().Manifest.Address, out Distributive d, out List<ReleaseAddress> deps);

								var qrr = r.Results.First();

								Package = new PackageAddress(release, d);
		
								Length = d switch {	Distributive.Complete => qrr.Manifest.CompleteSize,
													Distributive.Incremental => qrr.Manifest.IncrementalSize,
													_ => throw new ArgumentException("Wrong Distribution value")};
										
								Hash = d switch {	Distributive.Complete => qrr.Manifest.CompleteHash,
													Distributive.Incremental => qrr.Manifest.IncrementalHash, 
													_ => throw new ArgumentException("Wrong Distribution value")};
								
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
								
								Job j;

								while(Core.Running && !workflow.IsAborted)
								{
									Task[] tasks;

									Thread.Sleep(1);

									lock(Lock)
									{
										for(int i=0; i<8 - Hubs.Count; i++)
										{
											var h = Core.FindBestPeer(Role.Hub, Hubs.Keys.ToHashSet());
	
											if(h != null)
											{
												Hubs[h] = new();
	
												Task.Run(() =>	{
																	var timeout = Workflow.CreateNested();
	
																	//LocatePackageResponse lp = null;
		
																	try
																	{
																		if(!Settings.Dev.DisableTimeouts)
																			timeout.Cancellation.CancelAfter(Core.Timeout);
													
																		Core.Connect(h, timeout);
		
																		var lp = h.LocatePackage(Package, 16);

																		lock(Lock)
																		{
																			Hubs[h] = lp.Seeders.ToList();

																			foreach(var s in lp.Seeders)
																			{
																				if(!Seeders.ContainsKey(s))
																					Seeders[s] = SeederResult.Null;
																			}
																		}
																	}
																	catch(ConnectionFailedException)
																	{
																	}
																	catch(DistributedCallException)
																	{
																	}
																	catch(OperationCanceledException)
																	{
																	}
																},
																workflow.Cancellation.Token);
											}
											else
												break;
										}

										if(Jobs.Count < Math.Min(8, PiecesTotal - CompletedPieces.Count))
										{
											var s = Seeders.FirstOrDefault(i => i.Value != SeederResult.Bad && !Jobs.Any(j => j.Peer.IP.Equals(i.Key)));
											
											if(s.Key != null)
											{
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
												Core.Filebase.Write(Package, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												Seeders[j.Peer.IP] = SeederResult.Good;

												if(Core.Filebase.GetHash(Package).SequenceEqual(Hash))
												{
													Core.DeclarePackage(new[]{Package}, Workflow);

													var hubs = Hubs.Where(h => Seeders.Any(s => s.Value == SeederResult.Good && h.Value.Any(ip => ip.Equals(s.Key)))).Select(i => i.Key);

													foreach(var h in hubs)
														h.HubRank++;

													var seeds = CompletedPieces.Select(i => i.Peer);

													foreach(var h in seeds)
														h.SeedRank++;

													r.Peer.ChainRank++;

													Core.UpdatePeers(seeds.Union(hubs).Union(new[]{r.Peer}).Distinct());

													while(Dependencies.Any(i => !i.Completed) && Core.Running && !workflow.IsAborted)
														Thread.Sleep(1);
												
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
