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
	public string		Net { get; set; }

	public virtual void	Read(BinaryReader reader) => Net = reader.ReadASCII();
	public virtual void	Write(BinaryWriter writer) => writer.WriteASCII(Net);
}

public class Nnc<A, R> : ICall<A, R> where A : NnpArgumentation, new() where R : Result
{
	public A Argumentation;

	public Nnc(A argumentation)
	{
		Argumentation = argumentation;
	}
}

public class TransactionNna : NnpArgumentation
{
	public int				Nonce { get; set; }
	public Endpoint[]		Peers { get; set; }
	public IccpOperation[]	Operations { get; set; }

	public byte[]			Hash => _Hash ??= Cryptography.Hash(RawPayload);
	byte[]					_Hash;
	byte[]					_RawPayload;

	public byte[] RawPayload
	{
		get
		{ 
			if(_RawPayload == null)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);

				WritePayload(w);

				_RawPayload = s.ToArray();
			}
		
			return _RawPayload; 
		}

		set { _RawPayload = value; }
	}

	public override string ToString()
	{
		return $"Nonce={Nonce}, Operations={Operations.Length}, Peers={Peers.Length}";
	}

	public void WritePayload(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Nonce);
		writer.Write(Peers);
		writer.Write(Operations);
	}

	public void ReadPayload(BinaryReader reader)
	{
		Nonce		= reader.Read7BitEncodedInt();
		Peers		= reader.ReadArray<Endpoint>();
		Operations	= reader.ReadArray<IccpOperation>();
	}

	public void Restore()
	{
		ReadPayload(new BinaryReader(new MemoryStream(RawPayload)));
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		ReadPayload(reader);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		WritePayload(writer);
	}
}

public class TransactionConfirmationNna : NnpArgumentation
{
	public int		Nonce { get; set; }
	public byte[]	Hash{ get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Nonce = reader.Read7BitEncodedInt();
		Hash = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write7BitEncodedInt(Nonce);
		writer.WriteBytes(Hash);
	}
}

public class PeersNna : NnpArgumentation, IBinarySerializable
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

public class RequestNna : NnpArgumentation, IBinarySerializable
{
	public byte[]			Request { get; set; }
	public PacketFormat		Format { get; set; }
	public Endpoint			Node { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Request		= reader.ReadBytes();
		Format		= reader.Read<PacketFormat>();
		Node		= reader.ReadNullable<Endpoint>();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
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

public class JsonApiNna : NnpArgumentation, IBinarySerializable
{
	public string			Call { get; set; }
	public string			Request { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Call		= reader.ReadUtf8();
		Request		= reader.ReadUtf8();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
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

//public class McvTransactNna : NnpArgumentation, IBinarySerializable
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

public class HolderClassesNna : NnpArgumentation, IBinarySerializable
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

//public class HoldersByAccountNna : NnpArgumentation, IBinarySerializable
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

public class HolderAssetsNna : NnpArgumentation, IBinarySerializable
{
	public string	Entity { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Entity = reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(Entity);
	}
}

public class HolderAssetsNnr : Result, IBinarySerializable
{
	public Asset[] Assets {get; set;}

	public void Read(BinaryReader reader) => Assets = reader.ReadArray<Asset>();
	public void Write(BinaryWriter writer) => writer.Write(Assets);
}

public class AssetBalanceNna : NnpArgumentation, IBinarySerializable
{
	public string	Entity { get; set; }
	public string	Name { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Entity = reader.ReadASCII();
		Name	= reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(Entity);
		writer.WriteASCII(Name);
	}
}

public class AssetBalanceNnr : Result, IBinarySerializable
{
	public BigInteger Balance {get; set;}

	public  void Read(BinaryReader reader) => Balance = reader.ReadBigInteger();
	public  void Write(BinaryWriter writer) => writer.Write(Balance);
}

public class AssetTransferNna : NnpArgumentation, IBinarySerializable
{
	public string			FromEntity { get; set; }
	public string			ToNet { get; set; }
	public string			ToEntity { get; set; }
	public string			Name { get; set; }
	public string			Amount { get; set; }

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		FromEntity	= reader.ReadASCII();
		ToNet		= reader.ReadASCII();
		ToEntity	= reader.ReadASCII();
		Name		= reader.ReadASCII();
		Amount		= reader.ReadASCII();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteASCII(FromEntity);
		writer.WriteASCII(ToNet);
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

