namespace Uccs.Net;

public class SharePeersPpc : ProcPeerRequest
{
	public bool					Broadcast { get; set; }
	public Peer[]				Peers { get; set; }

	public override void Execute()
	{
		if(Peers.Length > 1000)
			throw new RequestException(RequestError.IncorrectRequest);

		lock(Peering.Lock)
		{
			var newfresh = (Peering as McvTcpPeering).RefreshPeers(Peers).ToArray();

			if(Broadcast && newfresh.Any())
			{
				foreach(var i in (Peering as HomoTcpPeering).Connections.Where(i => i != Peer))
				{
					i.Post(new SharePeersPpc {Broadcast = true, Peers = newfresh});
				}
			}
		}
	}
}
