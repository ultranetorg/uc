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

		Constructor.Register<IppRequest> (typeof(NnIpcClass).Assembly, typeof(NnIpcClass), i => i.Remove(i.Length - "NnIpc".Length));
		Constructor.Register<IppResponse>(typeof(NnIpcClass).Assembly, typeof(NnIpcClass), i => i.Remove(i.Length - "NnIpr".Length));
		Constructor.Register<CodeException>(typeof(ExceptionClass).Assembly, typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Accept(IppConnection connection)
	{
		var ct = connection.Reader.Read<NnIppConnectionType>();

		if(ct == NnIppConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			Registrations[net] = connection;
		}
	
		connection.RegisterHandler(typeof(NnIpcClass), this);
	}

	IppResponse Relay(IppConnection connection, NnIppRequest call)
	{
		var id = call.Id;

		if(Registrations.TryGetValue(call.Net, out var r))
		{
			var rp = r.Send(call);
			call.Id  = rp.Id = id; /// IMPORTANT !!!!!!!!!
			return rp;
		} 
		else
			throw new IpcException(IpcError.NotFound);

	}

	public IppResponse HolderClasses(IppConnection connection, NnIppRequest call)
	{
		return Relay(connection, call);
	}

	public IppResponse HolderAssets(IppConnection connection, NnIppRequest call)
	{
		return Relay(connection, call);
	}

	public IppResponse HoldersByAccount(IppConnection connection, NnIppRequest call)
	{
		return Relay(connection, call);
	}

	public IppResponse AssetBalance(IppConnection connection, NnIppRequest call)
	{
		return Relay(connection, call);
	}

	public IppResponse AssetTransfer(IppConnection connection, NnIppRequest call)
	{
		return Relay(connection, call);
	}
}
