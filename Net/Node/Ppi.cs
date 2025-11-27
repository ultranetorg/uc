using System.Diagnostics;

namespace Uccs.Net;

public abstract class Packet
{
	public int		Id { get; set; }
}

//public abstract class PpcArgumentation : CallArgumentation
//{
//	public TcpPeering	Peering;
//
//}

public class RequestPacket: Packet
{
	public Return					Return;
	public ManualResetEvent			Event;
	public CodeException			Exception;
}

public class HomoRequestPacket: RequestPacket
{
	public PeerRequest				Request { get ; set; }
}

//public class ResposePacket : Packet
//{
//	public Return	Return { get ; set; }
//}

public abstract class PeerRequest : ITypeCode
{
	public HomoPeer				Peer;
	public HomoTcpPeering		Peering;
	//public CallArgumentation	Argumentation { get ; set; }
	
	public abstract Return	Execute();
}


public abstract class Ppc<R> : PeerRequest where R : Return /// Peer-to-Peer Call
{
}

