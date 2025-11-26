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

		Constructor.Register<Argumentation>	(typeof(NnClass).Assembly, typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(typeof(NnClass).Assembly, typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(typeof(ExceptionClass).Assembly, typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
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

	Return Relay(IppConnection connection, NnArgumentation call)
	{
		if(Registrations.TryGetValue(call.Net, out var r))
		{
			var rp = r.Call(call);
			return rp;
		} 
		else
		{
			//return Nexus.NnPeering.Call(call.Net, call, Flow);

			throw new IpcException(IpcError.NotFound);
		} 
	}

	public Return HolderClasses(IppConnection connection, NnArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return HolderAssets(IppConnection connection, NnArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return HoldersByAccount(IppConnection connection, NnArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return AssetBalance(IppConnection connection, NnArgumentation call)
	{
		return Relay(connection, call);
	}

	public Return AssetTransfer(IppConnection connection, NnArgumentation call)
	{
		return Relay(connection, call);
	}
}
