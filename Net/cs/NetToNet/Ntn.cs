using System.Net;

namespace Uccs.Net;

public class NtnNodeSettings : SavableSettings
{
	public bool IsHub { get; set; }

	public NtnNodeSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public NtnNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
	}
}

public class Endpoint : IBinarySerializable
{
	public IPAddress	IP {get; set;}
	public long			Roles {get; set;}

	public Endpoint()
	{
	}

	public Endpoint(IPAddress ip, long roles)
	{
		IP = ip;
		Roles = roles;
	}

	public void Read(BinaryReader reader)
	{
		IP = reader.ReadIPAddress();
		Roles = reader.Read7BitEncodedInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(IP);
		writer.Write7BitEncodedInt64(Roles);
	}
}

//	public class Cluster
//	{
//		public string			Net {get; set;}
//		public List<Endpoint>	Peers {get; set;}
//	}

// 	public class NtnTcpPeering : TcpPeering
// 	{
// 
// 		public List<Cluster>			Clusters = [];
// 		public override long			Roles => 0;
// 		//public new NtnNodeSettings		Settings => base.Settings as NexusNodeSettings;
// 		bool							MinimalPeersReached;
// 
// 		public NtnTcpPeering(string name, string profile, NodeSettings settings, Flow workflow) : base(name, null, null, settings ?? new NodeSettings(profile), workflow)
// 		{
// 			if(Settings.Api != null)
// 			{
// 				ApiServer = new NodeApiServer(this, Flow);
// 			}
// 
// 			RunPeer();
// 		}
// 
// 		public override string ToString()
// 		{
// 			return string.Join(",", new string[] {Connections.Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
// 												  Settings.Peering.IP?.ToString()}.Where(i => !string.IsNullOrWhiteSpace(i)));
// 		}
// 
// 		protected override void Share(Peer peer)
// 		{
// 			peer.Post(new ShareNetsRequest {Broadcast = false,
// 											Nets = Clusters.Select(i => new ShareNetsRequest.Z {Id = i.Net, 
// 																								Peers = i.Peers.ToArray()}).ToArray()});
// 		}
// 
// 		public Cluster GetNet(Guid id)
// 		{
// 			if(Clusters.Find(i => i.Net == id) is not Cluster z)
// 			{ 
// 				z = new Cluster {Net = id, Peers = []};
// 				Clusters.Add(z);
// 			}
// 
// 			return z;
// 		}
// 
// 		public void Declare(Guid netid, long roles)
// 		{
// 			var n = GetNet(netid);
// 			var p = new Endpoint(Settings.Peering.IP ?? IP, roles);
// 			n.Peers.Add(p);
// 
// 			foreach(var c in Connections)
// 			{
// 				c.Post(new ShareNetsRequest {Broadcast = true, Nets = [new ShareNetsRequest.Z {Id = netid, Peers = [p]}]});
// 			}
// 		}
// 
// 		protected override void ProcessConnectivity()
// 		{
// 			base.ProcessConnectivity();
// 
// 			if(!MinimalPeersReached && 
// 				Connections.Count(i => i.Permanent) >= Settings.Peering.PermanentMin)
// 			{
// 				MinimalPeersReached = true;
// 				Flow.Log?.Report(this, $"Minimal peers reached");
// 			}
// 		}
// 	}
