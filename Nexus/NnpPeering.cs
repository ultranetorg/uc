using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class NnpPeering : TcpPeering<NnpPeer>
{
	RdnMcv										Mcv;
	protected Dictionary<string, List<NnpPeer>>	Peers = [];
	protected override IEnumerable<NnpPeer>		PeersToDisconnect => Peers.SelectMany(i => i.Value);

	protected override NnpPeer					CreatePeer() => new ();

	public NnpPeering(IProgram program, RdnMcv mcv, string name, PeeringSettings settings, long roles, Flow flow) : base(program, name, settings, flow)
	{
		Mcv = mcv;
		///Register(typeof(NncClass), node);
	}

	public override string ToString()
	{
		return string.Join(",", new object[]
								{
									Name, 
									Settings.EP
								}.Where(i => i != null));
	}

	protected override void AddPeer(NnpPeer peer)
	{
		Peers[peer.Net].Add(peer);
	}

	protected override void RemovePeer(NnpPeer peer)
	{
		Peers[peer.Net].Remove(peer);
	}

	protected override NnpPeer FindPeer(Endpoint ip)
	{
		return Peers.SelectMany(i => i.Value).FirstOrDefault(i => i.EP.Equals(ip));
	}

	protected override bool Consider(TcpClient client)
	{
		return true;
	}

	protected override Hello CreateOutboundHello(NnpPeer peer, bool permanent)
	{
		lock(Lock)
		{
			var h = new Hello
					{
						Net = peer.Net,
						Name = Name,
						Roles = 0,
						Versions = Versions,
						YourIP = peer.EP.IP,
						MyPort = Settings.EP.Port,
						Permanent = permanent
					};

			return h;
		}
	}

	protected override Hello CreateInboundHello(IPAddress ip, Hello inbound)
	{
		lock(Lock)
		{
			var h = new Hello
					{
						Net = inbound.Net,
						Name = Name,
						Roles = 0,
						Versions = Versions,
						YourIP = ip,
						MyPort = Settings.EP.Port
					};

			return h;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, NnpPeer peer)
	{
		if(!hello.Versions.Any(i => Versions.Contains(i)))
			return false;

		if(!inbound)
		{
			if(hello.Net != peer.Net)
				return false;
		}

		if(hello.Name == Name)
		{
			Flow.Log?.ReportError(this, $"To {peer.EP}. It's me" );

			if(peer != null)
			{	
				IgnoredIPs.Add(peer.EP.IP);
				Peers[hello.Net].Remove(peer);
			}

			return false;
		}

		if(peer != null && peer.Status == ConnectionStatus.OK)
		{
			Flow.Log?.ReportError(this, $"From {peer.EP}. Already established" );
			return false;
		}

		return true;
	}

	public NnpPeer GetPeer(string net, Endpoint endpoint)
	{
		NnpPeer p = null;

		lock(Lock)
		{
			p = Peers[net].Find(i => i.EP != endpoint);

			if(p != null)
				return p;

			p = new NnpPeer(endpoint) {Net = net};
			Peers[net].Add(p);
		}

		return p;
	}

	//public List<NnpPeer> RefreshPeers(string net, IEnumerable<NnpPeer> peers)
	//{
	//	lock(Lock)
	//	{
	//		var affected = new List<NnpPeer>();
	//											
	//		foreach(var i in peers.Where(i => i.EP != EP))
	//		{
	//			var p = Peers[net].Find(j => j.EP == i.EP);
	//			
	//			if(p == null)
	//			{
	//				i.Recent = true;
	//				
	//				Peers[net].Add(i);
	//				affected.Add(i);
	//			}
	//			else if(p.Roles != i.Roles)
	//			{
	//				p.Recent = true;
	//				p.Roles = i.Roles;
	//
	//				affected.Add(p);
	//			}
	//		}
	//
	//		return affected;
	//	}
	//}

	protected override void Main()
	{
		foreach(var i in Peers.SelectMany(i => i.Value).Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
		{
			OutboundConnect(i, true);
		}
	}

	public NnpPeer ChooseBestPeer(string net, HashSet<NnpPeer> exclusions)
	{
		NnpPeer p = null;
		
		if(Peers.TryGetValue(net, out var n)) /// try existing peers
		{
			p = n.Where(i => i.Net == net && (exclusions == null || !exclusions.Contains(i)))
				 .OrderByDescending(i => i.Status == ConnectionStatus.OK)
				 .FirstOrDefault();
		}

		if(p == null) /// get a new ones from Mcv
		{
			lock(Mcv.Lock)
			{	
				Peers.TryGetValue(net, out var pers);

				var s = Mcv.Subnets.Find(net);

				if(s == null)
					throw new EntityException(EntityError.NotFound);

				var a = s.Peers.OrderByRandom().FirstOrDefault(x => pers == null || !pers.Any(y => x != y.EP));

				if(a != null)
					p = GetPeer(net, a);
			}
		}

		return p;
	}

	public virtual Result Call(string net, Argumentation call, Flow flow)
	{
		HashSet<NnpPeer> tried;
		
		void reset()
		{
			tried = [];
		}

		reset();

		NnpPeer p;

		while(flow.Active)
		{
			Thread.Sleep(1);

			try
			{
				lock(Lock)
				{
					p = ChooseBestPeer(net, tried);

					if(p == null)
					{
						reset();
						continue;
					}
				}

				tried.Add(p);
				Connect(p, flow);

				return p.Call(call, flow);
			}
			catch(ContinueException)
			{
			}
			catch(NodeException)
			{
			}
			catch(ObjectDisposedException)
			{
			}
			catch(IOException)
			{
			}
			catch(OperationCanceledException)
			{
			}
		}

		throw new OperationCanceledException();
	}

	public void Broadcast(TransactionNna message, NnpPeer skip = null)
	{
		foreach(var i in Peers[message.Net].Where(i => i != skip))
		{
			try
			{
				i.Send(message);
			}
			catch(NodeException)
			{
			}
		}
	}
}
