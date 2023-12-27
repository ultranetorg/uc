using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class PeersBroadcastRequest : RdcRequest
	{
		public Peer[]				Peers { get; set; }
		public override bool		WaitResponse => false;

		public override RdcResponse Execute(Sun sun)
		{
			if(Peers.Length > 1000)
				throw new RequestException();

			lock(sun.Lock)
			{
				var newfresh = sun.RefreshPeers(Peers).ToArray();
	
				if(newfresh.Any())
				{
					foreach(var i in sun.Connections.Where(i => i != Peer))
					{
						i.Send(new PeersBroadcastRequest{Peers = newfresh});
					}
				}
			}

			return null;
		}
	}
}
