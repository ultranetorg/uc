namespace Uccs.Net;

public class VotePpc : PeerRequest
{
	public Vote		Vote { get; set; }

	public VotePpc()
	{
	}
	
	public override Result Execute()
	{
		var p = Peering as McvPeering;

		if(p.Node.Mcv == null)
			throw new NodeException(NodeError.NotGraph);

			var vp = VoteStatus.OK;
		
			Peering.Statistics.Consensing.Begin();

			try
			{
				lock(p.Mcv.Lock)
					vp = p.Mcv.ProcessIncoming(Vote, p.Synchronization);
			}
			catch(ConfirmationException ex)
			{
				lock(p.Mcv.Lock)
					p.ProcessConfirmationException(ex);
			}
				
			Peering.Statistics.Consensing.End();

			lock(p.Lock)
				if(vp == VoteStatus.OK)
				{
					p.Broadcast(Vote, Peer);
					p.Statistics.AcceptedVotes++;
				}
				else
					p.Statistics.RejectedVotes++;

		return null;
	}
}
