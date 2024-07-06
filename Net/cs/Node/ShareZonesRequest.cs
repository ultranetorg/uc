using System;
using System.Collections.Generic;
using System.Linq;

namespace Uccs.Net
{
	public class ShareZonesRequest : InterzoneCall<PeerResponse>
	{
		public class Z
		{
			public Guid			Id {get; set;}
			public InterPeer[]	Peers {get; set;}
		}

		public Z[]				Zones { get; set; }
		public bool				Broadcast { get; set; }
		public override bool	WaitResponse => false;

		public override PeerResponse Execute()
		{
			//if(Peers.Length > 1000)
			//	throw new RequestException(RequestError.IncorrectRequest);
		
			var fresh = new List<ZonePeers>();

			lock(Node.Lock)
			{
													
				foreach(var z in Zones)
				{
					var kz = Node.GetZone(z.Id);

					ZonePeers fz = null;

					foreach(var p in z.Peers)
					{
						if(kz.Peers.Find(i => i.IP.Equals(p.IP)) is not InterPeer kp)
						{
							if(fz == null)
							{
								fz = new (){Zone = z.Id, Peers = []};
								fresh.Add(fz);
							}

							kz.Peers.Add(p);
							fz.Peers.Add(p);
						}
						else if(kp.Roles != p.Roles)
						{
							if(fz == null)
							{
								fz = new (){Zone = z.Id, Peers = []};
								fresh.Add(fz);
							}

							kp.Roles = p.Roles;
							fz.Peers.Add(p);
						}
					}
				}

			}
				
			if(fresh.Any() && Broadcast)
			{
				foreach(var i in Node.Connections.Where(i => i != Peer))
				{
					i.Post(new ShareZonesRequest {	Broadcast = true,
													Zones = fresh.Select(i => new Z {	Id = i.Zone, 
																						Peers = i.Peers.ToArray()}).ToArray()});
				}
			}

			return null;
		}
	}
}
