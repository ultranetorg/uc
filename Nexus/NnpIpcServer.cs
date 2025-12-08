using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Rdn;

internal class  NnpIppConnection : IppConnection
{	
	public NnpIppConnection(IProgram program, NamedPipeServerStream pipe, IppServer server, Flow flow, Constructor constructor) : base(program, pipe, server, flow, constructor)
	{
	}

	public override void Established()
	{
		
	}
}

public class NnpNode
{
	public string			Api { get; set; }
	public string			Net;
	public IppConnection	Connection;
}
	
public class NnpIppServer : IppServer
{
	Nexus.Nexus				Nexus;
	public List<NnpNode>	Nodes = [];

	public NnpIppServer(Nexus.Nexus nexus) : base(nexus, NnpTcpPeering.GetName(nexus.Settings.Host), nexus.Flow)
	{
		Nexus = nexus;

		Constructor.Register<Argumentation>	(typeof(NnClass).Assembly,			typeof(NnClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(typeof(NnClass).Assembly,			typeof(NnClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(typeof(ExceptionClass).Assembly,	typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Accept(IppConnection connection)
	{
		var ct = connection.Reader.Read<NnpIppConnectionType>();

		if(ct == NnpIppConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			var api = connection.Reader.ReadUtf8();
			Nodes.Add(new NnpNode {Connection = connection, Net = net, Api = api});
		}
	
		connection.RegisterHandler(typeof(NnClass), this);
	}

	Return Relay(IppConnection connection, NnpArgumentation call)
	{
		var n = Nodes.Find(i => i.Net == call.Net);

		if(n != null)
		{
			var rp = n.Connection.Call(call, Flow);
			return rp;
		} 
		else
		{
			return Nexus.NnPeering.Call(call.Net, call, Flow);
		} 
	}

	public Return HolderClasses(IppConnection connection, NnpArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return HolderAssets(IppConnection connection, NnpArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return HoldersByAccount(IppConnection connection, NnpArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return AssetBalance(IppConnection connection, NnpArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return AssetTransfer(IppConnection connection, NnpArgumentation call)
	{
		return Relay(connection, call);
	}
}
