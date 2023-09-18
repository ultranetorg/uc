using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;

namespace Uccs.Net
{
	public enum HubStatus
	{
		Null, Estimating, Refreshing, Bad
	}

	public class PackageDownloadReport
	{
		public class Dependency
		{
			public ResourceAddress	Release { get; set; }
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
			public AccountAddress			Member { get; set; }
			public HubStatus				Status { get; set; }
			public IEnumerable<IPAddress>	Seeds { get; set; }
		}

		public string					File { get; set; }
		public long						Length { get; set; }
		public long						CompletedLength { get; set; }
		public IEnumerable<Dependency>	DependenciesRecursive { get; set; }
		public IEnumerable<Hub>			Hubs { get; set; }
		public IEnumerable<Seed>		Seeds { get; set; }
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
													var d = Seed.Peer.DownloadRelease(Download.Release.Address, Download.Release.Hash, Download.File.Path, Offset + Data.Position, Length - Data.Position).Data;
													Data.Write(d, 0, d.Length);
												}
											}
											catch(Exception ex) when(ex is ConnectionFailedException || ex is OperationCanceledException || ex is RdcNodeException || ex is RdcEntityException)
											{
											}
										}, 
										Download.Workflow.Cancellation);
			}
		}

		public Release			Release;
		public ReleaseFile		File;
		public bool				Succeeded;
		public long				Length => File.Length;
		public long				DownloadedLength => File.CompletedLength + CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);

		public Task				Task;
		public SeedCollector	SeedCollector;
		public List<Piece>		CurrentPieces = new();
		Sun						Sun;
		Workflow				Workflow;
		public object			Lock = new object();

		public FileDownload(Sun sun, Release release, string filepath, byte[] filehash, SeedCollector seedcollector, Workflow workflow)
		{
			Sun				= sun;
			Release			= release;
			File			= release.Files?.Find(i => i.Path == filepath);
			Workflow		= workflow;
			SeedCollector	= seedcollector ?? new SeedCollector(sun, release.Hash, workflow);

			Task = Task.Run(() =>
							{
								try
								{
									while(workflow.Active)
									{
										Task[] tasks;
	
										lock(Lock)
										{
											int left() => File.Pieces.Length - File.CompletedPieces.Count() - CurrentPieces.Count;

											if(File == null || (left() > 0 && CurrentPieces.Count < MaxThreadsCount))
											{
												SeedCollector.Seed[] seeds;
	
												lock(SeedCollector.Lock)
													seeds = SeedCollector.Seeds.Where(i => i.Good && CurrentPieces.All(j => j.Seed != i)).ToArray(); /// skip currrently used
												
												if(seeds.Any())
												{
													if(File == null)
													{
														var l = Sun.Call(seeds.First().IP, p => p.Request<FileInfoResponse>(new FileInfoRequest {Resource = release.Address, Hash = release.Hash, File = filepath}), workflow).Length;
														
														lock(Sun.ResourceHub.Lock)
														{
															File = Release.AddFile(filepath, l, DefaultPieceLength, (int)(l / DefaultPieceLength + (l % DefaultPieceLength != 0 ? 1 : 0)));
														}
													}
													
													if(Length > 0)
													{
														var s = seeds.AsEnumerable().GetEnumerator();

														for(int i=0; i < Math.Min(seeds.Length, Math.Min(left(), MaxThreadsCount - CurrentPieces.Count)); i++)
														{
															s.MoveNext();
															CurrentPieces.Add(new Piece(this, s.Current, Enumerable.Range(0, File.Pieces.Length).First(i => !File.CompletedPieces.Contains(i) && !CurrentPieces.Any(j => j.I == i))));
														}
													}
													else if(Sun.Zone.Cryptography.HashFile(new byte[] {}).SequenceEqual(filehash))
													{
														Sun.ResourceHub.WriteFile(Release.Address, release.Hash, File.Path, 0, new byte[0]);
														Succeeded = true;
														goto end;
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
											if(tasks.Length > 1)
												File=File;
										}
	
										var ti = Task.WaitAny(tasks, 100, workflow.Cancellation);
										
										if(ti == -1)
											continue;

										lock(Lock)
										{	
											var p = CurrentPieces.Find(i => i.Task == tasks[ti]);
											
											CurrentPieces.Remove(p);
	
											if(p.Succeeded)
											{
												//j.Seed.Failures++;
												p.Seed.Failed = DateTime.MinValue;
	
												lock(Sun.ResourceHub.Lock)
												{	
													Sun.ResourceHub.WriteFile(Release.Address, release.Hash, File.Path, p.Offset, p.Data.ToArray());
													File.CompletePiece(p.I);
												}
												
												//CompletedPieces.Add(p);
	
												if(File.CompletedPieces.Count() == File.Pieces.Length)
												{
													lock(Sun.ResourceHub.Lock)
														if(Sun.ResourceHub.Hashify(Release.Address, release.Hash, File.Path).SequenceEqual(filehash))
														{	
															Succeeded = true;
															goto end;
														}
														else
														{
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
	
											if(Release.Hash.SequenceEqual(filehash)) /// means ResourseType = File
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
