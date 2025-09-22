using System.Diagnostics;

namespace Uccs.Net;

public abstract class Packet : ITypeCode
{
	public int		Id { get; set; }
	public Peer		Peer;
}

public abstract class IPeer
{
 	public abstract	void			Post(ProcPeerRequest rq);
	public abstract PeerResponse	Send(FuncPeerRequest rq);
	public Rp						Send<Rp>(Ppc<Rp> rq) where Rp : PeerResponse => Send((FuncPeerRequest)rq) as Rp;
}

public abstract class Ppc<R> : FuncPeerRequest where R : PeerResponse /// Peer-to-Peer Call
{

}

public abstract class PeerRequest : Packet
{
	public TcpPeering				Peering;
	public Node						Node;
}

public abstract class ProcPeerRequest : PeerRequest
{
	public abstract void			Execute();

	public void SafeExecute()
	{
		try
		{
			Execute();
		}
		catch(Exception ex) when(!Debugger.IsAttached || ex is CodeException)
		{
		}
	}
}

public abstract class FuncPeerRequest : PeerRequest
{
	public ManualResetEvent			Event;
	public PeerResponse				Response;

	public abstract PeerResponse	Execute();

	public PeerResponse SafeExecute()
	{
		PeerResponse rp;

		try
		{
			rp = Execute();
		}
		catch(CodeException ex)
		{
			rp = Peering.Constract(typeof(PeerResponse), Peering.TypeToCode(GetType())) as PeerResponse;
			rp.Error = ex;
		}
		catch(Exception) when(!Debugger.IsAttached)
		{
			rp = Peering.Constract(typeof(PeerResponse), Peering.TypeToCode(GetType())) as PeerResponse;
			rp.Error = new NodeException(NodeError.Unknown);
		}

		rp.Id = Id;

		return rp;
	}
}

public abstract class PeerResponse : Packet
{
	public PpcClass			Class => Enum.Parse<PpcClass>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
	public CodeException	Error { get; set; }

	static PeerResponse()
	{
	}

}

// 	public class ProxyRequest : RdcCall<ProxyResponse>
// 	{
// 		public byte[]			Guid { get; set; }
// 		public AccountAddress	Destination { get; set; }
// 		public RdcRequest		Request { get; set; }
// 		public override	bool	WaitResponse => Request.WaitResponse;
// 
// 		public override RdcResponse Execute()
// 		{
// 			if(!sun.Roles.HasFlag(Role.Base))
// 				throw new NodeException(NodeError.NotBase);
// 			if(sun.Synchronization != Synchronization.Synchronized)
// 				throw new NodeException(NodeError.NotSynchronized);
// 
// 			lock(sun.Lock)
// 			{
// 				if(sun.Connections.Any(i =>{
// 												lock(i.InRequests) 
// 													return i.InRequests.OfType<ProxyRequest>().Any(j => j.Destination == Destination && j.Guid == Guid);
// 											}))
// 					throw new NodeException(NodeError.CircularRoute);
// 			}
// 
// 			if(sun.Settings.Generators.Contains(Destination))
// 			{
// 				return new ProxyResponse {Response = Request.SafeExecute(sun)};
// 			}
// 			else
// 			{
// 				Member m;
// 
// 				lock(sun.Lock)
// 				{
// 					m = sun.Mcv.LastConfirmedRound.Members.Find(i => i.Account == Destination);
// 				}
// 
// 				if(m?.Proxy != null)
// 				{
// 					sun.Connect(m.Proxy, sun.Flow);
// 			
// 					return new ProxyResponse {Response = m.Proxy.Request<ProxyResponse>(new ProxyRequest {Guid = Guid, Destination = Destination, Request = Request}).Response};
// 				}
// 			}
// 
// 			throw new NodeException(NodeError.NotOnlineYet);
// 		}
// 	}
// 
// 	public class ProxyResponse : RdcResponse
// 	{
// 		public RdcResponse Response { get; set; }
// 	}

// any
// {
// 	#/windows/x64|x86|arm64/10+.1+.0
// 	#/ubuntu/x64|x86|arm64/10+.1+.0
// }
// 
// microsoft/windows/x64|x86|arm64/10+.1+.0
