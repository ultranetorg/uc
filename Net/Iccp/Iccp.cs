using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public class Iccp
{
	public const string Scheme = "iccp";

	public static Constructor	Constructor;

	static Iccp()
	{
 		Constructor = new ();
		Constructor.Register<IccpArgumentation>	(Assembly.GetExecutingAssembly(), typeof(IccpClass), i => i[..^4]);
		Constructor.Register<IccpResult>		(Assembly.GetExecutingAssembly(), typeof(IccpClass), i => i[..^4]);
		Constructor.Register<IccpTransaction>	(Assembly.GetExecutingAssembly(), typeof(IccpTransactionClass), i => i);
		Constructor.Register<CodeException>		(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}

public enum IccpClass : uint
{
	None = 0, 
	
	Info,
	Peers,
	SubnetPeers,
	TransferRequest,
	LastOutgoingTransfer,
	LastIncomingTransfer,
	Request,

	AddressTextToUniversal,
	HolderClasses,
	HolderAssets,
	AssetBalance,
	Transact
}

public class IccpEntityAddress
{
	public static string ToText(string table, EntityId id) => $"{table}/{id}";
	public static string ToText<T>(T table, EntityId id) => $"{table}/{id}";
	public static byte[] ToBytes(byte table, EntityId id) => [table, ..id.Raw];
}

public abstract class IccpArgumentation : Argumentation, IBinarySerializable
{
	public virtual void Read(Reader reader){}
	public virtual void Write(Writer writer){}
}

public abstract class IccpResult : Result, IBinarySerializable
{
	public abstract void Read(Reader reader);
	public abstract void Write(Writer writer);
}


public class PeersIcca : IccpArgumentation
{
}

public class PeersIccr : IccpResult
{
	public Endpoint[]		Peers { get; set; }

	public override void	Read(Reader reader) => Peers = reader.ReadArray<Endpoint>();
	public override void	Write(Writer writer) => writer.Write(Peers);
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

public class RequestIccr : IccpResult
{
	public byte[]				Response { get; set; }

	public override void		Read(Reader reader) => Response = reader.ReadBytes();
	public override void		Write(Writer writer) => writer.WriteBytes(Response);
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

public class JsonApiIccr : IccpResult
{
	public string	Response { get; set; }
	public int		Status { get; set; }

	public override void Read(Reader reader)
	{ 
		Response = reader.ReadUtf8();
		Status = reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Response);
		writer.Write7BitEncodedInt(Status);
	}
}

public class AddressTextToUniversalIcca : IccpArgumentation
{
	public string			Text { get; set; }

	public override void	Read(Reader reader) => Text = reader.ReadASCII();
	public override void	Write(Writer writer) => writer.WriteASCII(Text);
}

public class AddressTextToUniversalIccr : IccpResult
{
	public byte[]			Universal { get; set; }

	public override void	Read(Reader reader) => Universal = reader.ReadBytes();
	public override void	Write(Writer writer) => writer.WriteBytes(Universal);
}

public class SubnetPeersIcca : IccpArgumentation
{
	public string			Name { get; set; }

	public override void	Read(Reader reader) => Name = reader.ReadASCII();
	public override void	Write(Writer writer) => writer.WriteASCII(Name);
}

public class SubnetPeersIccr : IccpResult
{
	public Endpoint[]		Peers { get; set; }

	public override void	Read(Reader reader) => Peers = reader.ReadArray<Endpoint>();
	public override void	Write(Writer writer) => writer.Write(Peers);
}


