using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using RocksDbSharp;

namespace Uccs.Net
{
	public enum NtnPpcClass : byte
	{
		None = 0, 
		NtnBlock,
		NtnStateHash
	}

	public abstract class NtnPpc<R> : Ppc<R> where R : PeerResponse
	{
		public new NtnTcpPeering	Peering => base.Peering as NtnTcpPeering;
	}

	public abstract class NtnTcpPeering : TcpPeering
	{
		public abstract NtnBlock					ProcessIncoming(byte[] raw, Peer peer);
		public abstract byte[]						GetStateHash(string net);

		protected override IEnumerable<Peer>		PeersToDisconnect => Peers.SelectMany(i => i.Value);

		protected Dictionary<string, List<Peer>>	Peers = [];

		public NtnTcpPeering(Node node, PeeringSettings settings, long roles, Flow flow) : base(node, settings, flow)
		{
			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsGenericType))
			{	
				if(Enum.TryParse<NtnPpcClass>(i.Name.Remove(i.Name.IndexOf("Request")), out var c))
				{
					Codes[i] = (byte)c;
					var x = i.GetConstructor([]);
 					Contructors[typeof(PeerRequest)][(byte)c] = () =>	{
																			var r = x.Invoke(null) as PeerRequest;
																			r.Node = node;
																			return r;
																		};
				}
			}

			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse))))
			{	
				if(Enum.TryParse<NtnPpcClass>(i.Name.Remove(i.Name.IndexOf("Response")), out var c))
				{
					Codes[i] = (byte)c;
					var x = i.GetConstructor([]);
					Contructors[typeof(PeerResponse)][(byte)c] = () => x.Invoke(null);
				}
			}
		}

		public override string ToString()
		{
			return string.Join(",", new object[] {Node.Name,
												  Settings?.IP}.Where(i => i != null));
		}

		protected override void AddPeer(Peer peer)
		{
			Peers[peer.Net].Add(peer);
		}

		protected override void RemovePeer(Peer peer)
		{
			Peers[peer.Net].Remove(peer);
		}

		protected override Peer FindPeer(IPAddress ip)
		{
			return Peers.SelectMany(i => i.Value).FirstOrDefault(i => i.IP.Equals(ip));
		}

		protected override bool Consider(TcpClient client)
		{
			return true;
		}

		protected override Hello CreateOutboundHello(Peer peer, bool permanent)
		{
			lock(Lock)
			{
				var h = new Hello();

				h.Net			= Node.Net.Name;
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

				h.Net			= Node.Net.Name;
				h.Roles			= 0;
				h.Versions		= Versions;
				h.IP			= ip;
				h.Name			= Node.Name;
			
				return h;
			}
		}

		protected override bool Consider(bool inbound, Hello hello, Peer peer)
		{
			if(!hello.Versions.Any(i => Versions.Contains(i)))
				return false;

			if(!inbound)
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
					Peers[hello.Net].Remove(peer);
				}

				return false;
			}

			if(peer != null && peer.Status == ConnectionStatus.OK)
			{
				Flow.Log?.ReportError(this, $"From {peer.IP}. Already established" );
				return false;
			}

			return true;
		}

		public Peer GetPeer(string net, IPAddress ip)
		{
			Peer p = null;

			lock(Lock)
			{
				p = Peers[net].Find(i => i.IP.Equals(ip));
	
				if(p != null)
					return p;
	
				p = new Peer(ip, 0) {Net = net};
				Peers[net].Add(p);
			}

			return p;
		}

		public List<Peer> RefreshPeers(string net, IEnumerable<Peer> peers)
		{
			lock(Lock)
			{
				var affected = new List<Peer>();
													
				foreach(var i in peers.Where(i => !i.IP.Equals(IP)))
				{
					var p = Peers[net].Find(j => j.IP.Equals(i.IP));
					
					if(p == null)
					{
						i.Recent = true;
						
						Peers[net].Add(i);
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

		protected override void ProcessConnectivity()
		{
			foreach(var n in Peers)
			{
				var needed = Settings.PermanentMin - n.Value.Count(i => i.Permanent && i.Status != ConnectionStatus.Disconnected);

				foreach(var p in n.Value.Where(p =>	p.Status == ConnectionStatus.Disconnected &&
													DateTime.UtcNow - p.LastTry > TimeSpan.FromSeconds(5))
										.OrderBy(i => i.Retries)
										.ThenBy(i => Settings.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
										.Take(needed))
				{
					OutboundConnect(p, true);
				}

				foreach(var p in n.Value.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
				{
					OutboundConnect(p, false);
				}
			}
		}

		public Peer ChooseBestPeer(string net, HashSet<Peer> exclusions)
		{
			return Peers[net].Where(i => i.Net == net && (exclusions == null || !exclusions.Contains(i)))
							 .OrderByDescending(i => i.Status == ConnectionStatus.OK)
							 .FirstOrDefault();
		}

		public R Call<R>(string net, Func<NtnPpc<R>> call, Flow workflow, IEnumerable<Peer> exclusions = null) where R : PeerResponse
		{
			return Call(net, (Func<PeerRequest>)call, workflow, exclusions) as R;
		}

		public PeerResponse Call(string net, Func<PeerRequest> call, Flow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					p = ChooseBestPeer(net, tried);
	
					if(p == null)
					{
						tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();
						continue;
					}
				}

				tried.Add(p);

				try
				{
					Connect(p, workflow);

					var c = call();
					c.Peering = this;

					return p.Send(c);
				}
 				catch(NodeException)
 				{
					p.LastFailure[Role.Base] = DateTime.UtcNow;
 				}
				catch(ContinueException)
				{
				}
			}

			throw new OperationCanceledException();
		}

		public void Broadcast(NtnBlock block, Peer skip = null)
		{
			foreach(var i in Peers[block.Net].Where(i => i != skip))
			{
				try
				{
					var v = new NtnBlockRequest {Raw = block.RawPayload} as PeerRequest;
					v.Peering = this;
					i.Post(v);
				}
				catch(NodeException)
				{
				}
			}
		}
	}
}
