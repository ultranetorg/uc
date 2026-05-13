using System.IO.Pipes;
using System.Numerics;
using Uccs.Net;

namespace Uccs.Nexus;

//public class IccpNode
//{
//	public string				Api { get; set; }
//	public string				Net;
//	public IccpLcpConnection	Connection;
//}
	
public class IccpLcpServer : LcpServer
{
	Nexus									Nexus;
	public IEnumerable<IccpLcpConnection>	Locals => Connections.Cast<IccpLcpConnection>();

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
			var c = connection as IccpLcpConnection;
			c.Net = connection.Reader.ReadUtf8();
			c.Api = connection.Reader.ReadUtf8();
		}

		connection.Handler = (from, to, a) => Relay(from, to, a);  /// relay from local nodes
	}

	public override IccpResult Relay(string from, string to, IccpArgumentation call)
	{
		if(call is not TransferRequestIcca tr)
		{
			var c = Locals.FirstOrDefault(i => i.Net == to);

			if(c != null)
			{
				try
				{
					return c.Call(from, to, call, Flow); /// try to relay to local node
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
