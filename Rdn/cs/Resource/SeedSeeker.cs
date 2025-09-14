using System.Net;

namespace Uccs.Rdn;

public class SeedSeeker
{
	public class Seed
	{
		public IPAddress	IP;
		public Peer			Peer;
		public DateTime		Failed;

		public bool			Good => Peer != null && Peer.Status == ConnectionStatus.OK && Failed == DateTime.MinValue;

		public override string ToString()
		{
			return $"{IP}, Good={Good}, Failed={Failed}";
		}
	}

	public class Hub
	{
		public AccountAddress		Member;
		public IPAddress[]			IPs;
		//public Seed[]				Seeds = {};
		public HubStatus			Status = HubStatus.Estimating;
		SeedSeeker					Seeker;
		Urr							Address;
		Flow						Flow;

		public Hub(SeedSeeker seeker, Urr hash, AccountAddress member, IEnumerable<IPAddress> ips)
		{
			Seeker = seeker;
			Address = hash;
			Member = member;
			IPs = ips.ToArray();
			Flow = Seeker.Flow.CreateNested(ToString());

			var t = Seeker.Node.CreateThread(() =>	{
														while(Flow.Active)
														{
															try
															{
																var lr = Seeker.Node.Peering.Call(IPs.Random(), () => new LocateReleaseRequest {Address = Address, Count = 16}, Flow);

																lock(Seeker.Lock)
																{
																	var seeds = lr.Seeders.Where(i => !Seeker.Seeds.Any(j => j.IP.Equals(i))).Select(i => new Seed {IP = i}).ToArray();

																	Seeker.Seeds.AddRange(seeds);
																}
															}
															catch(Exception ex) when (ex is NodeException || ex is EntityException)
															{
															}
															catch(OperationCanceledException)
															{
															}

															WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], seeker.Node.Settings.Seed.RefreshInterval);
														}
													});

			t.Name = ToString();
			t.Start();
		}

		public void Stop()
		{
			Flow.Abort();
		}

		public override string ToString()
		{
			return $"{GetType().Name}, Address={Address}, Member={Member}, {IPs[0]}";
		}
	}

	public RdnNode				Node;
	public Flow					Flow;
	public List<Hub>			Hubs = new();
	public List<Seed>			Seeds = new();
	public object				Lock = new object();
	//public AutoResetEvent		SeedsFound = new(true);

	Thread						HubingThread;
	DateTime					MembersRefreshed = DateTime.MinValue;
	RdnGenerator[]				Members;

	public SeedSeeker(RdnNode sun, Urr address, Flow flow)
	{
		Node = sun;
		Flow = flow.CreateNested($"{GetType().Name}, {address}");
		Hub hlast = null;

 		HubingThread = Node.CreateThread(() =>	{ 
													while(Flow.Active)
													{
														var r = Node.Peering.Call(() => new RdnMembersRequest(), Flow);
	
														lock(Lock)
														{
															Members = r.Members.ToArray();

															var nearest = Members.OrderByHash(i => i.Address.Bytes, address.MemberOrderKey).Take(ResourceHub.MembersPerDeclaration).Cast<RdnGenerator>();
		
															do 
															{
																var m = nearest.FirstOrDefault(x => !Hubs.Any(y => y.Member == x.Address));
														
												 				if(m != null)
																{
																	hlast = new Hub(this, address, m.Address, m.SeedHubPpcIPs);
																	Hubs.Add(hlast);
																}
																else
																	break;
															}
															while(Flow.Active);

															do 
															{
																var h = Hubs.FirstOrDefault(x => !nearest.Any(y => y.Address == x.Member));
	
																if(h != null)
																{
																	h.Stop();
																	Hubs.Remove(h);
																}
																else
																	break;
															}
															while(Flow.Active);
														}

														WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 60 * 1000);
													}
 												});
		HubingThread.Start();

		for(int i=0; i<8; i++)
		{
			Task.Run(() =>	{
								try
								{
									while(Flow.Active)
									{
										lock(Lock)
										{
											if(Seeds.Count(i => i.Good) > 32)
												break;
	
											var s = Seeds	.Where(i => !i.Good)
															.OrderByDescending(i => i.Peer == null) /// try new seeds
															.ThenBy(i => i.Failed) /// then oldest failed 
															.FirstOrDefault(i => DateTime.UtcNow - i.Failed > TimeSpan.FromSeconds(5));  /// skip recent failed
												
											if(s != null)
											{
												if(s.Peer == null)
													s.Peer = Node.Peering.GetPeer(s.IP);
		
												try
												{
													Monitor.Exit(Lock);
		
													Node.Peering.Connect(s.Peer, Flow);
														
													s.Failed = DateTime.MinValue;
												}
												catch(NodeException)
												{
													s.Failed = DateTime.UtcNow;
												}
												finally
												{
													Monitor.Enter(Lock);
												}
											}
										}

										WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 100);
									}
								}
								catch(OperationCanceledException)
								{
								}
							}, 
							Flow.Cancellation);
		}
	}

	public void Stop()
	{
// 		foreach(var i in Hubs)
// 		{
// 			i.Stop();
// 		}

		Flow.Abort();
		HubingThread.Join();
	}
}
