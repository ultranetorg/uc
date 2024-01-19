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
											catch(OperationCanceledException)
											{
											}
											catch(Exception ex) when(ex is NodeException || ex is EntityException)
											{
											}
										}, 
										Download.Workflow.Cancellation);
			}
		}

		public LocalRelease							Release;
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

		public FileDownload(Sun sun, LocalRelease release, string path, byte[] hash, SeedCollector seedcollector, Workflow workflow)
		{
			Sun					= sun;
			Release				= release;
			Release.Activity	= release.Type == DataType.File ? this : null;
			File				= release.Files.Find(i => i.Path == path) ?? release.AddEmpty(path);
			File.Activity		= this;
			Workflow			= workflow;
			SeedCollector		= seedcollector ?? new SeedCollector(sun, release.Hash, workflow);

			if(File.Completed)
			{
				if(release.Hashify(path).SequenceEqual(hash))
				{
					Succeeded = true;
					return;
				}
				else
					File.Reset();
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

											if(!File.Initialized || (left() > 0 && CurrentPieces.Count < MaxThreadsCount))
											{
												SeedCollector.Seed[] seeds;
	
												lock(SeedCollector.Lock)
													seeds = SeedCollector.Seeds	.Where(i => i.Good && CurrentPieces.All(j => j.Seed != i))
																				.OrderByDescending(i => Seeds.TryGetValue(i, out var v) ? v : 0)
																				.ToArray(); /// skip currrently used
												
												if(seeds.Any())
												{
													if(!File.Initialized)
													{
														long l = -1;

														var s = seeds.First();

														if(!Seeds.ContainsKey(s))
															Seeds[s] = 0;

														try
														{
															l = Sun.Call(s.IP, p => p.Request<FileInfoResponse>(new FileInfoRequest {Release = release.Hash, File = path}), workflow).Length;
														}
														catch(NodeException)
														{
															Seeds[s]--;
															continue;
														}
														catch(EntityException)
														{
															Seeds[s]--;
															continue;
														}
														
														lock(Sun.ResourceHub.Lock)
														{
															File.Init(l, DefaultPieceLength, (int)(l / DefaultPieceLength + (l % DefaultPieceLength != 0 ? 1 : 0)));
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
														else if(Sun.Zone.Cryptography.HashFile(new byte[] {}).SequenceEqual(hash)) /// zero-length file
														{
															release.AddCompleted(File.Path, new byte[0]);
															Succeeded = true;
															goto end;
														}
													}
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
														if(release.Hashify(File.Path).SequenceEqual(hash))
														{	
															Succeeded = true;
															goto end;
														}
														else
														{
															File.Reset();
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
									{	
										if(Release.Type == DataType.File)
											Release.Activity = null;

										File.Activity = null;
									}
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
