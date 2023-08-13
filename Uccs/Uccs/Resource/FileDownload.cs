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
		Null, Estimating, Bad
	}

	public class DownloadReport
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
			public AccountAddress			Account { get; set; }
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

		public class Hub
		{
			public AccountAddress		Account;
			public IPAddress[]			IPs;
			public Seed[]				Seeds = new Seed[]{};
			public HubStatus			Status = HubStatus.Estimating;
			public DateTime				Called;
			public bool					Refreshing =false;
			FileDownload				Download;

			public Hub(FileDownload download, AccountAddress member, IEnumerable<IPAddress> ips)
			{
				Download = download;
				Account = member;
				IPs = ips.ToArray();

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
										var lr = Download.Sun.Call(IPs.Random(), p => p.LocateRelease(Download.Hash, 16), Download.Workflow);

										lock(Download.Lock)
										{
											Download.Workflow.Log?.Report(this, "Hub gives seeds", $"for {Download.Resource}, {Account}, {{{string.Join(", ", lr.Seeders.Take(8))}}}, ");

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
			FileDownload			Download;

			public Piece(FileDownload download, Seed peer, int piece)
			{
				Download = download;
				Seed = peer;
				I = piece;

				Task = Task.Run(() =>	{
											try
											{
												Download.Sun.Connect(Seed.Peer, download.Workflow);

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

		public class Seed
		{
			public IPAddress	IP;
			public Peer			Peer;
			public DateTime		Failed;
			public int			Failures;
			public int			Succeses;
		}

		public ResourceAddress						Resource { get;  }
		public byte[]								Hash { get; }
		public string								File { get; }
		public long									Length { get; protected set; } = -1;
		public bool									Succeeded => Downloaded;
		public long									CompletedLength =>	CompletedPieces.Count * DefaultPieceLength 
																		- (CompletedPieces.Any(i => i.I == PiecesTotal-1) ? DefaultPieceLength - Length % DefaultPieceLength : 0) /// take the tail into account
																		+ CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);
		public object								Lock = new object();

		internal Sun								Sun;
		internal Workflow							Workflow;
		bool										Downloaded;
		List<Piece>									CurrentPieces = new();
		List<Piece>									CompletedPieces = new();
		public List<Hub>							Hubs = new();
		public List<Seed>							Seeds = new();
		int											PiecesTotal => (int)(Length / DefaultPieceLength + (Length % DefaultPieceLength != 0 ? 1 : 0));
		public Task									Task;

		public FileDownload(Sun sun, ResourceAddress resource, byte[] hash, string file,  byte[] filehash, Workflow workflow)
		{
			Sun			= sun;
			Resource	= resource;
			Hash		= hash;
			File		= file;
			Workflow	= workflow;

			int hubsgoodmax = 8;

			Task = Task.Run(() =>
							{
								Piece j;

								while(!workflow.IsAborted)
								{
									Task[] tasks;
									Hub hlast = null;

									lock(Lock)
									{
										var cr = Sun.Call<MembersResponse>(Role.Base, i =>	{
																								while(workflow.Active)
																								{
																									var cr = i.GetMembers();
	
																									if(cr.Members.Any())
																										return cr;
																								}
																									
																								throw new OperationCanceledException();
																							}, 
																							Sun.Workflow);

										var nearest = cr.Members.OrderBy(i => BigInteger.Abs(new BigInteger(i.Account) - new BigInteger(new Span<byte>(hash, 0, 20)))).Where(i => i.HubIPs.Any()).Take(8).ToArray();

										for(int i = 0; i < hubsgoodmax - Hubs.Count(i => i.Status == HubStatus.Estimating); i++)
										{
											var h = nearest.FirstOrDefault(x => !Hubs.Any(y => y.Account == x.Account));
											
											if(h != null)
											{
												Workflow.Log?.Report(this, "Hub found", $"for {Resource}/{Hex.ToHexString(Hash)}/{File}, {h.Account}, {{{string.Join(", ", h.HubIPs.Take(8))}}}");

												hlast = new Hub(this, h.Account, h.HubIPs);
												Hubs.Add(hlast);
											}
											else
												break;
										}

										foreach(var i in Hubs.Where(i => i.Seeds.Length < 16 && DateTime.UtcNow - i.Called > TimeSpan.FromSeconds(5)))
										{
											i.Refresh();
										}

										if(Length == -1 || (PiecesTotal - CompletedPieces.Count - CurrentPieces.Count > 0 && CurrentPieces.Count < 8))
										{
											var s = Seeds.OrderBy(i => i.Peer == null).FirstOrDefault(i =>	i.Peer == null ||  /// new candidate
																											i.Peer != null && i.Failed == DateTime.MinValue || /// succeeded previously
																											(DateTime.UtcNow - i.Failed) > TimeSpan.FromSeconds(5)  &&  /// bad node
																											CurrentPieces.All(j => j.Seed != i));
											
											if(s != null)
											{
												if(s.Peer == null)
												{
													s.Peer = sun.GetPeer(s.IP);
												}

												if(Length == -1)
												{
													sun.Connect(s.Peer, Workflow);

													Length = s.Peer.Request<FileInfoResponse>(new FileInfoRequest {Resource = resource, Hash = hash, File = file}).Length;
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

											lock(Sun.Resources.Lock)
												Sun.Resources.WriteFile(Resource, Hash, File, j.Offset, j.Data.ToArray());
											
											CompletedPieces.Add(j);

											if(CompletedPieces.Count() == PiecesTotal)
											{
												lock(Sun.Lock)
													if(Sun.Resources.Hashify(Resource, Hash, File).SequenceEqual(filehash))
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

								lock(Sun.Resources.Lock)
								{
									var hubs = Hubs.Where(h => h.Seeds.Any(s => s.Peer != null && s.Failed == DateTime.MinValue)).SelectMany(i => i.IPs);

									//foreach(var h in hubs)
									//	h.HubRank++;

									var seeds = CompletedPieces.Select(i => i.Seed.Peer);
									
									//foreach(var h in seeds)
									//	h.SeedRank++;

									lock(Sun.Lock)
									{
										Sun.UpdatePeers(seeds);
									}

									Sun.Resources.Downloads.Remove(this);

									Downloaded = true;
								}
									
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
