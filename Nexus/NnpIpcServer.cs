using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Rdn;

//internal class  NnpIppConnection : IppConnection
//{	
//	public NnpIppConnection(IProgram program, NamedPipeServerStream pipe, IppServer server, Flow flow, Constructor constructor) : base(program, pipe, server, flow, constructor)
//	{
//	}
//
//	public override void Established()
//	{
//		
//	}
//}

public class NnpNode
{
	public string			Api { get; set; }
	public string			Net;
	public IppConnection	Connection;
}
	
public class NnpIppServer : IppServer
{
	Nexus.Nexus				Nexus;
	public List<NnpNode>	Locals = [];

	public NnpIppServer(Nexus.Nexus nexus) : base(nexus, NnpIppConnection.GetName(nexus.Settings.Host), nexus.Flow)
	{
		Nexus = nexus;

		Constructor.Register<Argumentation>	(typeof(NnpClass).Assembly,			typeof(NnpClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<Result>		(typeof(NnpClass).Assembly,			typeof(NnpClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(typeof(ExceptionClass).Assembly,	typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Accept(IppConnection connection)
	{
		var ct = connection.Reader.Read<NnpIppConnectionType>();

		if(ct == NnpIppConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			var api = connection.Reader.ReadUtf8();
			Locals.Add(new NnpNode {Connection = connection, Net = net, Api = api});
		}
	
		connection.RegisterHandler(typeof(NnpClass), this);
	}

	Result Relay(IppConnection connection, NnpArgumentation call)
	{
		var n = Locals.Find(i => i.Net == call.Net);

		if(n != null)
		{
			var rp = n.Connection.Call(call, Flow);
			return rp;
		} 
		else
		{
			//Nexus.RdnNode.

			return Nexus.NnpPeering.Call(call.Net, call, Flow);
		} 
	}

	public Result Peers(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result Transact(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result Request(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result HolderClasses(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result HolderAssets(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result HoldersByAccount(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result AssetBalance(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
	public Result AssetTransfer(IppConnection connection, NnpArgumentation call) => Relay(connection, call);
}
