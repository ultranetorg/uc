using System.Numerics;

namespace Uccs.Net;
//
//public class AssetHolder : IBinarySerializable
//{
//	public string	Class { get; set; }
//	public string	Id { get; set; }
//
//	public void Read(BinaryReader reader)
//	{
//		Class = reader.ReadASCII();
//		Id = reader.ReadASCII();
//	}
//
//	public void Write(BinaryWriter writer)
//	{
//		writer.WriteASCII(Class);
//		writer.WriteASCII(Id);
//	}
//}

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

public enum NnpClass : uint
{
	None = 0, 
	
	Peers,
	Message,
	StateHash,
	Request,

	HolderClasses,
	HolderAssets,
	AssetBalance,
	AssetTransfer
}

public abstract class NnpArgumentation : Argumentation
{
	//public string		Net { get; set; }

	//public virtual void	Read(BinaryReader reader) => Net = reader.ReadASCII();
	//public virtual void	Write(BinaryWriter writer) => writer.WriteASCII(Net);
	public virtual void		Read(BinaryReader reader){}
	public virtual void		Write(BinaryWriter writer){}
}

public class Nnc<A, R> : ICall<A, R> where A : NnpArgumentation, new() where R : Result
{
	public A Argumentation;

	public Nnc(A argumentation)
	{
		Argumentation = argumentation;
	}
}

public class TransferRequestNna : NnpArgumentation
{
	public byte[]			Hash { get; set; }
	public byte[]			Signature { get; set; }
	//public Endpoint[]		Peers { get; set; }
	//public IccOperation[]	Operations { get; set; }

	///public override string ToString()
	///{
	///	return $"Nonce={Nonce}, Operations={Operations.Length}, Peers={Peers.Length}";
	///}

	public override void Read(BinaryReader reader)
	{
		Hash		= reader.ReadBytes(Cryptography.HashLength);
		Signature	= reader.ReadBytes(Cryptography.SignatureLength);
		//Peers		= reader.ReadArray<Endpoint>();
 		///Operations	= reader.ReadArray(() => {
 		///										var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 		///										o.Transaction = this;
 		///										o.Read(reader); 
 		///										return o; 
 		///									 });
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Hash);
		writer.Write(Signature);
		//writer.Write(Peers);
		///writer.Write(Operations, i =>	{
		///									writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
		///									i.Write(writer); 
		///								});
	}
}

public class LastOutgoingBlockNna : NnpArgumentation
{
	//public byte[]	Hash { get; set; }
	//public IccOperation[]	Operations { get; set; }

	public override void Read(BinaryReader reader)
	{
		//Hash = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		//writer.WriteBytes(Hash);
	}
}

public class LastOutgoingBlockNnr : Result, IBinarySerializable
{
	public IccTransfer		Block;

	public void Read(BinaryReader reader)
	{
		///Nonce		= reader.Read7BitEncodedInt();
		///Peers		= reader.ReadArray<Endpoint>();
 		///Operations	= reader.ReadArray(() => {
 		///										var o = Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as Operation;
 		///										o.Transaction = this;
 		///										o.Read(reader); 
 		///										return o; 
 		///									 });
	}

	public void Write(BinaryWriter writer)
	{
		///writer.Write7BitEncodedInt(Nonce);
		///writer.Write(Peers);
		///writer.Write(Operations, i =>	{
		///									writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
		///									i.Write(writer); 
		///								});
	}
}

public class LastAcceptedBlockNna : NnpArgumentation
{
}

public class LastAcceptedBlockNnr : Result
{
	public int		Id { get; set; }
	public byte[]	Hash { get; set; }

	public void Read(BinaryReader reader)
	{
		Id = reader.Read7BitEncodedInt();
		Hash = reader.ReadBytes();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.WriteBytes(Hash);
	}
}

public class PeersNna : NnpArgumentation
{
}

public class PeersNnr : Result, IBinarySerializable
{
	public Endpoint[]	Peers { get; set; }

	public void			Read(BinaryReader reader) => Peers = reader.ReadArray<Endpoint>();
	public void			Write(BinaryWriter writer) => writer.Write(Peers);
}


public enum PacketFormat : byte
{
	None, Binary, JsonUtf8
}

public class RequestNna : NnpArgumentation
{
	public byte[]			Request { get; set; }
	public PacketFormat		Format { get; set; }
	public Endpoint			Node { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(BinaryReader reader)
	{
		Request		= reader.ReadBytes();
		Format		= reader.Read<PacketFormat>();
		Node		= reader.ReadNullable<Endpoint>();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteBytes(Request);
		writer.Write(Format);
		writer.WriteNullable(Node);
		writer.Write7BitEncodedInt(Timeout);
	}
}

public class RequestNnr : Result, IBinarySerializable
{
	public byte[]	Response { get; set; }

	public void		Read(BinaryReader reader) => Response = reader.ReadBytes();
	public void		Write(BinaryWriter writer) => writer.WriteBytes(Response);
}

public class JsonApiNna : NnpArgumentation
{
	public string			Call { get; set; }
	public string			Request { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(BinaryReader reader)
	{
		Call		= reader.ReadUtf8();
		Request		= reader.ReadUtf8();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Call);
		writer.WriteUtf8(Request);
		writer.Write7BitEncodedInt(Timeout);
	}
}

public class JsonApiNnr : Result, IBinarySerializable
{
	public string	Response { get; set; }
	public int		Status { get; set; }

	public void Read(BinaryReader reader)
	{ 
		Response = reader.ReadUtf8();
		Status = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Response);
		writer.Write7BitEncodedInt(Status);
	}
}

//public class McvTransactNna : NnpArgumentation
//{
//	public byte[]		Operations { get; set; }
//	public int			Timeout { get; set; } = 5000;
//
//	public override void Read(BinaryReader reader)
//	{
//		base.Read(reader);
//		Operations	= reader.ReadBytes();
//		Timeout		= reader.Read7BitEncodedInt();
//	}
//
//	public override void Write(BinaryWriter writer)
//	{
//		base.Write(writer);
//		writer.WriteBytes(Operations);
//		writer.Write7BitEncodedInt(Timeout);
//	}
//}
//
//public class McvTransactNnr : Result, IBinarySerializable
//{
//	public byte[]	Result { get; set; }
//
//	public void		Read(BinaryReader reader) => Result = reader.ReadBytes();
//	public void		Write(BinaryWriter writer) => writer.WriteBytes(Result);
//}

public class HolderClassesNna : NnpArgumentation
{
}

public class HolderClassesNnr : Result, IBinarySerializable
{
	public string[]			Classes { get; set; }

	public void				Read(BinaryReader reader) => Classes = reader.ReadArray(reader.ReadASCII);
	public void				Write(BinaryWriter writer) => writer.Write(Classes, writer.WriteASCII);
}

//public class HolderClassesNnc : Nnc<HolderClassesNna, HolderClassesNnr>
//{
//	public HolderClassesNnc(string net)
//	{
//		Argumentation.Net = net;
//	}
//}

//public class HoldersByAccountNna : NnpArgumentation
//{
//	public byte[]	Address { get; set; }
//
//	public override void Read(BinaryReader reader)
//	{
//		base.Read(reader);
//		Address = reader.ReadBytes();
//	}
//
//	public override void Write(BinaryWriter writer)
//	{
//		base.Write(writer);
//		writer.WriteBytes(Address);
//	}
//}
//
//public class HoldersByAccountNnr : Result, IBinarySerializable
//{
//	public string[] Holders { get; set; }
//
//	public void Read(BinaryReader reader) => Holders = reader.ReadArray(reader.ReadASCII);
//	public void Write(BinaryWriter writer) => writer.Write(Holders, writer.WriteASCII);
//}

public class HolderAssetsNna : NnpArgumentation
{
	public string	Entity { get; set; }

	public override void Read(BinaryReader reader)
	{
		Entity = reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Entity);
	}
}

public class HolderAssetsNnr : Result, IBinarySerializable
{
	public Asset[]	Assets { get; set; }

	public void Read(BinaryReader reader) => Assets = reader.ReadArray<Asset>();
	public void Write(BinaryWriter writer) => writer.Write(Assets);
}

public class AssetBalanceNna : NnpArgumentation
{
	public string	Entity { get; set; }
	public string	Name { get; set; }

	public override void Read(BinaryReader reader)
	{
		Entity = reader.ReadASCII();
		Name	= reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(Entity);
		writer.WriteASCII(Name);
	}
}

public class AssetBalanceNnr : Result, IBinarySerializable
{
	public BigInteger	Balance {get; set;}

	public void			Read(BinaryReader reader) => Balance = reader.ReadBigInteger();
	public void			Write(BinaryWriter writer) => writer.Write(Balance);
}

public class AssetTransferNna : NnpArgumentation
{
	public string			FromEntity { get; set; }
	public string			ToEntity { get; set; }
	public string			Name { get; set; }
	public string			Amount { get; set; }

	public override void Read(BinaryReader reader)
	{
		FromEntity	= reader.ReadASCII();
		ToEntity	= reader.ReadASCII();
		Name		= reader.ReadASCII();
		Amount		= reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteASCII(FromEntity);
		writer.WriteASCII(ToEntity);
		writer.WriteASCII(Name);
		writer.WriteASCII(Amount);
	}
}

public class AssetTransferNnr : Result, IBinarySerializable
{
	public byte[]	TransactionId { get; set; }

	public  void Read(BinaryReader reader) => TransactionId = reader.ReadBytes();
	public  void Write(BinaryWriter writer) => writer.WriteBytes(TransactionId);
}

