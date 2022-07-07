using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace UC.Net
{
	public enum DownloadStage
	{
		Null, Working, Completed
	}

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
			public DownloadStage	Stage;
			public MemoryStream		Data = new MemoryStream();
			public bool				Succeeded => Data.Length == Length;
			Download				Download;
			Core					Core => Download.Core;

			public Job(Download download, Peer peer, int piece)
			{
				Download = download;
				Peer = peer;
				Piece = piece;
				Stage = DownloadStage.Working;

				Task = Task.Run(() =>	
								{
									var timeout = Download.Flowvizor.CreateNested();

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
												catch(RemoteCallException)
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
									
									Stage = DownloadStage.Completed;
								}, 
								Download.Flowvizor.Cancellation.Token);
				}
		}

		public Core									Core;
		public Flowvizor							Flowvizor;
		public PackageAddress						Package;
		public List<Job>							Jobs = new();
		public Dictionary<Peer, List<IPAddress>>	Hubs = new();
		public Dictionary<IPAddress, SeederResult>	Seeders = new();
		public byte[]								Hash;
		public long									Length;
		public int									PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		public bool									Completed;
		public bool									Successful; 
		public HashSet<int>							CompletedPieces = new();
		Task										Task;
		public long									CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																		- (CompletedPieces.Contains(PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																		+ Jobs.Sum(i => i.Data != null ? i.Data.Length : 0);

		public Download(Core core, Flowvizor vizor, PackageAddress package)
		{
			Core = core;
			Flowvizor = vizor;
			Package = package;

			Task = Task.Run(() =>
							{
								var qrrp = Core.Call(Role.Chain, vizor, c => c.QueryRelease(package, VersionQuery.Exact, "", true));
		
								Length = qrrp.Manifests.First().GetInt64(package.Distribution switch {	Distribution.Complete => ReleaseManifest.CompleteSizeField, 
																										Distribution.Incremental => ReleaseManifest.IncrementalSizeField, 
																										_ =>  throw new RequirementException("Wrong Distribution value") });
										
								Hash = qrrp.Manifests.First().Get<byte[]>(package.Distribution switch {	Distribution.Complete => ReleaseManifest.CompleteHashField, 
																										Distribution.Incremental => ReleaseManifest.IncrementalHashField, 
																										_ =>  throw new RequirementException("Wrong Distribution value") });
								
								Download.Job j ;

								while(Core.Running && !vizor.Cancellation.IsCancellationRequested && !vizor.IsAborted)
								{
									Task[] tasks;

									Thread.Sleep(1);

									lock(Core.Lock)
									{
										for(int i=0; i<8 - Hubs.Count; i++)
										{
											var h = Core.FindBestPeer(Role.Hub, Hubs.Keys.ToHashSet());
	
											if(h != null)
											{
												Hubs[h] = null;
	
												var t = Task.Run(() =>	
																{
																	var timeout = Flowvizor.CreateNested();
	
																	LocatePackageResponse lp = null;
		
																	try
																	{
																		if(!Settings.Dev.DisableTimeouts)
																			timeout.Cancellation.CancelAfter(Core.Timeout);
													
																		Core.Connect(h, timeout);
		
																		lp = h.LocatePackage(Package, 16);

																		lock(Core.Lock)
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
																	catch(RemoteCallException)
																	{
																	}
																	catch(OperationCanceledException)
																	{
																	}
																});
	
											}
											else
												break;
										}

										if(Jobs.Count(i => i.Stage == DownloadStage.Working) < Math.Min(8, PiecesTotal - CompletedPieces.Count))
										{
											var s = Seeders.FirstOrDefault(i => i.Value != SeederResult.Bad && !Jobs.Any(j => j.Peer.IP.Equals(i.Key)));
											
											if(s.Key != null)
											{
												Add(Core.GetPeer(s.Key), Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Contains(i)));
											}
										}
									
										tasks = Jobs.Select(i => i.Task).ToArray();

										if(tasks.Length == 0)
										{
											continue;
										}
									}

									var ti = Task.WaitAny(tasks, vizor.Cancellation.Token);

									lock(Core.Lock)
									{	
										j = Jobs.Find(i => i.Task == tasks[ti]);
										
										Jobs.Remove(j);

										if(j.Succeeded)
										{
											Core.Filebase.Write(package, j.Offset, j.Data.ToArray());
											CompletedPieces.Add(j.Piece);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												Seeders[j.Peer.IP] = SeederResult.Good;
												Core.RememberPeers(new[]{j.Peer});

												if(Core.Filebase.GetHash(package).SequenceEqual(Hash))
												{
													Successful = true;

													foreach(var s in Seeders.Where(i => i.Value == SeederResult.Good))
														foreach(var h in Hubs)
															if(h.Value.Any(i => i.Equals(s.Key)))
																h.Key.HubHits++;
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
							vizor.Cancellation.Token);
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
