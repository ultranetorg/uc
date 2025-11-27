namespace Uccs.Rdn;

public class RdnNnTcpPeering : NnTcpPeering
{
	public RdnNode			Node;

	public RdnNnTcpPeering(RdnNode node, PeeringSettings settings, long roles, Flow flow) : base(node, node.Settings.Name, settings, roles, flow)
	{
		Node = node;
		node.Mcv.Confirmed += (r) =>	{
											foreach(var i in r.ConsensusNnStates)
											{
												var b = new NnBlock();

												b.Net	= node.Net.Name;
												b.State = new() {State = node.Mcv.LastConfirmedRound.Hash,
																 Peers = node.Mcv.LastConfirmedRound.Members.Select(i => new NnState.Peer {IP = i.GraphPpcIPs[0], Port = 0}).ToArray()};
												Broadcast(b);
											}
										};
		Run();
	}

	public byte[] GetStateHash(string net)
	{
		lock(Node.Mcv.Lock)
		{
			var d = Node.Mcv.Domains.Find(net, Node.Mcv.LastConfirmedRound.Id);

			if(d == null)
				throw new EntityException(EntityError.NotFound);

			return d.NnSelfHash;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, NnPeer peer)
	{
		if(base.Consider(inbound, hello, peer) == false)
			return false;

		if(inbound)
		{
			lock(Node.Mcv.Lock)
			{	
				var n = Node.Mcv.Domains.Latest(hello.Net)?.NnChildNet;
				
				if(n == null)
					return false;

				if(!n.Peers.Any(i => i.IP.Equals(peer.IP)))
					return false;
			}
		}

		return true;
	}

//	public NnBlock ProcessIncoming(byte[] raw, Peer peer)
//	{
//		lock(Node.Mcv.Lock)
//		{
//			var b = Node.Mcv.NnBlocks.Find(i => i.RawPayload.SequenceEqual(raw));
//
//			if(b != null)
//				return null;
//
//			b = new NnBlock {RawPayload = raw};
//			b.Restore();
//
//			var r = Call(b.Net, () => new StateHashNnc {Net = Node.Net.Name}, Flow); /// get the hash  from other net for checking
//
//			if(r.Hash.SequenceEqual(b.State.Hash))
//			{
//				Node.Mcv.NnBlocks.Add(b);
//
//				return b;
//			}
//			else
//				return null;
//		}
//	}

//	public R Call<A, R>(Func<Nnc<A, R>> call, Flow workflow, IEnumerable<Peer> exclusions = null) where A : NnRequest where R : class, IBinarySerializable
//	{
//		return Call(call, workflow, exclusions) as R;
//	}
//
//	public CallReturn Call(PeerRequest call, Flow workflow, IEnumerable<Peer> exclusions = null)
//	{
//		var tried = exclusions != null ? [.. exclusions] : new HashSet<Peer>();
//
//		Peer p;
//
//		while(workflow.Active)
//		{
//			Thread.Sleep(1);
//
//			lock(Lock)
//			{
//				p = Node.Peering.ChooseBestPeer((long)Role.Graph, tried);
//
//				if(p == null)
//				{
//					tried = exclusions != null ? [.. exclusions] : new HashSet<Peer>();
//					continue;
//				}
//			}
//
//			tried.Add(p);
//
//			try
//			{
//				Connect(p, workflow);
//
//				var c = new  call();
//				c.Peering = this;
//
//				return p.Send(c);
//			}
//			catch(NodeException)
//			{
//			}
//			catch(ContinueException)
//			{
//			}
//		}
//
//		throw new OperationCanceledException();
//	}

//	public override CallReturn Call(string net, Func<FuncPeerRequest> call, Flow workflow, IEnumerable<Peer> exclusions = null)
//	{
//		if(net == Node.Net.Name)
//		{
//			return Call(call, workflow, exclusions);
//		}
//
//		return base.Call(net, call, workflow, exclusions);
//	}
}
