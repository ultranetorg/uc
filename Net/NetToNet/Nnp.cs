using System.Net;
using System.Numerics;

namespace Uccs.Net;

public class NnNodeSettings : SavableSettings
{
	public bool IsHub { get; set; }

	public NnNodeSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public NnNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
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

public class AssetHolder : IBinarySerializable
{
	public string	Class { get; set; }
	public string	Id { get; set; }

	public void Read(BinaryReader reader)
	{
		Class = reader.ReadASCII();
		Id = reader.ReadASCII();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Class);
		writer.WriteASCII(Id);
	}
}

public class Asset : IBinarySerializable
{
	public string	Name { get; set; }
	public string	Units { get; set; }

	public override string ToString()
	{
		return $"{Name} ({Units})";
	}

	public void Read(BinaryReader reader)
	{
		Name = reader.ReadASCII();
		Units = reader.ReadASCII();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Name);
		writer.WriteASCII(Units);
	}
}

public enum NnClass : byte
{
	None = 0, 
	
	NnBlock,
	NnStateHash,

	HolderClasses,
	HoldersByAccount,
	HolderAssets,
	AssetBalance,
	AssetTransfer
}


public abstract class NnArgumentation : IppArgumentation
{
	public string			Net { get; set; }

	public override void	Read(BinaryReader reader) => Net = reader.ReadASCII();
	public override void	Write(BinaryWriter writer) => writer.WriteASCII(Net);
}

public abstract class NnResponse : IppReturn
{
}

public abstract class Nnc<R> : NnArgumentation where R : Return
{
}

public class HolderClassesNnc : Nnc<HolderClassesNnr>
{
}

public class HolderClassesNnr : NnResponse
{
	public string[]			Classes { get; set; }

	public override void	Read(BinaryReader reader) => Classes = reader.ReadArray(reader.ReadASCII);
	public override void	Write(BinaryWriter writer) => writer.Write(Classes, writer.WriteASCII);
}

public class HoldersByAccountNnc : Nnc<HoldersByAccountNnr>
{
	public byte[]	Address { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Address = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteBytes(Address);
	}
}

public class HoldersByAccountNnr : NnResponse
{
	public AssetHolder[] Holders { get; set; }

	public override	void Read(BinaryReader reader) => Holders = reader.ReadArray<AssetHolder>();
	public override void Write(BinaryWriter writer) => writer.Write(Holders);
}

public class HolderAssetsNnc : Nnc<HolderAssetsNnr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		HolderClass = reader.ReadASCII();
		HolderId = reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(HolderClass);
		writer.WriteASCII(HolderId);
	}
}

public class HolderAssetsNnr : NnResponse
{
	public Asset[] Assets {get; set;}

	public override void Read(BinaryReader reader) => Assets = reader.ReadArray<Asset>();
	public override void Write(BinaryWriter writer) => writer.Write(Assets);
}

public class AssetBalanceNnc : Nnc<AssetBalanceNnr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }
	public string	Name { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		HolderClass = reader.ReadASCII();
		HolderId	= reader.ReadASCII();
		Name		= reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(HolderClass);
		writer.WriteASCII(HolderId);
		writer.WriteASCII(Name);
	}
}

public class AssetBalanceNnr : NnResponse
{
	public BigInteger Balance {get; set;}

	public override void Read(BinaryReader reader) => Balance = reader.ReadBigInteger();
	public override void Write(BinaryWriter writer) => writer.Write(Balance);
}

///public class AssetTransferNnc : Nnc<AssetTransferNnr>
///{
///	public string	FromClass { get; set; }
///	public string	FromId { get; set; }
///	public string	ToNet { get; set; }
///	public string	ToClass { get; set; }
///	public string	ToId { get; set; }
///	public string	Name { get; set; }
///	public string	Amount { get; set; }
///}
///
///public class AssetTransferNnIpr : IppResponse
///{
///}


//
//public class StateHashNnc : IBinarySerializable
//{
//	public string			Net { get; set; }
//
//	public StateHashNnc()
//	{
//	}
//	
//	public override PeerResponse Execute()
//	{
//		///return new StateHashNnr {Hash = Peering.GetStateHash(Net)};
//		return null;
//	}
//}
//
//public class StateHashNnr : IBinarySerializable
//{
//	public byte[]	Hash { get; set; }
//}


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
