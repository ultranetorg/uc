using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Rdn;

internal class  NnIppConnection : IppConnection
{	
	public NnIppConnection(IProgram program, NamedPipeServerStream pipe, IppServer server, Flow flow, Constructor constructor) : base(program, pipe, server, flow, constructor)
	{
	}

	public override void Established()
	{
		
	}
}
	
public class NnIppServer : IppServer
{
	Nexus.Nexus							Nexus;
	Dictionary<string, IppConnection>	Registrations = [];

	public NnIppServer(Nexus.Nexus nexus) : base(nexus, NnTcpPeering.GetName(nexus.Settings.Host), nexus.Flow)
	{
		Nexus = nexus;

		Constructor.Register<CallArgumentation>	(typeof(NnClass).Assembly, typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CallReturn>		(typeof(NnClass).Assembly, typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>		(typeof(ExceptionClass).Assembly, typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Accept(IppConnection connection)
	{
		var ct = connection.Reader.Read<NnIppConnectionType>();

		if(ct == NnIppConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			Registrations[net] = connection;
		}
	
		connection.RegisterHandler(typeof(NnClass), this);
	}

	CallReturn Relay(IppConnection connection, NnRequest call)
	{
		//var a = call as NnRequest;
		//var id = call.Id;

		if(Registrations.TryGetValue(call.Net, out var r))
		{
			var rp = r.Send(call);
			//call.Id = rp.Id = id; /// IMPORTANT !!!!!!!!!
			return rp;
		} 
		else
			throw new IpcException(IpcError.NotFound);

	}

	public CallReturn HolderClasses(IppConnection connection, NnRequest call)
	{
		return Relay(connection, call);
	}

	public CallReturn HolderAssets(IppConnection connection, NnRequest call)
	{
		return Relay(connection, call);
	}

	public CallReturn HoldersByAccount(IppConnection connection, NnRequest call)
	{
		return Relay(connection, call);
	}

	public CallReturn AssetBalance(IppConnection connection, NnRequest call)
	{
		return Relay(connection, call);
	}

	public CallReturn AssetTransfer(IppConnection connection, NnRequest call)
	{
		return Relay(connection, call);
	}
}
