using System.Net;

namespace Uccs.Rdn
{
	public class Harvester
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
			Harvester					Collector;
			Urr							Address;

			public Hub(Harvester collector, Urr hash, AccountAddress member, IEnumerable<IPAddress> ips)
			{
				Collector = collector;
				Address = hash;
				Member = member;
				IPs = ips.ToArray();

				Task.Run(() =>	{
									while(Collector.Flow.Active)
									{
										try
										{
											var lr = Collector.Node.Call(IPs.Random(), () => new LocateReleaseRequest {Address = Address, Count = 16}, Collector.Flow);
	
											lock(Collector.Lock)
											{
												var seeds = lr.Seeders.Where(i => !Collector.Seeds.Any(j => j.IP.Equals(i))).Select(i => new Seed {IP = i}).ToArray();
	
												Collector.Seeds.AddRange(seeds);
											}
										}
										catch(Exception ex) when (ex is NodeException || ex is EntityException)
										{
										}
										catch(OperationCanceledException)
										{
										}

										WaitHandle.WaitAny([Collector.Flow.Cancellation.WaitHandle], collector.Node.Mcv.Settings.Seed.CollectRefreshInterval);
									}
								}, 
								Collector.Flow.Cancellation);
			}
		}

		public RdnNode				Node;
		public Flow					Flow;
		public List<Hub>			Hubs = new();
		public List<Seed>			Seeds = new();
		public object				Lock = new object();
		//public AutoResetEvent		SeedsFound = new(true);

		int							hubsgoodmax = 8;
		Thread						Thread;
		DateTime					MembersRefreshed = DateTime.MinValue;
		MembersResponse.Member[]	Members;

		public Harvester(RdnNode sun, Urr address, Flow flow)
		{
			Node = sun;
			Flow = flow.CreateNested($"SeedCollector {address}");
			Hub hlast = null;

 			Thread = Node.CreateThread(() =>	{ 
													while(Flow.Active)
													{
														if(DateTime.UtcNow - MembersRefreshed > TimeSpan.FromSeconds(60))
														{
															var r = Node.Call(() => new MembersRequest(), Flow);

															lock(Lock)
																Members = r.Members.ToArray();
													
															MembersRefreshed = DateTime.UtcNow;
														}
		
														lock(Lock)
														{
															var nearest = Members.OrderByNearest(address.MemberOrderKey).Take(ResourceHub.MembersPerDeclaration);
			
															for(int i = 0; i < hubsgoodmax - Hubs.Count(i => i.Status == HubStatus.Estimating); i++)
															{
																var h = nearest.FirstOrDefault(x => !Hubs.Any(y => y.Member == x.Account));
														
												 				if(h != null)
																{
																	hlast = new Hub(this, address, h.Account, h.SeedHubRdcIPs);
																	Hubs.Add(hlast);
																}
																else
																	break;
															}
														}
	
														WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 100);
													}
 												});
			Thread.Start();

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
													{
														s.Peer = Node.GetPeer(s.IP);
													}
			
													try
													{
														Monitor.Exit(Lock);
			
														Node.Connect(s.Peer, Flow);
															
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
			Flow.Abort();
			Thread.Join();
		}
	}
}
