using System.Net;

namespace Uccs.Rdn;

public enum HubStatus
{
	None, Estimating, Refreshing, Bad
}

public interface IIntegrity
{
	bool Verify(byte[] hash);
}

public class DHIntegrity : IIntegrity
{
	byte[]		Release;

	public DHIntegrity(byte[] release)
	{
		Release = release;
	}

	public bool Verify(byte[] hash)
	{
		return Release.SequenceEqual(hash);
	}
}

public class SPDIntegrity : IIntegrity
{
	Urrsd			Release;
	AccountAddress	Account;
	Cryptography	Cryptography;

	public SPDIntegrity(Cryptography cryptography, Urrsd release, AccountAddress account)
	{
		Release		 = release;
		Account		 = account;
		Cryptography = cryptography;
	}

	public bool Verify(byte[] hash)
	{
		return Release.Prove(Cryptography, Account, hash);
	}
}

public delegate void PieceDelegate(FileDownload.Piece piece);

public class FileDownload
{
	public const int DefaultPieceLength = 512 * 1024;
	public const int MaxThreadsCount = 8;

	public class Piece
	{
		public SeedSeeker.Seed		Seed;
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

		public Piece(FileDownload download, SeedSeeker.Seed peer, int piece)
		{
			Download = download;
			Seed = peer;
			I = piece;

			Task = Task.Run(() =>	{
										try
										{
											while(Data.Position < Length)
											{
												var d = Download.Rdn.Peering.Call(Seed.Peer, new DownloadReleaseRequest{Address = Download.Release.Address, 
																														File = Download.File.Path, 
																														Offset = Offset + Data.Position,
																														Length = Length - Data.Position}).Data;
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
									Download.Flow.Cancellation);
		}
	}

	public LocalRelease							Release;
	public LocalFile							File;
	public bool									Succeeded;
	public long									Length => File.Length;
	public long									DownloadedLength => File.CompletedLength + CurrentPieces.Sum(i => i.Data != null ? i.Data.Length : 0);

	public Task									Task;
	public SeedSeeker							Seeker;
	public List<Piece>							CurrentPieces = new();
	public Dictionary<SeedSeeker.Seed, int>		Seeds = new();
	public RdnNode								Rdn;
	public PieceDelegate						PieceSucceeded;
	Flow										Flow;

	public FileDownload(RdnNode sun, LocalRelease release, bool single, string path, string localpath, IIntegrity integrity, SeedSeeker seeker, Flow flow)
	{
		Rdn			= sun;
		Release		= release;
		File		= release.Find(path) ?? release.AddEmpty(path, localpath);
		Flow		= flow;
		Seeker		= seeker ?? new SeedSeeker(sun, release.Address, flow);

		if(File.Status == LocalFileStatus.Completed)
		{
			if(integrity.Verify(release.Hashify(path)))
			{
				Task = Task.CompletedTask;
				Succeeded = true;
				return;
			}
			else
				File.Reset();
		}

		if(single)
			Release.Activity = this;
		
		File.Activity = this;

		Task = Task.Run(() =>
						{
							try
							{
								while(flow.Active)
								{
									Task[] tasks;

									//lock(Lock)
									{
										int left() => File.Pieces.Length - File.CompletedPieces.Count() - CurrentPieces.Count;

										if(File.Status != LocalFileStatus.Inited || (File.Length > 0 && left() > 0 && CurrentPieces.Count < MaxThreadsCount))
										{
											SeedSeeker.Seed[] seeds;

											lock(Seeker.Lock)
												seeds = Seeker.Seeds	.Where(i => i.Good && CurrentPieces.All(j => j.Seed != i))
																		.OrderByDescending(i => Seeds.TryGetValue(i, out var v) ? v : 0)
																		.ToArray(); /// skip currrently used
											
											if(seeds.Any())
											{
												if(File.Status != LocalFileStatus.Inited)
												{
													long l = -1;

													var s = seeds.First();

													if(!Seeds.ContainsKey(s))
														Seeds[s] = 0;

													try
													{
														l = Rdn.Peering.Call(s.IP, () => new FileInfoRequest {Release = release.Address, File = path}, flow).Length;
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
													
													lock(Rdn.ResourceHub.Lock)
													{
														File.Init(l, DefaultPieceLength, (int)(l / DefaultPieceLength + (l % DefaultPieceLength != 0 ? 1 : 0)));
													}
												}
												
												lock(Rdn.ResourceHub.Lock)
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
													else if(integrity.Verify(Rdn.Net.Cryptography.HashFile([]))) /// zero-length file
													{
														File.Write(0, []);
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

									var ti = Task.WaitAny(tasks, 100, flow.Cancellation);
									
									if(ti == -1)
										continue;

									lock(Rdn.ResourceHub.Lock)
									{	
										var p = CurrentPieces.Find(i => i.Task == tasks[ti]);
										
										CurrentPieces.Remove(p);

										if(p.Succeeded)
										{
											Seeds[p.Seed]++;

											lock(Rdn.ResourceHub.Lock)
											{	
												File.Write(p.Offset, p.Data.ToArray());
												File.CompletePiece(p.I);

												PieceSucceeded?.Invoke(p);
											}

											if(File.CompletedPieces.Count() == File.Pieces.Length)
											{
												lock(Rdn.ResourceHub.Lock)
													if(integrity.Verify(release.Hashify(File.Path)))
													{	
														Succeeded = true;
														goto end;
													}
													else
													{
														File.Reset();
														Seeds.Clear();
														CurrentPieces.Clear();
													}
											}
										}
										else
										{	
											Seeds[p.Seed]--;
										}
									}
								}

							end:

								//var hubs = Hubs.Where(h => h.Seeds.Any(s => s.Peer != null && s.Failed == DateTime.MinValue)).SelectMany(i => i.IPs);

								//foreach(var h in hubs)
								//	h.HubRank++;

								if(seeker == null)
								{
									Seeker.Stop();
								}

								if(Succeeded)
								{
									//var seeds = CompletedPieces.Select(i => i.Seed.Peer);
										
									//foreach(var h in seeds)
									//	h.SeedRank++;
	
									//lock(Sun.Lock)
									//	Sun.UpdatePeers(seeds);
	
									lock(Rdn.ResourceHub.Lock)
									{	
										File.Complete();

										if(single) /// means ResourseType = File
										{
											Release.Complete(Availability.Full);
										}
									}
								}
							}
							catch(Exception) when(Flow.Aborted)
							{
							}
							finally
							{
								lock(Rdn.ResourceHub.Lock)
								{	
									if(single)
										Release.Activity = null;

									File.Activity = null;
								}
							}
						},
						flow.Cancellation);
	}

	public void Stop()
	{
		Flow.Abort();
	}
}

public class DirectoryDownload
{
	public LocalRelease			Release;
	public string				LocalPath;
	public bool					Succeeded;
	public Queue<Xon>			Files = new();
	public int					CompletedCount;
	public int					TotalCount;
	public List<FileDownload>	CurrentDownloads = new();
	public Task					Task;
	public SeedSeeker			SeedSeeker;
	Flow						Flow;

	public DirectoryDownload(RdnNode sun, LocalRelease release, string localpath, IIntegrity integrity, Flow flow)
	{
		Release = release;
		LocalPath = localpath;
		Release.Activity = this;
		Flow = flow;
		SeedSeeker = new SeedSeeker(sun, release.Address, Flow);

		void run()
		{
			try
			{
				sun.ResourceHub.GetFile(release, false, LocalRelease.Index, null, integrity, SeedSeeker, Flow);

				var index = new Xon(release.Find(LocalRelease.Index).Read());

				void enumearate(Xon xon)
				{
					if(xon.Parent != null && xon.Parent.Name != null)
						xon.Name = xon.Parent.Name + "/" + xon.Name;

					if(xon.Value != null)
					{
						Files.Enqueue(xon);
					}

					foreach(var i in xon.Nodes)
					{
						enumearate(i);
					}
				}

				enumearate(index);

				TotalCount = Files.Count;

				do 
				{
					if(CurrentDownloads.Count < 10 && Files.Any())
					{
						var f = Files.Dequeue();

						lock(sun.ResourceHub.Lock)
						{
							var dd = sun.ResourceHub.DownloadFile(release, false, f.Name, Path.Join(LocalPath, f.Name), new DHIntegrity(f.Value as byte[]), SeedSeeker, Flow);

							if(dd != null)
							{
								CurrentDownloads.Add(dd);
							}
						}
					}

					var i = Task.WaitAny(CurrentDownloads.Select(i => i.Task).ToArray(), Flow.Cancellation);

					if(CurrentDownloads[i].Succeeded)
					{
						CompletedCount++;
						CurrentDownloads.Remove(CurrentDownloads[i]);
					}
				}
				while(Files.Any() && Flow.Active);

				SeedSeeker.Stop();

				lock(sun.ResourceHub.Lock)
				{
					Succeeded = true;
					release.Complete(Availability.Full);
				}
			}
			catch(Exception) when(Flow.Aborted)
			{
			}
			finally
			{
				lock(sun.ResourceHub.Lock)
					Release.Activity = null;
			}
		}

		Task = Task.Run(run, Flow.Cancellation);
	}

	public void Stop()
	{
		Flow.Abort();
	}

	public override string ToString()
	{
		return Release.Address.ToString();
	}
}

public class FileDownloadProgress : ResourceActivityProgress
{
	public FileDownloadProgress()
	{
	}

	public FileDownloadProgress(FileDownload file)
	{
		Path				= file.File.Status == LocalFileStatus.Inited ? file.File.Path : null;
		Length				= file.File.Status == LocalFileStatus.Inited ? file.File.Length : -1;
		DownloadedLength	= file.File.Status == LocalFileStatus.Inited ? file.DownloadedLength : -1;
	}

	public string	Path { get; set; }
	public long		Length { get; set; }
	public long		DownloadedLength { get; set; }
}

public class ReleaseDownloadProgress : ResourceActivityProgress
{
	public class Hub
	{
		public AccountAddress	Member { get; set; }
		public HubStatus		Status { get; set; }
	}

	public class Seed
	{
		public IPAddress	IP { get; set; }
		public int			Failures { get; set; }
		public int			Succeses { get; set; }
	}

	public IEnumerable<Hub>						Hubs { get; set; }
	public IEnumerable<FileDownloadProgress>	CurrentFiles { get; set; }
	public IEnumerable<Seed>					Seeds { get; set; }
	public bool									Succeeded  { get; set; }

	public ReleaseDownloadProgress()
	{
	}

	public ReleaseDownloadProgress(SeedSeeker seeker)
	{
		Hubs	= seeker.Hubs.Select(i => new Hub {Member = i.Member, Status = i.Status}).ToArray();
		Seeds	= seeker.Seeds.Select(i => new Seed {IP = i.IP}).ToArray();
	}

	public override string ToString()
	{
		return $"H={{{Hubs.Count()}}}, S={{{Seeds.Count()}}}, F={{{CurrentFiles.Count()}}}, {string.Join(", ", CurrentFiles.Select(i => $"{i.Path}={i.DownloadedLength}/{i.Length}"))}";
	}
}
