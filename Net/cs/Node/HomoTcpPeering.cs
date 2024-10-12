using System.Collections.Immutable;
using System.Net;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class HomoTcpPeering : TcpPeering
	{
		Net											Net;
		public long									Roles;
		public ColumnFamilyHandle					PeersFamily => Node.Database.GetColumnFamily(Net.Address + nameof(Peers));
		public Guid									PeerId;

		public HomoTcpPeering(Node node, Net net, PeeringSettings settings, long roles, Flow flow) : base(node, settings, flow)
		{
			Net = net;
			Roles = roles;

			Flow.Log?.Report(this, $"Ultranet Node {Version}");
			Flow.Log?.Report(this, $"Runtime: {Environment.Version}");
			Flow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			//Flow.Log?.Report(this, $"Profile: {Settings.Profile}");

			if(!Node.Database.TryGetColumnFamily(net.Address + nameof(Peers), out var cf))
				Node.Database.CreateColumnFamily(new (), net.Address + nameof(Peers));
		}

		public virtual void Run()
		{
			if(PeerId != Guid.Empty)
				throw new NodeException(NodeError.AlreadyRunning);

			PeerId = Guid.NewGuid();

			LoadPeers();

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
			return string.Join(",", new string[] {Node.Name,
												  Settings.IP != null ? IP.ToString() : null}.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		protected override Hello CreateHello(IPAddress ip, bool permanent)
		{
			lock(Lock)
			{
				var h = new Hello();

				h.Net			= Net.Address;
				h.Roles			= Roles;
				h.Versions		= Versions;
				h.IP			= ip;
				h.Name			= Node.Name;
				h.PeerId		= PeerId;
				h.Permanent		= permanent;
			
				return h;
			}
		}

		protected override bool ValidateHello(Hello hello, Peer peer)
		{
			if(!hello.Versions.Any(i => Versions.Contains(i)))
				return false;

			if(hello.Net != Net.Address)
				return false;

			if(hello.PeerId == PeerId)
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

		void LoadPeers()
		{
			using(var i = Node.Database.NewIterator(PeersFamily))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				var p = new Peer();
					p.IP = new IPAddress(i.Key());
					p.Recent = false;
	 				p.LoadNode(new BinaryReader(new MemoryStream(i.Value())));
	 				Peers.Add(p);
				}
			}
			
			if(Peers.Any())
			{
				Flow.Log?.Report(this, "PEE loaded", $"n={Peers.Count}");
			}
			else
			{
				Peers = Net.Initials?.Select(i => new Peer(i, Net.Port) {Recent = false, 
																		 LastSeen = DateTime.MinValue}).ToList() ?? [];

				SavePeers(Peers);
			}
		}

		public void SavePeers(IEnumerable<Peer> peers)
		{
			using(var b = new WriteBatch())
			{
				foreach(var i in peers)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
					i.SaveNode(w);
					b.Put(i.IP.GetAddressBytes(), s.ToArray(), PeersFamily);
				}
	
				Node.Database.Write(b);
			}
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
	
				SavePeers(affected);

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
