namespace Uccs.Net;

public class VoteRequest : McvPpc<PeerResponse>
{
	public Vote				Vote { get; set; }
	public override bool	WaitResponse => false;

	public VoteRequest()
	{
	}
	
	public override PeerResponse Execute()
	{
		if(Node.Mcv == null)
			throw new NodeException(NodeError.NotBase);

		lock(Peering.Lock)
		{
			lock(Mcv.Lock)
			{
				Peering.Statistics.Consensing.Begin();
				
				var accepted = false;

				try
				{
					accepted = Peering.ProcessIncoming(Vote, false);
				}
				catch(ConfirmationException ex)
				{
					Peering.ProcessConfirmationException(ex);
					accepted = true; /// consensus failed but the vote looks valid
				}
								
				Peering.Statistics.Consensing.End();

				//if(Peering.Synchronization == Synchronization.Synchronized)
				//{
				//	//var r = sun.Mcv.FindRound(v.RoundId);
				//	var _v = v.Round?.Votes.Find(i => i.Signature.SequenceEqual(v.Signature)); 
				//
				//	if(_v != null) /// added or existed
				//	{
				//		if(accepted) /// for the added vote
				//		{
				//			var m = Mcv.LastConfirmedRound.Members.Find(i => i.Address == v.Generator);
				//			
				//			if(m != null)
				//			{
				//				//m.BaseRdcIPs	= v.BaseRdcIPs.ToArray();
				//				//m.SeedHubRdcIPs	= v.SeedHubRdcIPs.ToArray();
				//				m.Proxy			= Peer;
				//			}
				//		}
				//		else if(_v.Peers != null && !_v.Peers.Contains(Peer)) /// for the existing vote
				//			_v.BroadcastConfirmed = true;
				//	}
				//}

				if(accepted)
				{
					Peering.Broadcast(Vote, Peer);
					Peering.Statistics.AcceptedVotes++;
				}
				else
					Peering.Statistics.RejectedVotes++;

			}
		}

		return null; 
	}
}
