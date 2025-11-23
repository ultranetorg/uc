using System.IO.Pipes;
using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public class AssetHolder
{
	public string	Class { get; set; }
	public string	Id { get; set; }
}

public class Asset
{
	public string	Name { get; set; }
	public string	Units { get; set; }

	public override string ToString()
	{
		return $"{Name} ({Units})";
	}
}

public enum NnIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public enum NnIpcClass : byte
{
	None = 0, 
	HolderClasses,
	HoldersByAccount,
	HolderAssets,
	AssetBalance,
	AssetTransfer
}

public abstract class NnIppRequest : IppFuncRequest /// Pipe-to-Pipe Call
{
	public string	Net { get; set; }
}

public abstract class NnIpc<R> : NnIppRequest where R : IppResponse /// Pipe-to-Pipe Call
{
}

public interface INn
{
	HolderClassesNnIpr		HolderClasses(HolderClassesNnIpc call);
	HoldersByAccountNnIpr	HoldersByAccount(HoldersByAccountNnIpc call);
	HolderAssetsNnIpr		HolderAssets(HolderAssetsNnIpc call);
	AssetBalanceNnIpr		AssetBalance(AssetBalanceNnIpc call);
	AssetTransferNnIpr		AssetTransfer(AssetTransferNnIpc call);
}

public class HolderClassesNnIpc : NnIpc<HolderClassesNnIpr>
{
}

public class HolderClassesNnIpr : IppResponse
{
	public string[]  Classes { get; set; }
}

public class HoldersByAccountNnIpc : NnIpc<HoldersByAccountNnIpr>
{
	public byte[]	Address { get; set; }
}

public class HoldersByAccountNnIpr : IppResponse
{
	public AssetHolder[] Holders {get; set;}
}

public class HolderAssetsNnIpc : NnIpc<HolderAssetsNnIpr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }
}

public class HolderAssetsNnIpr : IppResponse
{
	public Asset[] Assets {get; set;}
}

public class AssetBalanceNnIpc : NnIpc<AssetBalanceNnIpr>
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }
	public string	Name { get; set; }
}

public class AssetBalanceNnIpr : IppResponse
{
	public BigInteger Balance {get; set;}
}

public class AssetTransferNnIpc : NnIpc<AssetTransferNnIpr>
{
	public string	FromClass { get; set; }
	public string	FromId { get; set; }
	public string	ToNet { get; set; }
	public string	ToClass { get; set; }
	public string	ToId { get; set; }
	public string	Name { get; set; }
	public string	Amount { get; set; }
}

public class AssetTransferNnIpr : IppResponse
{
}

public class NnIppClientConnection : IppConnection
{
	public Rp Send<Rp>(NnIpc<Rp> rq) where Rp : IppResponse => Send((IppFuncRequest)rq) as Rp;

	public NnIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<IppRequest>	(Assembly.GetExecutingAssembly(), typeof(NnIpcClass), i => i.Remove(i.Length - 5));
		Constructor.Register<IppResponse>	(Assembly.GetExecutingAssembly(), typeof(NnIpcClass), i => i.Remove(i.Length - 5));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Established()
	{
		Writer.Write(NnIppConnectionType.Client);
	}
}

public class NnIppNodeConnection : IppConnection
{

	//public NnIppNodeConnection(IProgram program, NamedPipeServerStream pipe, IppServer server, Flow flow, Constructor constructor) : base(program, pipe, server, flow, constructor)
	//{
	//}

	public NnIppNodeConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<IppRequest>	(Assembly.GetExecutingAssembly(), typeof(NnIpcClass), i => i.Remove(i.Length - 5));
		Constructor.Register<IppResponse>	(Assembly.GetExecutingAssembly(), typeof(NnIpcClass), i => i.Remove(i.Length - 5));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
