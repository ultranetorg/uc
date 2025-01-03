namespace Uccs.Net;

public class NtnBlockRequest : ProcPeerRequest
{
	public byte[]			Raw { get; set; }

	public NtnBlockRequest()
	{
	}
	
	public override void Execute()
	{
		var p = Peering as NtnTcpPeering;

		lock(Peering.Lock)
		{
			var b = p.ProcessIncoming(Raw, Peer);

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
				p.Broadcast(b, Peer);
				//Peering.Statistics.AcceptedVotes++;
			}
			//else
				//Peering.Statistics.RejectedVotes++;

		}
	}
}
