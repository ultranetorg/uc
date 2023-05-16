using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class PeersBroadcastRequest : RdcRequest
	{
		public IEnumerable<Peer>	Peers { get; set; }
		public override bool		WaitResponse => false;

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				var newfresh = core.RefreshPeers(Peers).ToArray();
	
				if(newfresh.Any())
				{
					foreach(var i in core.Connections.Where(i => i != Peer))
					{
						i.Send(new PeersBroadcastRequest{Peers = newfresh});
					}
				}
			}

			return null;
		}
	}
}
