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
	public PeerRequest				Request { get ; set; }

	public Return					Return;
	public ManualResetEvent			Event;
	public CodeException			Exception;
}

public class ResposePacket : Packet
{
	public Return	Return { get ; set; }
}

public abstract class PeerRequest : ITypeCode
{
	public Peer					Peer;
	public TcpPeering			Peering;
	//public CallArgumentation	Argumentation { get ; set; }
	
	public abstract Return	Execute();
}


public abstract class Ppc<R> : PeerRequest where R : Return /// Peer-to-Peer Call
{
}

public abstract class IPeer
{
 	public abstract	void			Send(PeerRequest rq);
	public abstract Return			Call(PeerRequest rq);
	public Rp						Send<Rp>(Ppc<Rp> rq) where Rp : Return => Call((PeerRequest)rq) as Rp;
}