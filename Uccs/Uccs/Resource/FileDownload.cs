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
		public const long DefaultPieceLength = 512 * 1024;

		public class Piece
		{
			public SeedCollector.Seed	Seed;
			public Task					Task;
			public int					I = -1;
			public long					Length => I * DefaultPieceLength + DefaultPieceLength > Download.Length ? Download.Length % DefaultPieceLength : DefaultPieceLength;
			public long					Offset => I * DefaultPieceLength;
			public MemoryStream			Data = new MemoryStream();
			public bool					Succeeded => Data.Length == Length;
			FileDownload				Download;

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
													var d = Seed.Peer.DownloadRelease(Download.Resource, Download.Hash, Download.File, Offset + Data.Position, Length - Data.Position).Data;
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

		public ResourceAddress	Resource { get;  }
		public byte[]			Hash { get; }
		public string			File { get; }
		public long				Length { get; protected set; } = -1;
		public bool				Succeeded;
		public long				CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
													- (CompletedPieces.Any(i => i.I == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
													+ CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);

		List<Piece>				CurrentPieces = new();
		List<Piece>				CompletedPieces = new();
		int						PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		public Task				Task;
		public SeedCollector	SeedCollector;
		Sun						Sun;
		Workflow				Workflow;
		object					Lock = new object();

		public FileDownload(Sun sun, ResourceAddress resource, byte[] hash, string file,  byte[] filehash, SeedCollector seedcollector, Workflow workflow)
		{
			Sun				= sun;
			Resource		= resource;
			Hash			= hash;
			File			= file;
			Workflow		= workflow;
			SeedCollector	= seedcollector ?? new SeedCollector(sun, hash, workflow);

			Task = Task.Run(() =>
							{
								Piece j;

								while(workflow.Active)
								{
									Task[] tasks;

									lock(Lock)
									{
										if(Length == -1 || (PiecesTotal - CompletedPieces.Count - CurrentPieces.Count > 0 && CurrentPieces.Count < 8))
										{
											SeedCollector.Seed s;

											lock(SeedCollector.Lock)
												s = SeedCollector.Seeds.Find(i => i.Good && CurrentPieces.All(j => j.Seed != i)); /// skip currrently used
											
											if(s != null)
											{
												//if(s.Peer == null)
												//{
												//	s.Peer = sun.GetPeer(s.IP);
												//}

												if(Length == -1)
												{
													Length = Sun.Call<FileInfoResponse>(s.IP, p => p.Request<FileInfoResponse>(new FileInfoRequest {Resource = resource, Hash = hash, File = file}), workflow).Length;
												}
												
												if(Length > 0)
													CurrentPieces.Add(new Piece(this, s, Enumerable.Range(0, (int)PiecesTotal).First(i => !CompletedPieces.Any(j => j.I == i) && !CurrentPieces.Any(j => j.I == i))));
												else if(Sun.Zone.Cryptography.HashFile(new byte[] {}).SequenceEqual(filehash))
													goto end;
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
											//j.Seed.Failures++;
											j.Seed.Failed = DateTime.MinValue;

											lock(Sun.Resources.Lock)
												Sun.Resources.WriteFile(Resource, Hash, File, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												lock(Sun.Resources.Lock)
													if(Sun.Resources.Hashify(Resource, Hash, File).SequenceEqual(filehash))
													{	
														goto end;
													}
													else
													{
														CurrentPieces.Clear();
														CompletedPieces.Clear();
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

								var seeds = CompletedPieces.Select(i => i.Seed.Peer);
									
								//foreach(var h in seeds)
								//	h.SeedRank++;

								lock(Sun.Lock)
									Sun.UpdatePeers(seeds);

								lock(Sun.Resources.Lock)
									Sun.Resources.FileDownloads.Remove(this);

								Succeeded = true;
							},
							workflow.Cancellation.Token);
		}

		public override string ToString()
		{
			return $"{Resource}/{File}";
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
