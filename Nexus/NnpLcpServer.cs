using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Nexus;

public class NnpNode
{
	public string			Api { get; set; }
	public string			Net;
	public NnpLcpConnection	Connection;
}
	
public class NnpLcpServer : LcpServer
{
	Nexus					Nexus;
	public List<NnpNode>	Locals = [];

	public NnpLcpServer(Nexus nexus) : base(nexus, NnpLcpConnection.GetName(nexus.Settings.Host), nexus.Flow)
	{
		Nexus = nexus;

		Constructor.Register<Argumentation>	(typeof(NnpClass).Assembly,			typeof(NnpClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<Result>		(typeof(NnpClass).Assembly,			typeof(NnpClass),		i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(typeof(ExceptionClass).Assembly,	typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	protected override LcpConnection CreateConnection(NamedPipeServerStream pipe)
	{
		return new NnpLcpConnection(Program, pipe, this, Constructor, Flow);
	}

	public override void Accept(LcpConnection connection)
	{
		var ct = connection.Reader.Read<NnpIppConnectionType>();

		if(ct == NnpIppConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			var api = connection.Reader.ReadUtf8();
			Locals.Add(new NnpNode {Connection = connection as NnpLcpConnection, Net = net, Api = api});
		}

		connection.Handler = (from, to, a) => Relay(from, to, a);  /// relay from local nodes
	}

	public override Result Relay(string from, string to, NnpArgumentation call)
	{
		if(call is not TransferRequestNna tr)
		{
			var n = Locals.Find(i => i.Net == to);

			if(n != null)
			{
				try
				{
					return n.Connection.Call(from, call, Flow); /// try to relay to local node
				}
				catch(CodeException ex)
				{
				}
			}

			return Nexus.NnpPeering.Call(from, to, call, Flow); /// relay to peers
		} 
		else
		{
			Nexus.NnpPeering.Broadcast(from, to, tr);
			return null;
		}
	}
}
