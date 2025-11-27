using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

//
//public interface NnPpc
//{
//	public NnTcpPeering		Peering { get ;set; }
//	public Return			Execute();
//}
//
//public class HolderClassesNnPpc : HolderClassesNna, NnPpc
//{
//	public NnTcpPeering		Peering { get; set; }
//
//	public Return Execute()
//	{
//	}
//}

public class NnTcpPeering : TcpPeering<NnPeer>
{
	protected Dictionary<string, List<NnPeer>>	Peers = [];
	protected override IEnumerable<NnPeer>		PeersToDisconnect => Peers.SelectMany(i => i.Value);

	public static string						GetName(IPAddress ip) => "NnPeeringIpcServer" + ip.ToString();
	protected override NnPeer					CreatePeer() => new ();

	public NnTcpPeering(IProgram program, string name, PeeringSettings settings, long roles, Flow flow) : base(program, name, settings, flow)
	{
		///Register(typeof(NncClass), node);
	}

	public override string ToString()
	{
		return string.Join(",", new object[]
								{
									Name, 
									Settings?.IP}.Where(i => i != null)
								);
	}

	protected override void AddPeer(NnPeer peer)
	{
		Peers[peer.Net].Add(peer);
	}

	protected override void RemovePeer(NnPeer peer)
	{
		Peers[peer.Net].Remove(peer);
	}

	protected override NnPeer FindPeer(IPAddress ip)
	{
		return Peers.SelectMany(i => i.Value).FirstOrDefault(i => i.IP.Equals(ip));
	}

	protected override bool Consider(TcpClient client)
	{
		return true;
	}

	protected override Hello CreateOutboundHello(NnPeer peer, bool permanent)
	{
		lock(Lock)
		{
			var h = new Hello();

			h.Net			= peer.Net;
			h.Name			= Name;
			h.Roles			= 0;
			h.Versions		= Versions;
			h.IP			= peer.IP;
			h.Permanent		= permanent;
		
			return h;
		}
	}

	protected override Hello CreateInboundHello(IPAddress ip, Hello inbound)
	{
		lock(Lock)
		{
			var h = new Hello();

			h.Net			= inbound.Net;
			h.Name			= Name;
			h.Roles			= 0;
			h.Versions		= Versions;
			h.IP			= ip;
		
			return h;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, NnPeer peer)
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

	public NnPeer GetPeer(string net, IPAddress ip)
	{
		NnPeer p = null;

		lock(Lock)
		{
			p = Peers[net].Find(i => i.IP.Equals(ip));

			if(p != null)
				return p;

			p = new NnPeer(ip, 0) {Net = net};
			Peers[net].Add(p);
		}

		return p;
	}

	public List<NnPeer> RefreshPeers(string net, IEnumerable<NnPeer> peers)
	{
		lock(Lock)
		{
			var affected = new List<NnPeer>();
												
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

	public NnPeer ChooseBestPeer(string net, HashSet<NnPeer> exclusions)
	{
		return Peers.TryGetValue(net, out var n) ? n.Where(i => i.Net == net && (exclusions == null || !exclusions.Contains(i)))
													.OrderByDescending(i => i.Status == ConnectionStatus.OK)
													.FirstOrDefault()
												 : null;
	}

	//public Rp Send<A, Rp>(Nnc<A, Rp> call) where A : NnRequest where Rp : class, IBinarySerializable => Send(new IppFuncRequest {Arguments = call}).Return as Rp;
	//
	//public R Call<R>(string net, Func<Nnc<R>> call, Flow workflow, IEnumerable<NnPeer> exclusions = null) where R : PeerResponse
	//{
	//	return Call(net, (Func<FuncPeerRequest>)call, workflow, exclusions) as R;
	//}

	public virtual Return Call(string net, Argumentation call, Flow workflow, IEnumerable<NnPeer> exclusions = null)
	{
		var tried = exclusions != null ? [.. exclusions] : new HashSet<NnPeer>();

		NnPeer p;

		while(workflow.Active)
		{
			Thread.Sleep(1);

			lock(Lock)
			{
				p = ChooseBestPeer(net, tried);

				if(p == null)
				{
					tried = exclusions != null ? [.. exclusions] : new HashSet<NnPeer>();
					continue;
				}
			}

			tried.Add(p);

			try
			{
				Connect(p, workflow);

				return p.Call(call);
			}
			catch(NodeException)
			{
			}
			catch(ContinueException)
			{
			}
		}

		throw new OperationCanceledException();
	}

	public void Broadcast(NnBlock block, NnPeer skip = null)
	{
///		foreach(var i in Peers[block.Net].Where(i => i != skip))
///		{
///			try
///			{
///				var v = new BlockNnc {Raw = block.RawPayload};
///				v.Peering = this;
///				i.Send(v);
///			}
///			catch(NodeException)
///			{
///			}
///		}
	}
}
