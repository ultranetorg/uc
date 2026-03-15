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
			var updated = Peering.RefreshPeers(Peers).ToArray();

			if(updated.Any())
			{
				foreach(var i in Peering.Connections.Where(i => i != Peer))
				{
					i.Send(new SharePeersPpc {Peers = updated});
				}
			}
		}

		return null;
	}
}
