using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;


public class IccpHello : Hello
{
	public List<string>	Nets;

	public override void Write(Writer writer)
	{
		base.Write(writer);
		writer.Write(Nets, writer.WriteASCII);
	}

	public override void Read(Reader reader)
	{
		base.Read(reader);
		Nets = reader.ReadList(reader.ReadASCII);
	}
}

public class IccpPeering : TcpPeering<IccpPeer>
{
	public Mcv									Mcv;
	protected List<IccpPeer>					Peers = [];
	protected override IEnumerable<IccpPeer>	PeersToDisconnect => Peers;
	Func<List<string>>							GetNets;
	public LcpServer							Lcp;
	const int									SubnetPeerBunch = 16;

	protected override IccpPeer					CreatePeer() => new ();

	public IccpPeering(IProgram program, string name, PeeringSettings settings, LcpServer lcp, Func<List<string>> nets, Flow flow) : base(program, name, settings, flow)
	{
		Lcp = lcp;
		GetNets = nets;
		Constructor = Iccp.Constructor;
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
		var h = new IccpHello();

		h.Read(r);
		
		return h;
	}

	protected override void AddPeer(IccpPeer peer)
	{
		Peers.Add(peer);
	}

	protected override void RemovePeer(IccpPeer peer)
	{
		Peers.Remove(peer);
	}

	protected override IccpPeer FindPeer(Endpoint ip)
	{
		return Peers.FirstOrDefault(i => i.EP.Equals(ip));
	}

	protected override bool Consider(TcpClient client)
	{
		return true;
	}

	protected override Hello CreateOutboundHello(IccpPeer peer, bool permanent)
	{
		lock(Lock)
		{
			var h = new IccpHello
					{
						Nets = GetNets(),
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
			var h = new IccpHello
					{
						Nets = (inbound as IccpHello).Nets,
						Name = Name,
						Roles = 0,
						Versions = Versions,
						YourIP = ip,
						MyPort = Settings.EP.Port
					};

			return h;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, IccpPeer peer)
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

	public IccpPeer GetPeer(Endpoint endpoint, List<string> nets)
	{
		IccpPeer p = null;

		lock(Lock)
		{
			p = Peers.Find(i => i.EP == endpoint);

			if(p != null)
				return p;

			p = new IccpPeer(endpoint){Nets = nets};
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
		IccpPeer[] peers;
		
		lock(Lock)
			peers = Peers.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected).ToArray();

		foreach(var i in peers)
		{
			OutboundConnect(i, true);
		}
	}

	public virtual IccpResult Call(string from, string to, IccpArgumentation call, Flow flow)
	{
		HashSet<IccpPeer> tried;
		
		void reset()
		{
			tried = [];
		}

		reset();

		while(flow.Active)
		{
			Thread.Sleep(1);

			try
			{
				IccpPeer p;

				lock(Lock)
					p = Peers.Where(i => i.Nets.Contains(to) && (tried == null || !tried.Contains(i)))
							 .OrderByDescending(i => i.Status == ConnectionStatus.OK)
							 .FirstOrDefault();

				if(p == null) /// get a new ones from Mcv
				{
					if(to == Net.Root)
					{
						var l = Lcp.Connections.Cast<IccpLcpConnection>().FirstOrDefault(i => i.Net == to);

						lock(Lock)
							p = GetPeer((l.Call(null, to, new PeersIcca {}, flow) as PeersIccr).Peers.Random(), [to]);
					} 
					else
					{
						string x = to;

						if(!x.EndsWith($".{Net.Root}"))
							x += $".{Net.Root}";

						var nets = x.Split('.').ToArray();
						int d = nets.Length;

						string take(int depth) => string.Join('.', nets[^depth..]);

						IccpLcpConnection l = null;

						while(flow.Active)
						{
							l = Lcp.Connections.Cast<IccpLcpConnection>().FirstOrDefault(c => c.Net == take(d));

							if(l != null)
								break;
							else
								Thread.Sleep(100);
					
							if(d == 1)
								break;


							d--;
						}

						var peers = (l.Call(null, take(d), new SubnetPeersIcca {Name = nets[^(d+1)]}, flow) as SubnetPeersIccr).Peers;
				
						p = Connect(peers.Take(SubnetPeerBunch).Select(i => GetPeer(i, [nets[^(d+1)]])), flow);

						for(int i = d+1; i < nets.Length; i++)
						{
							p = Connect((p.Call(null, take(d), new SubnetPeersIcca {Name = nets[^(d+1)]}, flow) as SubnetPeersIccr).Peers.Take(SubnetPeerBunch).Select(i => GetPeer(i, [nets[^(d+1)]])), flow);
						}
					}
				}

				if(p == null)
				{
					reset();
					continue;
				}

				tried.Add(p);

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

	public void Broadcast(string from, string to, TransferRequestIcca message, IccpPeer skip = null)
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
