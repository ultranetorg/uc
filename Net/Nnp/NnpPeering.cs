using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

public class NnpPeering : TcpPeering<NnpPeer>
{
	Mcv											Mcv;
	protected List<NnpPeer>						Peers = [];
	protected override IEnumerable<NnpPeer>		PeersToDisconnect => Peers;

	protected override NnpPeer					CreatePeer() => new ();

	public NnpPeering(IProgram program, Mcv mcv, string name, PeeringSettings settings, long roles, Flow flow) : base(program, name, settings, flow)
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

	public override Hello WaitHello(TcpClient client)
	{
		var r = new Reader(client.GetStream());
		var h = new NnHello();

		h.Read(r);
		
		return h;
	}

	protected override void AddPeer(NnpPeer peer)
	{
		Peers.Add(peer);
	}

	protected override void RemovePeer(NnpPeer peer)
	{
		Peers.Remove(peer);
	}

	protected override NnpPeer FindPeer(Endpoint ip)
	{
		return Peers.FirstOrDefault(i => i.EP.Equals(ip));
	}

	protected override bool Consider(TcpClient client)
	{
		return true;
	}

	protected override Hello CreateOutboundHello(NnpPeer peer, bool permanent)
	{
		lock(Lock)
		{
			var h = new NnHello
					{
						Nets = peer.Nets,
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
			var h = new NnHello
					{
						Nets = (inbound as NnHello).Nets,
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

		//if(!inbound)
		//{
		//	if(hello.Net != peer.Net)
		//		return false;
		//}

		if(hello.Name == Name)
		{
			Flow.Log?.ReportError(this, $"To {peer.EP}. It's me" );

			if(peer != null)
			{	
				IgnoredIPs.Add(peer.EP.IP);
				Peers.Remove(peer);
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

	public NnpPeer GetPeer(Endpoint endpoint, List<string> nets)
	{
		NnpPeer p = null;

		lock(Lock)
		{
			p = Peers.Find(i => i.EP == endpoint);

			if(p != null)
				return p;

			p = new NnpPeer(endpoint){Nets = nets};
			Peers.Add(p);
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
		foreach(var i in Peers.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
		{
			OutboundConnect(i, true);
		}
	}

	public NnpPeer ChooseBestPeer(string net, HashSet<NnpPeer> exclusions)
	{
		/// try existing peers
		var p = Peers.Where(i => i.Nets.Contains(net) && (exclusions == null || !exclusions.Contains(i)))
					 .OrderByDescending(i => i.Status == ConnectionStatus.OK)
					 .FirstOrDefault();

		if(p == null) /// get a new ones from Mcv
		{
			lock(Mcv.Lock)
			{	
				var pers = Peers.Where(i => i.Nets.Contains(net));

				var f = Mcv.Friends.Find(net)
						??
						throw new EntityException(EntityError.NotFound);

				var a = f.Peers.OrderByRandom().FirstOrDefault(x => pers == null || !pers.Any(y => x != y.EP));

				if(a != null)
					p = GetPeer(a, [net]);
			}
		}

		return p;
	}

	public virtual Result Call(string from, string to, Argumentation call, Flow flow)
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
					p = ChooseBestPeer(to, tried);

					if(p == null)
					{
						reset();
						continue;
					}
				}

				tried.Add(p);
				Connect(p, flow);

				return p.Call(from, to, call, flow);
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

	public void Broadcast(string from, string to, TransferRequestNna message, NnpPeer skip = null)
	{
		foreach(var i in Peers.Where(i => i.Nets.Contains(to) && i != skip))
		{
			try
			{
				i.Send(from, to, message);
			}
			catch(NodeException)
			{
			}
		}
	}
}
