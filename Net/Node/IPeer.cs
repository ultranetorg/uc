using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Uccs.Net
{
	public enum PeerCallClass : byte
	{
		None, 
		Vote,
		PeersBroadcast, Time, Members, Funds, AllocateTransaction, PlaceTransactions, TransactionStatus, Account, 
		Domain, QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease,
		Stamp, TableStamp, DownloadTable, DownloadRounds,
		Cost
	}

	public class TransactionsAddress : IBinarySerializable
	{
		public AccountAddress	Account { get; set; }
		public int				Nid { get; set; }

		public void Read(BinaryReader r)
		{
			Account = r.ReadAccount();
			Nid = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Account); 
			w.Write7BitEncodedInt(Nid);
		}
	}

	public abstract class Packet : ITypeCode
	{
		public int		Id { get; set; }
		public Peer		Peer;

		public abstract byte TypeCode { get; }
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
		public new McvNode	Node => base.Node as McvNode;
		public Mcv			Mcv => Node.Mcv;

		protected void RequireBase()
		{
			if(Mcv.Settings.Base == null)
				throw new NodeException(NodeError.NotBase);

			if(Node is McvNode m && m.Synchronization != Synchronization.Synchronized)
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

			if(!Mcv.NextVoteMembers.Any(i => Mcv.Settings.Generators.Contains(i.Account))) 
				throw new NodeException(NodeError.NotMember);
		}
	}

	public abstract class PeerRequest : Packet
	{
		public override byte			TypeCode => (byte)Class;
		public virtual bool				WaitResponse { get; protected set; } = true;
		
		public ManualResetEvent			Event;
		public PeerResponse				Response;
		public Node						Node;

		public abstract PeerResponse	Execute();

		public PeerCallClass Class
		{
			get
			{
				return Enum.Parse<PeerCallClass>(GetType().Name.Remove(GetType().Name.IndexOf("Request")));
			}
		}

		public PeerRequest()
		{
		}

		public static PeerRequest FromType(PeerCallClass type)
		{
			return Assembly.GetExecutingAssembly().GetType(typeof(PeerRequest).Namespace + "." + type + "Request").GetConstructor([]).Invoke(null) as PeerRequest;
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
					rp = PeerResponse.FromType(Class);
					rp.Error = ex;
				}
				catch(Exception) when(!Debugger.IsAttached)
				{
					rp = PeerResponse.FromType(Class);
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
				catch(Exception ex) when(!Debugger.IsAttached || ex is EntityException || ex is NodeException || ex is RequestException)
				{
				}

				return null;
			}
		}
	}

	public abstract class PeerResponse : Packet
	{
		public override byte	TypeCode => (byte)Type;
		public PeerCallClass			Type => Enum.Parse<PeerCallClass>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
		public NetException		Error { get; set; }

		public static PeerResponse FromType(PeerCallClass type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(PeerResponse).Namespace + "." + type + "Response").GetConstructor([]).Invoke(null) as PeerResponse;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(PeerResponse)} type", ex);
			}
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
