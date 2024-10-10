using System.Diagnostics;

namespace Uccs.Net
{
	public abstract class Packet : ITypeCode
	{
		public int		Id { get; set; }
		public Peer		Peer;
	}

	public abstract class IPeer
	{
 		public abstract	void			Post(PeerRequest rq);
		public abstract PeerResponse	Send(PeerRequest rq);
		public Rp						Send<Rp>(PeerCall<Rp> rq) where Rp : PeerResponse => Send((PeerRequest)rq) as Rp;
	}

	public abstract class PeerCall<R> : PeerRequest where R : PeerResponse
	{
	}

	public abstract class McvCall<R> : PeerCall<R> where R : PeerResponse
	{
		public new McvTcpPeering	Peering => base.Peering as McvTcpPeering;
		public new McvNode			Node => base.Node as McvNode;
		public Mcv					Mcv => Node.Mcv;

		protected void RequireBase()
		{
			if(Node.Mcv == null)
				throw new NodeException(NodeError.NotBase);

			if(Peering.Synchronization != Synchronization.Synchronized)
				throw new NodeException(NodeError.NotSynchronized);
		}

// 		protected Rdn RequireRdnBase(Sun sun)
// 		{
// 			RequireBase();
// 
// 			var r = sun.Mcv as Rdn;
// 
// 			if(r == null)
// 				throw new NodeException(NodeError.NoMcv);
// 
// 			return r;
// 		}

		protected void RequireMember()
		{
			RequireBase();

			if(!Node.Mcv.NextVoteRound.VotersRound.Members.Any(i => Node.Mcv.Settings.Generators.Contains(i.Address))) 
				throw new NodeException(NodeError.NotMember);
		}

		protected Generator RequireMemberFor(AccountAddress signer)
		{
			RequireBase();

			var m = Node.Mcv.NextVoteRound.VotersRound.Members.NearestBy(m => m.Address, signer);

			if(!Node.Mcv.Settings.Generators.Contains(m.Address)) 
				throw new NodeException(NodeError.NotMember);

			return m;
		}
	}

	public abstract class PeerRequest : Packet
	{
		public virtual bool				WaitResponse { get; protected set; } = true;
		
		public ManualResetEvent			Event;
		public PeerResponse				Response;
		public TcpPeering				Peering;
		public Node						Node;

		public abstract PeerResponse	Execute();

		public PeerCallClass Class
		{
			get
			{
				return Enum.Parse<PeerCallClass>(GetType().Name.Remove(GetType().Name.IndexOf("Request")));
			}
		}

		static PeerRequest()
		{
		}

		public PeerRequest()
		{
		}

		public PeerResponse SafeExecute()
		{
			if(WaitResponse)
			{
				PeerResponse rp;

				try
				{
					rp = Execute();
				}
				catch(NetException ex)
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
			else
			{
				try
				{
					Execute();
				}
				catch(Exception ex) when(!Debugger.IsAttached || ex is NetException)
				{
				}

				return null;
			}
		}
	}

	public abstract class PeerResponse : Packet
	{
		public PeerCallClass	Class => Enum.Parse<PeerCallClass>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
		public NetException		Error { get; set; }

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

}

// any
// {
// 	#/windows/x64|x86|arm64/10+.1+.0
// 	#/ubuntu/x64|x86|arm64/10+.1+.0
// }
// 
// microsoft/windows/x64|x86|arm64/10+.1+.0
