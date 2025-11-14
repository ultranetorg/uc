namespace Uccs.Rdn;

public class RdnNnTcpPeering : NnTcpPeering
{
	public new RdnNode			Node => base.Node as RdnNode;

	public RdnNnTcpPeering(RdnNode node, PeeringSettings settings, long roles, Flow flow) : base(node, settings, roles, flow)
	{
		node.Mcv.Confirmed += (r) =>	{
											foreach(var i in r.ConsensusNnStates)
											{
												var b = new NniBlock();

												b.Net	= node.Net.Name;
												b.State = new() {State = node.Mcv.LastConfirmedRound.Hash,
																 Peers = node.Mcv.LastConfirmedRound.Members.Select(i => new NnState.Peer {IP = i.GraphPpcIPs[0], Port = 0}).ToArray()};
												Broadcast(b);
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

			return d.NnSelfHash;
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
				var n = Node.Mcv.Domains.Latest(hello.Net)?.NnChildNet;
				
				if(n == null)
					return false;

				if(!n.Peers.Any(i => i.IP.Equals(peer.IP)))
					return false;
			}
		}

		return true;
	}

	public override NniBlock ProcessIncoming(byte[] raw, Peer peer)
	{
		lock(Node.Mcv.Lock)
		{
			var b = Node.Mcv.NnBlocks.Find(i => i.RawPayload.SequenceEqual(raw));

			if(b != null)
				return null;

			b = new NniBlock {RawPayload = raw};
			b.Restore();

			var r = Call(b.Net, () => new StateHashNnc {Net = Node.Net.Name}, Flow);

			if(r.Hash.SequenceEqual(b.State.Hash))
			{
				Node.Mcv.NnBlocks.Add(b);

				return b;
			}
			else
				return null;
		}
	}
}
