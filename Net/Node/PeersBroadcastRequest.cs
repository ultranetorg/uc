using System.Linq;

namespace Uccs.Net
{
	public class PeersBroadcastRequest : RdcCall<RdcResponse>
	{
		public Peer[]				Peers { get; set; }
		public override bool		WaitResponse => false;

		public override RdcResponse Execute()
		{
			if(Peers.Length > 1000)
				throw new RequestException(RequestError.IncorrectRequest);

			lock(Sun.Lock)
			{
				var newfresh = Sun.RefreshPeers(Peers).ToArray();
	
				if(newfresh.Any())
				{
					foreach(var i in Sun.Connections(null).Where(i => i != Peer))
					{
						i.Post(new PeersBroadcastRequest{Peers = newfresh});
					}
				}
			}

			return null;
		}
	}
}
