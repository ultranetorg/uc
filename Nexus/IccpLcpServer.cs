using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Nexus;

public class IccpNode
{
	public string				Api { get; set; }
	public string				Net;
	public IccpLcpConnection	Connection;
}
	
public class IccpLcpServer : LcpServer
{
	Nexus					Nexus;
	public List<IccpNode>	Locals = [];

	public IccpLcpServer(Nexus nexus) : base(nexus, IccpLcpConnection.GetName(nexus.Settings.Host), nexus.Flow)
	{
		Nexus = nexus;
	}

	protected override LcpConnection CreateConnection(NamedPipeServerStream pipe)
	{
		return new IccpLcpConnection(Program, pipe, this, Flow);
	}

	public override void Accept(LcpConnection connection)
	{
		var ct = connection.Reader.Read<IccpLcpConnectionType>();

		if(ct == IccpLcpConnectionType.Node)
		{	
			var net = connection.Reader.ReadUtf8();
			var api = connection.Reader.ReadUtf8();
			Locals.Add(new IccpNode {Connection = connection as IccpLcpConnection, Net = net, Api = api});
		}

		connection.Handler = (from, to, a) => Relay(from, to, a);  /// relay from local nodes
	}

	public override Result Relay(string from, string to, IccpArgumentation call)
	{
		if(call is not TransferRequestIcca tr)
		{
			var n = Locals.Find(i => i.Net == to);

			if(n != null)
			{
				try
				{
					return n.Connection.Call(from, to, call, Flow); /// try to relay to local node
				}
				catch(CodeException ex)
				{
				}
			}

			return Nexus.IccpPeering.Call(from, to, call, Flow); /// relay to peers
		} 
		else
		{
			Nexus.IccpPeering.Broadcast(from, to, tr);
			return null;
		}
	}
}
