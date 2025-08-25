namespace Uccs.Rdn;

public class RdnNtnTcpPeering : NtnTcpPeering
{
	public new RdnNode			Node => base.Node as RdnNode;

	public RdnNtnTcpPeering(RdnNode node, PeeringSettings settings, long roles, Flow flow) : base(node, settings, roles, flow)
	{
		node.Mcv.ConsensusConcluded +=	(r, reached) =>
										{
											if(reached)
											{
												foreach(var i in r.ConsensusNtnStates)
												{
													var b = new NtnBlock();

													b.Net	= node.Net.Name;
													b.State = new() {State = node.Mcv.LastConfirmedRound.Hash,
																	 Peers = node.Mcv.LastConfirmedRound.Members.Select(i => new NtnState.Peer {IP = i.GraphPpcIPs[0], Port = 0}).ToArray()};
													Broadcast(b);
												}
											}
										};
		Run();
	}

	public override byte[] GetStateHash(string net)
	{
		lock(Node.Mcv.Lock)
		{
			var d = Node.Mcv.Domains.Find(net, Node.Mcv.LastConfirmedRound.Id);

			if(d == null)
				throw new EntityException(EntityError.NotFound);

			return d.NtnSelfHash;
		}
	}

	protected override bool Consider(bool inbound, Hello hello, Peer peer)
	{
		if(base.Consider(inbound, hello, peer) == false)
			return false;

		if(inbound)
		{
			lock(Node.Mcv.Lock)
			{	
				var n = Node.Mcv.Domains.Latest(hello.Net)?.NtnChildNet;
				
				if(n == null)
					return false;

				if(!n.Peers.Any(i => i.IP.Equals(peer.IP)))
					return false;
			}
		}

		return true;
	}

	public override NtnBlock ProcessIncoming(byte[] raw, Peer peer)
	{
		lock(Node.Mcv.Lock)
		{
			var b = Node.Mcv.NtnBlocks.Find(i => i.RawPayload.SequenceEqual(raw));

			if(b != null)
				return null;

			b = new NtnBlock {RawPayload = raw};
			b.Restore();

			var r = Call(b.Net, () => new NtnStateHashRequest {Net = Node.Net.Name}, Flow);

			if(r.Hash.SequenceEqual(b.State.Hash))
			{
				Node.Mcv.NtnBlocks.Add(b);

				return b;
			}
			else
				return null;
		}
	}
}
