namespace Uccs.Net
{
// 	public class ShareNetsRequest : NtnMcvPeerCall<PeerResponse>
// 	{
// 		public class Z
// 		{
// 			public Guid			Id {get; set;}
// 			public Endpoint[]	Peers {get; set;}
// 		}
// 
// 		public Z[]				Nets { get; set; }
// 		public bool				Broadcast { get; set; }
// 		public override bool	WaitResponse => false;
// 
// 		public override PeerResponse Execute()
// 		{
// 			var fresh = new List<ntn.Cluster>();
// 
// 			lock(Node.Lock)
// 			{
// 				foreach(var z in Nets)
// 				{
// 					var kz = Node.GetNet(z.Id);
// 
// 					NexusNode.Cluster fz = null;
// 
// 					foreach(var p in z.Peers)
// 					{
// 						if(kz.Peers.Find(i => i.IP.Equals(p.IP)) is not Endpoint kp)
// 						{
// 							if(fz == null)
// 							{
// 								fz = new (){Net = z.Id, Peers = []};
// 								fresh.Add(fz);
// 							}
// 
// 							kz.Peers.Add(p);
// 							fz.Peers.Add(p);
// 						}
// 						else if(kp.Roles != p.Roles)
// 						{
// 							if(fz == null)
// 							{
// 								fz = new (){Net = z.Id, Peers = []};
// 								fresh.Add(fz);
// 							}
// 
// 							kp.Roles = p.Roles;
// 							fz.Peers.Add(p);
// 						}
// 					}
// 				}
// 
// 				
// 				if(fresh.Any() && Broadcast)
// 				{
// 					foreach(var i in Node.Connections.Where(i => i != Peer))
// 					{
// 						i.Post(new ShareNetsRequest {Broadcast = true,
// 													 Nets = fresh.Select(i => new Z {Id = i.Net, 
// 																					 Peers = i.Peers.ToArray()}).ToArray()});
// 					}
// 				}
// 			}
// 
// 			return null;
// 		}
// 	}
}
