namespace Uccs.Net;

public class SharePeersRequest : McvPpc<PeerResponse>
{
	public bool					Broadcast { get; set; }
	public Peer[]				Peers { get; set; }
	public override bool		WaitResponse => false;

	public override PeerResponse Execute()
	{
		if(Peers.Length > 1000)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Peering.Lock)
		{
			var newfresh = Peering.RefreshPeers(Peers).ToArray();

			if(Broadcast && newfresh.Any())
			{
				foreach(var i in Peering.Connections.Where(i => i != Peer))
				{
					i.Post(new SharePeersRequest {Broadcast = true, Peers = newfresh});
				}
			}
		}

		return null;
	}
}
