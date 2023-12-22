using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Collections;

namespace Uccs.Net
{
	public enum HubStatus
	{
		None, Estimating, Refreshing, Bad
	}

	public class FileDownload
	{
		public const int DefaultPieceLength = 512 * 1024;
		public const int MaxThreadsCount = 8;

		public class Piece
		{
			public SeedCollector.Seed	Seed;
			public Task					Task;
			public int					I = -1;
			public long					Length => I * Download.File.PieceLength + Download.File.PieceLength > Download.Length ? Download.Length % Download.File.PieceLength : Download.File.PieceLength;
			public long					Offset => I * Download.File.PieceLength;
			public MemoryStream			Data = new MemoryStream();
			public bool					Succeeded => Data.Length == Length;
			FileDownload				Download;

			public Piece(int piece)
			{
				I = piece;
			}

			public Piece(FileDownload download, SeedCollector.Seed peer, int piece)
			{
				Download = download;
				Seed = peer;
				I = piece;

				Task = Task.Run(() =>	{
											try
											{
												while(Data.Position < Length)
												{
													var d = Seed.Peer.DownloadRelease(Download.Release.Hash, Download.File.Path, Offset + Data.Position, Length - Data.Position).Data;
													Data.Write(d, 0, d.Length);
												}
											}
											catch(Exception ex) when(ex is OperationCanceledException || ex is RdcNodeException || ex is RdcEntityException)
											{
											}
										}, 
										Download.Workflow.Cancellation);
			}
		}

		public LocalRelease								Release;
		public LocalFile							File;
		public bool									Succeeded;
		public long									Length => File.Length;
		public long									DownloadedLength => File.CompletedLength + CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);

		public Task									Task;
		public SeedCollector						SeedCollector;
		public List<Piece>							CurrentPieces = new();
		public Dictionary<SeedCollector.Seed, int>	Seeds = new();
		Sun											Sun;
		Workflow									Workflow;

		public FileDownload(Sun sun, LocalRelease release, string filepath, byte[] filehash, SeedCollector seedcollector, Workflow workflow)
		{
			Sun				= sun;
			Release			= release;
			File			= release.Files?.Find(i => i.Path == filepath);
			Workflow		= workflow;
			SeedCollector	= seedcollector ?? new SeedCollector(sun, release.Hash, workflow);

			if(File != null && File.Completed)
			{	
				release.RemoveFile(File);
				File = null;
			}
			
			Task = Task.Run(() =>
							{
								try
								{
									while(workflow.Active)
									{
										Task[] tasks;
	
										//lock(Lock)
										{
											int left() => File.Pieces.Length - File.CompletedPieces.Count() - CurrentPieces.Count;

											if(File == null || (left() > 0 && CurrentPieces.Count < MaxThreadsCount))
											{
												SeedCollector.Seed[] seeds;
	
												lock(SeedCollector.Lock)
													seeds = SeedCollector.Seeds	.Where(i => i.Good && CurrentPieces.All(j => j.Seed != i))
																				.OrderByDescending(i => Seeds.TryGetValue(i, out var v) ? v : 0)
																				.ToArray(); /// skip currrently used
												
												if(seeds.Any())
												{
													if(File == null)
													{
														long l = -1;

														var s = seeds.First();

														if(!Seeds.ContainsKey(s))
															Seeds[s] = 0;

														try
														{
															l = Sun.Call(s.IP, p => p.Request<FileInfoResponse>(new FileInfoRequest {Release = release.Hash, File = filepath}), workflow).Length;
														}
														catch(RdcNodeException)
														{
															Seeds[s]--;
															continue;
														}
														catch(RdcEntityException)
														{
															Seeds[s]--;
															continue;
														}
														
														lock(Sun.ResourceHub.Lock)
														{
															File = Release.AddFile(filepath, l, DefaultPieceLength, (int)(l / DefaultPieceLength + (l % DefaultPieceLength != 0 ? 1 : 0)));
														}
													}
													
													lock(Sun.ResourceHub.Lock)
													{
														if(Length > 0)
														{
															var s = seeds.AsEnumerable().GetEnumerator();
	
															for(int i=0; i < Math.Min(seeds.Length, Math.Min(left(), MaxThreadsCount - CurrentPieces.Count)); i++)
															{
																s.MoveNext();
																
																if(!Seeds.ContainsKey(s.Current))
																	Seeds[s.Current] = 0;
																
																CurrentPieces.Add(new Piece(this, s.Current, Enumerable.Range(0, File.Pieces.Length).First(i => !File.CompletedPieces.Contains(i) && !CurrentPieces.Any(j => j.I == i))));
															}
														}
														else if(Sun.Zone.Cryptography.HashFile(new byte[] {}).SequenceEqual(filehash)) /// zero-length file
														{
															release.AddFile(File.Path, new byte[0]);
															Succeeded = true;
															goto end;
														}
													}
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
	
										var ti = Task.WaitAny(tasks, 100, workflow.Cancellation);
										
										if(ti == -1)
											continue;

										lock(Sun.ResourceHub.Lock)
										{	
											var p = CurrentPieces.Find(i => i.Task == tasks[ti]);
											
											CurrentPieces.Remove(p);
	
											if(p.Succeeded)
											{
												//j.Seed.Failures++;
												//p.Seed.Failed = DateTime.MinValue;
												Seeds[p.Seed]++;
	
												lock(Sun.ResourceHub.Lock)
												{	
													release.WriteFile(File.Path, p.Offset, p.Data.ToArray());
													File.CompletePiece(p.I);
												}
	
												if(File.CompletedPieces.Count() == File.Pieces.Length)
												{
													lock(Sun.ResourceHub.Lock)
														if(release.Hashify(File.Path).SequenceEqual(filehash))
														{	
															Succeeded = true;
															goto end;
														}
														else
														{
															Release.RemoveFile(File);
															File = null;
															Seeds.Clear();
															CurrentPieces.Clear();
															//CompletedPieces.Clear();
															//Hubs.Clear();
															//Seeds.Clear();
														}
												}
	
												///if(!d.HubsSeeders[h].Contains(s)) /// this hub gave a good seeder
												///	d.HubsSeeders[h].Add(s);
											}
											else
											{	
												Seeds[p.Seed]--;
												//j.Seed.Succeses++;
												//j.Seed.Failed = DateTime.UtcNow;
											}
										}
									}
	
								end:
	
									//var hubs = Hubs.Where(h => h.Seeds.Any(s => s.Peer != null && s.Failed == DateTime.MinValue)).SelectMany(i => i.IPs);
	
									//foreach(var h in hubs)
									//	h.HubRank++;
	
									if(seedcollector == null)
									{
										SeedCollector.Stop();
									}
	
									if(Succeeded)
									{
										//var seeds = CompletedPieces.Select(i => i.Seed.Peer);
											
										//foreach(var h in seeds)
										//	h.SeedRank++;
		
										//lock(Sun.Lock)
										//	Sun.UpdatePeers(seeds);
		
										lock(Sun.ResourceHub.Lock)
										{	
											File.Complete();
	
											if(Release.Type == DataType.File) /// means ResourseType = File
											{
												Release.Complete(Availability.Full);
											}
										}
									}
								}
								catch(Exception) when(Workflow.Aborted)
								{
								}
								finally
								{
									lock(Sun.ResourceHub.Lock)
										Sun.ResourceHub.FileDownloads.Remove(this);
								}
							},
							workflow.Cancellation);
		}

		public override string ToString()
		{
			return $"{Release}/{File.Path}";
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
