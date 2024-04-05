using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;

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
			SeedCollector				Collector;
			ReleaseAddress				Address;

			public Hub(SeedCollector collector, ReleaseAddress hash, AccountAddress member, IEnumerable<IPAddress> ips)
			{
				Collector = collector;
				Address = hash;
				Member = member;
				IPs = ips.ToArray();

				Task.Run(() =>	{
									while(Collector.Workflow.Active)
									{
										try
										{
											var lr = Collector.Sun.Call(IPs.Random(), p => p.Request(new LocateReleaseRequest {Address = Address, Count = 16}), Collector.Workflow);
	
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

										WaitHandle.WaitAny(new WaitHandle []{Collector.Workflow.Cancellation.WaitHandle}, collector.Sun.Settings.ResourceHub.CollectRefreshInterval);
									}
								}, 
								Collector.Workflow.Cancellation);
			}
		}

		public Sun					Sun;
		public Workflow				Workflow;
		public List<Hub>			Hubs = new();
		public List<Seed>			Seeds = new();
		public object				Lock = new object();
		//public AutoResetEvent		SeedsFound = new(true);

		int							hubsgoodmax = 8;
		Thread						Thread;
		DateTime					MembersRefreshed = DateTime.MinValue;
		MembersResponse.Member[]	Members;

		public SeedCollector(Sun sun, ReleaseAddress address, Workflow workflow)
		{
			Sun = sun;
			Workflow = workflow.CreateNested($"SeedCollector {address}");
			Hub hlast = null;

 			Thread = sun.CreateThread(() =>	{ 
												while(Workflow.Active)
												{
													if(DateTime.UtcNow - MembersRefreshed > TimeSpan.FromSeconds(60))
													{
														var r = Sun.Call(i =>	{
																					var cr = i.Request(new MembersRequest());
				
																					if(cr.Members.Any())
																						return cr;
																												
																					throw new ContinueException();
																				}, 
																				Workflow);
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
	
													WaitHandle.WaitAny(new WaitHandle []{Workflow.Cancellation.WaitHandle}, 100);
												}
 											});
			Thread.Start();

			for(int i=0; i<8; i++)
			{
				Task.Run(() =>	{
									try
									{
										while(Workflow.Active)
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
														s.Peer = Sun.GetPeer(s.IP);
													}
			
													try
													{
														Monitor.Exit(Lock);
			
														Sun.Connect(s.Peer, Workflow);
															
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

											WaitHandle.WaitAny(new WaitHandle []{Workflow.Cancellation.WaitHandle}, 100);
										}
									}
									catch(OperationCanceledException)
									{
									}
								}, 
								Workflow.Cancellation);
			}
		}

		public void Stop()
		{
			Workflow.Abort();
			Thread.Join();
		}
	}
}
