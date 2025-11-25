using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public enum NnIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

//public abstract class NnIppRequest// : IppFuncRequest
//{
//	public string	Net { get; set; }
//}

//public abstract class NnIpc<A, R> : NnIppRequest where A : IBinarySerializable where R : IBinarySerializable
//{
//}

//public class HolderClassesNnIpc : NnIpc<HolderClassesNnIpr>
//{
//}
//
//public class HolderClassesNnIpr : IppResponse
//{
//	public string[]  Classes { get; set; }
//}

//public class HoldersByAccountNnIpc : NnIpc<HoldersByAccountNnIpr>
//{
//	public byte[]	Address { get; set; }
//}
//
//public class HoldersByAccountNnIpr : IppResponse
//{
//	public AssetHolder[] Holders { get; set; }
//}
//
//public class HolderAssetsNnIpc : NnIpc<HolderAssetsNnIpr>
//{
//	public string	HolderClass { get; set; }
//	public string	HolderId { get; set; }
//}
//
//public class HolderAssetsNnIpr : IppResponse
//{
//	public Asset[] Assets {get; set;}
//}
//
//public class AssetBalanceNnIpc : NnIpc<AssetBalanceNnIpr>
//{
//	public string	HolderClass { get; set; }
//	public string	HolderId { get; set; }
//	public string	Name { get; set; }
//}
//
//public class AssetBalanceNnIpr : IppResponse
//{
//	public BigInteger Balance {get; set;}
//}
//
//public class AssetTransferNnIpc : NnIpc<AssetTransferNnIpr>
//{
//	public string	FromClass { get; set; }
//	public string	FromId { get; set; }
//	public string	ToNet { get; set; }
//	public string	ToClass { get; set; }
//	public string	ToId { get; set; }
//	public string	Name { get; set; }
//	public string	Amount { get; set; }
//}
//
//public class AssetTransferNnIpr : IppResponse
//{
//}

public class NnIppClientConnection : IppConnection
{
	public Rp Call<Rp>(Nnc<Rp> call) where Rp : class, IBinarySerializable => Send(new IppFuncRequest {Argumentation = call}).Return as Rp;

	public NnIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<CallArgumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CallReturn>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>		(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
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
		Constructor.Register<CallArgumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CallReturn>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>		(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
