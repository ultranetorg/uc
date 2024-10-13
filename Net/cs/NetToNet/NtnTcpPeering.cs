using System.Collections.Immutable;
using System.Net;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class NtnTcpPeering : TcpPeering
	{
		public NtnTcpPeering(Node node, PeeringSettings settings, long roles, Flow flow) : base(node, settings, flow)
		{
		}

		public virtual void Run()
		{
			if(Settings.IP != null)
			{
				ListeningThread = Node.CreateThread(Listening);
				ListeningThread.Name = $"{Node.Name} Listening";
				ListeningThread.Start();
			}

			MainThread = Node.CreateThread(() =>	{
														while(Flow.Active)
														{
															var r = WaitHandle.WaitAny([MainWakeup, Flow.Cancellation.WaitHandle], 500);

															lock(Lock)
															{
																ProcessConnectivity();
															}
														}
													});

			MainThread.Name = $"{Node.Name} Main";
			MainThread.Start();
		}

		public override string ToString()
		{
			return string.Join(",", new object[] {Node.Name,
												  Settings?.IP}.Where(i => i != null));
		}

		protected override Hello CreateOutboundHello(Peer peer, bool permanent)
		{
			lock(Lock)
			{
				var h = new Hello();

				h.Net			= Node.Net.Address;
				h.Roles			= 0;
				h.Versions		= Versions;
				h.IP			= peer.IP;
				h.Name			= Node.Name;
				h.Permanent		= permanent;
			
				return h;
			}
		}

		protected override Hello CreateInboundHello(IPAddress ip, Hello inbound)
		{
			lock(Lock)
			{
				var h = new Hello();

				h.Net			= Node.Net.Address;
				h.Roles			= 0;
				h.Versions		= Versions;
				h.IP			= ip;
				h.Name			= Node.Name;
			
				return h;
			}
		}

		protected override bool ValidateHello(bool inbound, Hello hello, Peer peer)
		{
			if(!hello.Versions.Any(i => Versions.Contains(i)))
				return false;

			if(inbound)
			{
				//if(hello.Net )
				//	return false;
			}
			else
			{
				if(hello.Net != peer.Net)
					return false;
			}

			if(hello.Name == Node.Name)
			{
				Flow.Log?.ReportError(this, $"To {peer.IP}. It's me" );

				if(peer != null)
				{	
					IgnoredIPs.Add(peer.IP);
					Peers.Remove(peer);
				}

				return false;
			}

			if(peer != null && peer.Status == ConnectionStatus.OK)
			{
				Flow.Log?.ReportError(this, $"From {hello.IP}. Already established" );
				return false;
			}

			return true;
		}

		public Peer GetPeer(IPAddress ip)
		{
			Peer p = null;

			lock(Lock)
			{
				p = Peers.Find(i => i.IP.Equals(ip));
	
				if(p != null)
					return p;
	
				p = new Peer(ip, 0);
				Peers.Add(p);
			}

			return p;
		}

		public List<Peer> RefreshPeers(IEnumerable<Peer> peers)
		{
			lock(Lock)
			{
				var affected = new List<Peer>();
													
				foreach(var i in peers.Where(i => !i.IP.Equals(IP)))
				{
					var p = Peers.Find(j => j.IP.Equals(i.IP));
					
					if(p == null)
					{
						i.Recent = true;
						
						Peers.Add(i);
						affected.Add(i);
					}
					else if(p.Roles != i.Roles || p.Port != i.Port)
					{
						p.Recent = true;
						p.Roles = i.Roles;
						p.Port = i.Port;

						affected.Add(p);
					}
				}

				return affected;
			}
		}

		protected virtual void ProcessConnectivity()
		{
			var needed = Settings.PermanentMin - Peers.Count(i => i.Permanent && i.Status != ConnectionStatus.Disconnected);
		
			foreach(var p in Peers	.Where(p =>	p.Status == ConnectionStatus.Disconnected &&
												DateTime.UtcNow - p.LastTry > TimeSpan.FromSeconds(5))
									.OrderBy(i => i.Retries)
									.ThenBy(i => Settings.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
									.Take(needed))
			{
				OutboundConnect(p, true);
			}

			foreach(var p in Peers.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
			{
				OutboundConnect(p, false);
			}

		}

		protected override void OnConnected(Peer peer)
		{
			//RefreshPeers([peer]);

		}

		public Rp Call<Rp>(IPeer peer, Ppc<Rp> rq) where Rp : PeerResponse
		{
			rq.Peering	= this;

			return peer.Send((PeerRequest)rq) as Rp;
		}

		public R Call<R>(IPAddress ip, Func<Ppc<R>> call, Flow workflow) where R : PeerResponse
		{
			var p = GetPeer(ip);

			Connect(p, workflow);

			var c = call();
			c.Peering	= this;

			return p.Send(c);
		}

		public void Tell(IPAddress ip, PeerRequest requet, Flow workflow)
		{
			var p = GetPeer(ip);

			Connect(p, workflow);

			var c = requet;
			c.Peering	= this;

			p.Post(c);

		}
	}
}
