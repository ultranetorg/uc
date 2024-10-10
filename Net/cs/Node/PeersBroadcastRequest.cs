namespace Uccs.Net
{
	public class PeersBroadcastRequest : PeerCall<PeerResponse>
	{
		public Peer[]				Peers { get; set; }
		public override bool		WaitResponse => false;

		public override PeerResponse Execute()
		{
			if(Peers.Length > 1000)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Peering.Lock)
			{
				var newfresh = Peering.RefreshPeers(Peers).ToArray();
	
				if(newfresh.Any())
				{
					foreach(var i in Peering.Connections.Where(i => i != Peer))
					{
						i.Post(new PeersBroadcastRequest{Peers = newfresh});
					}
				}
			}

			return null;
		}
	}
}
