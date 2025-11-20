using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using RocksDbSharp;

namespace Uccs.Net;

public abstract class HomoTcpPeering : TcpPeering /// same type of peers
{
	public List<Peer>						Peers = [];
	public IEnumerable<Peer>				Connections => Peers.Where(i => i.Status == ConnectionStatus.OK);
	protected override IEnumerable<Peer>	PeersToDisconnect => Peers;

	public long								Roles;
	public ColumnFamilyHandle				PeersFamily => Database.GetColumnFamily(PeersColumn);
	string									PeersColumn => GetType().Name + nameof(Peers);

	Net										Net;
	RocksDb									Database;

	public HomoTcpPeering(IProgram program, string name, Net net, RocksDb database, PeeringSettings settings, long roles, Flow flow) : base(program, name, settings, flow)
	{
		Roles = roles;
		Database = database;
		Net = net;

		if(!Database.TryGetColumnFamily(PeersColumn, out _))
			Database.CreateColumnFamily(new (), PeersColumn);
	}

	public override void Run()
	{
		LoadPeers();

		base.Run();
	}

	protected override void AddPeer(Peer peer)
	{
		Peers.Add(peer);
	}

	protected override void RemovePeer(Peer peer)
	{
		Peers.Remove(peer);
	}

	protected override Peer FindPeer(IPAddress ip)
	{
		return Peers.Find(i => i.IP.Equals(ip));
	}

	protected override bool Consider(TcpClient client)
	{
		return Connections.Count(i => i.Inbound) < Settings.InboundMax;
	}

	protected override Hello CreateOutboundHello(Peer peer, bool permanent)
	{
		lock(Lock)
		{
			var h = new Hello();

			h.Net			= Net.Name;
			h.Roles			= Roles;
			h.Versions		= Versions;
			h.IP			= peer.IP;
			h.Name			= Name;
			h.Permanent		= permanent;
		
			return h;
		}
	}

	protected override Hello CreateInboundHello(IPAddress ip, Hello inbound)
	{
		lock(Lock)
		{
			var h = new Hello();

			h.Net			= Net.Name;
			h.Roles			= Roles;
			h.Versions		= Versions;
			h.IP			= ip;
			h.Name			= Name;
		
			return h;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, Peer peer)
	{
		if(hello.Permanent && Connections.Count(i => i.Inbound && i.Permanent) >= Settings.PermanentInboundMax)
			return false;

		if(!hello.Versions.Any(i => Versions.Contains(i)))
			return false;

		if(hello.Net != Net.Name)
			return false;

		if(hello.Name == Name)
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

	void LoadPeers()
	{
		using(var i = Database.NewIterator(PeersFamily))
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
			Peers = Net.Initials.Select(i => new Peer(i, Net.PpiPort) {Recent = false, 
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

			Database.Write(b);
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

	protected override void ProcessConnectivity()
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
		peer.Post(new SharePeersPpc {Broadcast = false, Peers = Peers.Where(i => i.Recent).ToArray()});
	}

	public Peer ChooseBestPeer(long role, HashSet<Peer> exclusions)
	{
		return Peers.Where(i => i.Roles.IsSet(role) && (exclusions == null || !exclusions.Contains(i)))
					.OrderByDescending(i => i.Status == ConnectionStatus.OK)
					.FirstOrDefault();
	}

	public Peer Connect(long role, HashSet<Peer> exclusions, Flow flow)
	{
		Peer peer;
			
		while(flow.Active)
		{
			lock(Lock)
			{
				peer = ChooseBestPeer(role, exclusions);

				if(peer == null)
				{
					exclusions?.Clear();
					continue;
				}
			}

			exclusions?.Add(peer);

			try
			{
				Connect(peer, flow);

				return peer;
			}
			catch(NodeException)
			{
			}
		}

		throw new OperationCanceledException();
	}

	public Rp Call<Rp>(IPeer peer, Ppc<Rp> rq) where Rp : PeerResponse
	{
		rq.Peering	= this;

		return peer.Send((FuncPeerRequest)rq) as Rp;
	}

	public R Call<R>(IPAddress ip, Func<Ppc<R>> call, Flow workflow) where R : PeerResponse
	{
		var p = GetPeer(ip);

		Connect(p, workflow);

		var c = call();
		c.Peering	= this;

		return p.Send(c);
	}

	public void Tell(IPAddress ip, ProcPeerRequest requet, Flow workflow)
	{
		var p = GetPeer(ip);

		Connect(p, workflow);

		var c = requet;
		c.Peering	= this;

		p.Post(c);

	}
}
