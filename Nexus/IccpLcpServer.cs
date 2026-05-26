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
		var c = connection as IccpLcpConnection;
	
		c.Type = connection.Reader.Read<IccpLcpConnectionType>();

		if(c.Type == IccpLcpConnectionType.Node)
		{	
			c.Net = connection.Reader.ReadUtf8();
			c.Api = connection.Reader.ReadUtf8();
		}

		connection.Handler = (from, to, a, c) => Relay(from, to, a, c);  /// relay from local nodes

		ConnectionEstablished?.Invoke(connection);
	}

	public override IccpResult Relay(string from, string to, IccpArgumentation call, IccpLcpConnection connection)
	{
		if(call is not TransferRequestIcca tr)
		{
			var c = Locals.FirstOrDefault(i => i.Net == to);

			if(c != null && c != connection)
			{
				try
				{
					return c.Call(from, to, call, Flow); /// try to relay to local node
				}
				catch(IccpException ex)
				{
				}
				catch(EntityException)
				{
					throw;
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
