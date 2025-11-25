using System.Diagnostics;

namespace Uccs.Net;

public abstract class Packet : ITypeCode
{
	public int		Id { get; set; }
}

public abstract class IPeer
{
 	public abstract	void			Post(ProcPeerRequest rq);
	public abstract PeerResponse	Send(FuncPeerRequest rq);
	public Rp						Send<Rp>(Ppc<Rp> rq) where Rp : PeerResponse => Send((FuncPeerRequest)rq) as Rp;
}

public abstract class PeerRequest : Packet
{
	public Peer				Peer;
	public TcpPeering		Peering;
	//public Node				Node;
}

public abstract class ProcPeerRequest : PeerRequest
{
	public abstract void			Execute();
}

public abstract class FuncPeerRequest : PeerRequest
{
	public ManualResetEvent			Event;
	public PeerResponse				Response;
	public CodeException			Exception;

	public abstract PeerResponse	Execute();

    public FuncPeerRequest ShallowCopy()
    {
        return (FuncPeerRequest)MemberwiseClone();
    }
}

public abstract class PeerResponse : Packet
{
}

public abstract class Ppc<R> : FuncPeerRequest where R : PeerResponse /// Peer-to-Peer Call
{

}
