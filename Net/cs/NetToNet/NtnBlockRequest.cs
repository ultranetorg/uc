namespace Uccs.Net;

public class NtnBlockRequest : NtnPpc<PeerResponse>
{
	public byte[]			Raw { get; set; }
	public override bool	WaitResponse => false;

	public NtnBlockRequest()
	{
	}
	
	public override PeerResponse Execute()
	{
		lock(Peering.Lock)
		{
			var b = Peering.ProcessIncoming(Raw, Peer);

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

			if(b != null)
			{
				Peering.Broadcast(b, Peer);
				//Peering.Statistics.AcceptedVotes++;
			}
			//else
				//Peering.Statistics.RejectedVotes++;

			return null; 
		}
	}
}
