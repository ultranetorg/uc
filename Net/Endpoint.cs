using System.Net;

namespace Uccs.Net;

public class Endpoint : IBinarySerializable, IEquatable<Endpoint>
{
	public IPAddress	IP {get; set;}
	public ushort		Port {get; set;}
	//public long			Roles {get; set;}

	public byte[]		Raw => [..IP.GetAddressBytes(), ..BitConverter.GetBytes(Port)];

	public Endpoint()
	{
	}

	public Endpoint(IPAddress iP, ushort port)
	{
		IP = iP;
		Port = port;
		//Roles = roles;
	}

	public override string ToString()
	{
		return $"{IP}:{Port}";
	}

	public void Read(BinaryReader reader)
	{
		IP = reader.ReadIPAddress();
		Port = reader.ReadUInt16();
		//Roles = reader.Read7BitEncodedInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(IP);
		writer.Write(Port);
		//writer.Write7BitEncodedInt64(Roles);
	}

	public override bool Equals(object o)
	{
		return o is Endpoint e && Equals(e);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(IP, Port);
	}

	public bool Equals(Endpoint e)
	{
		return IP.Equals(e.IP) && Port == e.Port;
	}

	public static Endpoint Parse(string v)
	{
		var i = v.IndexOf(':');

		return new Endpoint(IPAddress.Parse(v.AsSpan(0, i)), ushort.Parse(v.AsSpan(i + 1)));
	}
}


//	public class Cluster
//	{
//		public string			Net {get; set;}
//		public List<Endpoint>	Peers {get; set;}
//	}

// 	public class NnTcpPeering : TcpPeering
// 	{
// 
// 		public List<Cluster>			Clusters = [];
// 		public override long			Roles => 0;
// 		//public new NnNodeSettings		Settings => base.Settings as NexusNodeSettings;
// 		bool							MinimalPeersReached;
// 
// 		public NnTcpPeering(string name, string profile, NodeSettings settings, Flow workflow) : base(name, null, null, settings ?? new NodeSettings(profile), workflow)
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
