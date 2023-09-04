using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class SeedCollector
	{
		public class Seed
		{
			public IPAddress	IP;
			public Peer			Peer;
			public DateTime		Failed;

			public bool			Good => Peer != null && Peer.Status == ConnectionStatus.OK && Failed == DateTime.MinValue;
		}

		public class Hub
		{
			public AccountAddress		Member;
			public IPAddress[]			IPs;
			//public Seed[]				Seeds = {};
			public HubStatus			Status = HubStatus.Estimating;
			public DateTime				Called;
			public bool					Refreshing =false;
			SeedCollector				Collector;
			byte[]						Hash;

			public Hub(SeedCollector finder, byte[] hash, AccountAddress member, IEnumerable<IPAddress> ips)
			{
				Collector = finder;
				Hash = hash;
				Member = member;
				IPs = ips.ToArray();

				Refresh();
			}

			public void Refresh()
			{
				if(Refreshing)
					return;
				else
				{	
					Refreshing = true;
					Called = DateTime.UtcNow;
				}


				Task.Run(() =>	{

									try
									{
										var lr = Collector.Sun.Call(IPs.Random(), p => p.LocateRelease(Hash, 16), Collector.Workflow);

										lock(Collector.Lock)
										{
											//Download.Workflow.Log?.Report(this, "Hub gives seeds", $"for {Download.Resource}, {Account}, {{{string.Join(", ", lr.Seeders.Take(8))}}}, ");

											var seeds = lr.Seeders.Where(i => !Collector.Seeds.Any(j => j.IP.Equals(i))).Select(i => new Seed {IP = i}).ToArray();

											Collector.Seeds.AddRange(seeds);
										}

										while(Collector.Workflow != null && Collector.Workflow.Active)
										{
											lock(Collector.Lock)
											{
												if(Collector.Seeds.Count(i => i.Good) > 16)
												{
													break;
												}
	
												var s = Collector.Seeds	.Where(i => !i.Good)
																		.OrderByDescending(i => i.Peer == null) /// try new seeds
																		.ThenBy(i => i.Failed) /// then oldest failed 
																		.FirstOrDefault(i => DateTime.UtcNow - i.Failed > TimeSpan.FromSeconds(5));  /// skip recent failed
												
												if(s != null)
												{
													if(s.Peer == null)
													{
														s.Peer = Collector.Sun.GetPeer(s.IP);
													}
		
													try
													{
														Monitor.Exit(Collector.Lock);
		
														Collector.Sun.Connect(s.Peer, Collector.Workflow);
													}
													catch(ConnectionFailedException)
													{
														s.Failed = DateTime.UtcNow;
													}
													finally
													{
														Monitor.Enter(Collector.Lock);
													}
														
													s.Failed = DateTime.MinValue;
												}
											}
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
								Collector.Workflow.Cancellation.Token);
			}
		}

		public Sun					Sun;
		public Workflow				Workflow;
		public List<Hub>			Hubs = new();
		public List<Seed>			Seeds = new();
		public object				Lock = new object();

		int							hubsgoodmax = 8;
		Thread						Thread;
		DateTime					MembersRefreshed = DateTime.MinValue;
		MembersResponse.Member[]	Members;

		public SeedCollector(Sun sun, byte[] hash, Workflow workflow)
		{
			Sun = sun;
			Workflow = workflow;
			Hub hlast = null;

 			Thread = new Thread(() =>{ 
										while(Workflow != null && Workflow.Active)
										{
											if(DateTime.UtcNow - MembersRefreshed > TimeSpan.FromSeconds(60))
											{
												var r = Sun.Call<MembersResponse>(i =>	{
																							while(Workflow.Active)
																							{
																								var cr = i.GetMembers();
			
																								if(cr.Members.Any())
																									return cr;
																							}
																											
																							throw new OperationCanceledException();
																						}, 
																						Workflow);
												lock(Lock)
													Members = r.Members.ToArray();
												
												MembersRefreshed = DateTime.UtcNow;
											}
	
											lock(Lock)
											{
												var nearest = Members.OrderBy(i => BigInteger.Abs(new BigInteger(i.Account) - new BigInteger(new Span<byte>(hash, 0, 20)))).Where(i => i.HubIPs.Any()).Take(8).ToArray();
		
												for(int i = 0; i < hubsgoodmax - Hubs.Count(i => i.Status == HubStatus.Estimating); i++)
												{
													var h = nearest.FirstOrDefault(x => !Hubs.Any(y => y.Member == x.Account));
													
													if(h != null)
													{
														//Workflow.Log?.Report(this, "Hub found", $"for {Resource}/{Hex.ToHexString(Hash)}/{File}, {h.Account}, {{{string.Join(", ", h.HubIPs.Take(8))}}}");
		
														hlast = new Hub(this, hash, h.Account, h.HubIPs);
														Hubs.Add(hlast);
													}
													else
														break;
												}
		
												foreach(var i in Hubs.Where(i => DateTime.UtcNow - i.Called > TimeSpan.FromSeconds(5)))
												{
													i.Refresh();
												}
											}

											Thread.Sleep(100);
										}
 									});
			Thread.Start();
		}

		public void Stop()
		{
			Workflow = null;
			Thread.Join();
		}
	}
}
