using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Uccs.Net
{
	public enum RdcClass : byte
	{
		None, 
		Proxy, 
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

	public abstract class RdcPacket : ITypeCode
	{
		public int		Id { get; set; }
		public Peer		Peer;

		public abstract byte TypeCode { get; }
	}

	public abstract class RdcInterface
	{
 		public abstract	void					Post(RdcRequest rq);
		public Rp								Send<Rp>(RdcCall<Rp> rq) where Rp : RdcResponse => Send((RdcRequest)rq) as Rp;
		public abstract RdcResponse				Send(RdcRequest rq);
	}

	public abstract class RdcCall<RP> : RdcRequest where RP : RdcResponse
	{
	}

	public abstract class RdsCall<RP> : RdcCall<RP> where RP : RdcResponse
	{
		public Rds Rds => Mcv as Rds;
	}

	public abstract class RdcRequest : RdcPacket
	{
		public override byte			TypeCode => (byte)Class;
		public virtual bool				WaitResponse { get; protected set; } = true;
		public Guid						McvId { get; set; }
		
		public ManualResetEvent			Event;
		public RdcResponse				Response;
		public Sun						Sun;
		public Mcv						Mcv;

		public abstract RdcResponse		Execute();

		public RdcClass Class
		{
			get
			{
				return Enum.Parse<RdcClass>(GetType().Name.Remove(GetType().Name.IndexOf("Request")));
			}
		}

		public RdcRequest()
		{
		}

		public static RdcRequest FromType(RdcClass type)
		{
			return Assembly.GetExecutingAssembly().GetType(typeof(RdcRequest).Namespace + "." + type + "Request").GetConstructor([]).Invoke(null) as RdcRequest;
		}

		public RdcResponse SafeExecute()
		{
			if(WaitResponse)
			{
				RdcResponse rp;

				try
				{
					rp = Execute();
				}
				catch(SunException ex)
				{
					rp = RdcResponse.FromType(Class);
					rp.Error = ex;
				}
				catch(Exception) when(!Debugger.IsAttached)
				{
					rp = RdcResponse.FromType(Class);
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

		protected void RequireBase()
		{
			if(!Mcv.Roles.HasFlag(Role.Base))
				throw new NodeException(NodeError.NotBase);

			if(Mcv.Synchronization != Synchronization.Synchronized)
				throw new NodeException(NodeError.NotSynchronized);
		}

// 		protected Rds RequireRdsBase(Sun sun)
// 		{
// 			RequireBase();
// 
// 			var r = sun.Mcv as Rds;
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

	public abstract class RdcResponse : RdcPacket
	{
		public override byte	TypeCode => (byte)Type;
		public RdcClass			Type => Enum.Parse<RdcClass>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
		public SunException		Error { get; set; }

		public static RdcResponse FromType(RdcClass type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(RdcResponse).Namespace + "." + type + "Response").GetConstructor([]).Invoke(null) as RdcResponse;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(RdcResponse)} type", ex);
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
