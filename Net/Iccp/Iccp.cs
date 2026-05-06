using System.Numerics;
using System.Reflection;

namespace Uccs.Net;
//
//public class AssetHolder : IBinarySerializable
//{
//	public string	Class { get; set; }
//	public string	Id { get; set; }
//
//	public void Read(Reader reader)
//	{
//		Class = reader.ReadASCII();
//		Id = reader.ReadASCII();
//	}
//
//	public void Write(Writer writer)
//	{
//		writer.WriteASCII(Class);
//		writer.WriteASCII(Id);
//	}
//}

public class Iccp
{
	public static Constructor	Constructor;

	static Iccp()
	{
 		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(IccpClass), i => i[..^4]);
		Constructor.Register<Result>		(Assembly.GetExecutingAssembly(), typeof(IccpClass), i => i[..^4]);
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}


public class Asset : IBinarySerializable
{
	public string	Name { get; set; }
	public byte[]	Id { get; set; }
	public string	Units { get; set; }

	public static Asset Energy(byte year)
	{
		var a = new Asset();
		a.Name = $"Energy for {Time.FirstYear + year} year";
		a.Id = [0, year];
		a.Units = "Execution Cycles (EC)";

		return a;
	}

	public static Asset Spacetime()
	{
		var a = new Asset();
		a.Name = $"Space-time";
		a.Id = [1];
		a.Units = "Byte-days (BD)";

		return a;
	}

	public override string ToString()
	{
		return $"{Name} ({Units})";
	}

	public void Read(Reader reader)
	{
		Name	= reader.ReadASCII();
		Id		= reader.ReadBytes();
		Units	= reader.ReadASCII();
	}

	public void Write(Writer writer)
	{
		writer.WriteASCII(Name);
		writer.WriteBytes(Id);
		writer.WriteASCII(Units);
	}
}

public enum IccpClass : uint
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

public abstract class IccpArgumentation : Argumentation, IBinarySerializable
{
	//public string		Net { get; set; }

	//public virtual void	Read(Reader reader) => Net = reader.ReadASCII();
	//public virtual void	Write(Writer writer) => writer.WriteASCII(Net);
	public virtual void		Read(Reader reader){}
	public virtual void		Write(Writer writer){}
}

public class TransferRequestIcca : IccpArgumentation
{
	public byte[]			Hash { get; set; }
	public byte[]			Signature { get; set; }
	//public Endpoint[]		Peers { get; set; }
	//public IccOperation[]	Operations { get; set; }

	///public override string ToString()
	///{
	///	return $"Nonce={Nonce}, Operations={Operations.Length}, Peers={Peers.Length}";
	///}

	public override void Read(Reader reader)
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

	public override void Write(Writer writer)
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

public class LastOutgoingTransferIcca : IccpArgumentation
{
	//public byte[]	Hash { get; set; }
	//public IccOperation[]	Operations { get; set; }

	public override void Read(Reader reader)
	{
		//Hash = reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		//writer.WriteBytes(Hash);
	}
}

public class LastOutgoingTransferIccr : Result, IBinarySerializable
{
	public IccpTransfer		Transfer;

	public void Read(Reader reader)
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

	public void Write(Writer writer)
	{
		///writer.Write7BitEncodedInt(Nonce);
		///writer.Write(Peers);
		///writer.Write(Operations, i =>	{
		///									writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
		///									i.Write(writer); 
		///								});
	}
}

public class LastIncomingTransferIcca : IccpArgumentation
{
}

public class LastIncomingTransferIccr : Result
{
	public int					Id { get; set; }
	public IccpTransferResult	Result { get; set; }

	public void Read(Reader reader)
	{
		Id		= reader.Read7BitEncodedInt();
		Result	= reader.Read<IccpTransferResult>();
	}

	public void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Result);
	}
}

public class PeersIcca : IccpArgumentation
{
}

public class PeersIccr : Result, IBinarySerializable
{
	public Endpoint[]	Peers { get; set; }

	public void			Read(Reader reader) => Peers = reader.ReadArray<Endpoint>();
	public void			Write(Writer writer) => writer.Write(Peers);
}


public enum PacketFormat : byte
{
	None, Binary, JsonUtf8
}

public class RequestIcca : IccpArgumentation
{
	public byte[]			Request { get; set; }
	public PacketFormat		Format { get; set; }
	public Endpoint			Node { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(Reader reader)
	{
		Request		= reader.ReadBytes();
		Format		= reader.Read<PacketFormat>();
		Node		= reader.ReadNullable<Endpoint>();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Request);
		writer.Write(Format);
		writer.WriteNullable(Node);
		writer.Write7BitEncodedInt(Timeout);
	}
}

public class RequestIccr : Result, IBinarySerializable
{
	public byte[]	Response { get; set; }

	public void		Read(Reader reader) => Response = reader.ReadBytes();
	public void		Write(Writer writer) => writer.WriteBytes(Response);
}

public class JsonApiIcca : IccpArgumentation
{
	public string			Call { get; set; }
	public string			Request { get; set; }
	public int				Timeout { get; set; } = 5000;

	public override void Read(Reader reader)
	{
		Call		= reader.ReadUtf8();
		Request		= reader.ReadUtf8();
		Timeout		= reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Call);
		writer.WriteUtf8(Request);
		writer.Write7BitEncodedInt(Timeout);
	}
}

public class JsonApiIccr : Result, IBinarySerializable
{
	public string	Response { get; set; }
	public int		Status { get; set; }

	public void Read(Reader reader)
	{ 
		Response = reader.ReadUtf8();
		Status = reader.Read7BitEncodedInt();
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Response);
		writer.Write7BitEncodedInt(Status);
	}
}

//public class McvTransactIcca : NnpArgumentation
//{
//	public byte[]		Operations { get; set; }
//	public int			Timeout { get; set; } = 5000;
//
//	public override void Read(Reader reader)
//	{
//		base.Read(reader);
//		Operations	= reader.ReadBytes();
//		Timeout		= reader.Read7BitEncodedInt();
//	}
//
//	public override void Write(Writer writer)
//	{
//		base.Write(writer);
//		writer.WriteBytes(Operations);
//		writer.Write7BitEncodedInt(Timeout);
//	}
//}
//
//public class McvTransactIccr : Result, IBinarySerializable
//{
//	public byte[]	Result { get; set; }
//
//	public void		Read(Reader reader) => Result = reader.ReadBytes();
//	public void		Write(Writer writer) => writer.WriteBytes(Result);
//}

public class AddressTextToUniversalIcca : IccpArgumentation
{
	public string			Text { get; set; }

	public override void	Read(Reader reader) => Text = reader.ReadASCII();
	public override void	Write(Writer writer) => writer.WriteASCII(Text);
}

public class AddressTextToUniversalIccr : Result, IBinarySerializable
{
	public byte[]	Universal { get; set; }

	public void		Read(Reader reader) => Universal = reader.ReadBytes();
	public void		Write(Writer writer) => writer.WriteBytes(Universal);
}


public class HolderClassesIcca : IccpArgumentation
{
}

public class HolderClassesIccr : Result, IBinarySerializable
{
	public string[]			Classes { get; set; }

	public void				Read(Reader reader) => Classes = reader.ReadArray(reader.ReadASCII);
	public void				Write(Writer writer) => writer.Write(Classes, writer.WriteASCII);
}

//public class HolderClassesNnc : Nnc<HolderClassesIcca, HolderClassesIccr>
//{
//	public HolderClassesNnc(string net)
//	{
//		Argumentation.Net = net;
//	}
//}

//public class HoldersByAccountIcca : NnpArgumentation
//{
//	public byte[]	Address { get; set; }
//
//	public override void Read(Reader reader)
//	{
//		base.Read(reader);
//		Address = reader.ReadBytes();
//	}
//
//	public override void Write(Writer writer)
//	{
//		base.Write(writer);
//		writer.WriteBytes(Address);
//	}
//}
//
//public class HoldersByAccountIccr : Result, IBinarySerializable
//{
//	public string[] Holders { get; set; }
//
//	public void Read(Reader reader) => Holders = reader.ReadArray(reader.ReadASCII);
//	public void Write(Writer writer) => writer.Write(Holders, writer.WriteASCII);
//}

public class HolderAssetsIcca : IccpArgumentation
{
	public byte[]	Entity { get; set; }

	public override void Read(Reader reader)
	{
		Entity = reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Entity);
	}
}

public class HolderAssetsIccr : Result, IBinarySerializable
{
	public Asset[]	Assets { get; set; }

	public void Read(Reader reader) => Assets = reader.ReadArray<Asset>();
	public void Write(Writer writer) => writer.Write(Assets);
}

public class AssetBalanceIcca : IccpArgumentation
{
	public byte[]	Entity { get; set; }
	public byte[]	Asset { get; set; }

	public override void Read(Reader reader)
	{
		Entity	= reader.ReadBytes();
		Asset	= reader.ReadBytes();
	}

	public override void Write(Writer writer)
	{
		writer.WriteBytes(Entity);
		writer.WriteBytes(Asset);
	}
}

public class AssetBalanceIccr : Result, IBinarySerializable
{
	public BigInteger	Balance {get; set;}

	public void			Read(Reader reader) => Balance = reader.ReadBigInteger();
	public void			Write(Writer writer) => writer.Write(Balance);
}

public class AssetTransferIcca : IccpArgumentation
{
	public string			FromEntity { get; set; }
	public string			ToEntity { get; set; }
	public string			Name { get; set; }
	public string			Amount { get; set; }

	public override void Read(Reader reader)
	{
		FromEntity	= reader.ReadASCII();
		ToEntity	= reader.ReadASCII();
		Name		= reader.ReadASCII();
		Amount		= reader.ReadASCII();
	}

	public override void Write(Writer writer)
	{
		writer.WriteASCII(FromEntity);
		writer.WriteASCII(ToEntity);
		writer.WriteASCII(Name);
		writer.WriteASCII(Amount);
	}
}

public class AssetTransferIccr : Result, IBinarySerializable
{
	public byte[]	TransactionId { get; set; }

	public  void Read(Reader reader) => TransactionId = reader.ReadBytes();
	public  void Write(Writer writer) => writer.WriteBytes(TransactionId);
}

