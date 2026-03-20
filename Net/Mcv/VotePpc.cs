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

		//lock(p.Lock)
		{
			var vp = VoteStatus.OK;
		
			//lock(p.Mcv.Lock)
			{
				Peering.Statistics.Consensing.Begin();
				

				try
				{
					lock(p.Mcv.Lock)
						vp = p.Mcv.ProcessIncoming(Vote, p.Synchronization == Synchronization.Downloading);
				}
				catch(ConfirmationException ex)
				{
					lock(p.Mcv.Lock)
						p.ProcessConfirmationException(ex);
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
			}

			lock(p.Lock)
				if(vp == VoteStatus.OK)
				{
					p.Broadcast(Vote, Peer);
					p.Statistics.AcceptedVotes++;
				}
				else
					p.Statistics.RejectedVotes++;
		}

		return null;
	}
}
