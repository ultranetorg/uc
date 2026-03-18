namespace Uccs.Net;

public class SharePeersPpc : PeerRequest
{
	//public bool					Broadcast { get; set; }
	public HomoPeer[]			Peers { get; set; }

	public override Result Execute()
	{
		if(Peers.Length > 1000)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Peering.Lock)
		{
			Peering.RefreshPeers(Peers, Peer);
		}

		return null;
	}
}
