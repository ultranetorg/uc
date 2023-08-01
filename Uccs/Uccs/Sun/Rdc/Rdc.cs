using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml.Linq;
using NativeImport;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Uccs.Net
{
	public enum Rdc : byte
	{
		Null, 
		Proxy, 
		MemberJoin, MemberVox, AnalyzerVox,
		PeersBroadcast, Time, Members, NextRound, LastOperation, SendTransactions, GetOperationStatus, Account, 
		Author, QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease,
		Stamp, TableStamp, DownloadTable, DownloadRounds
	}

	public enum RdcResult : byte
	{
		Null, Success, NodeException, EntityException
	}

	public enum RdcNodeError : byte
	{
		Null,
		Integrity,
		Internal,
		Timeout,
		NotFound,
		NotChain,
		NotBase,
		NotMember,
		NotSeeder,
		NotSynchronized,
		TooEearly,
		AllNodesFailed,
		NotOnlineYet,
		CircularRoute,
	}

	public enum RdcEntityError : byte
	{
		Null,
		InvalidRequest,
		AccountNotFound,
		ResourceNotFound,
		ClusterNotFound,
		RoundNotAvailable,
	}

 	public class RdcNodeException : Exception
 	{
		public RdcNodeError Error;

 		public RdcNodeException(RdcNodeError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
 	}

 	public class RdcEntityException : Exception
 	{
		public RdcEntityError Error;

 		public RdcEntityException(RdcEntityError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
 	}

	public class OperationAddress : IBinarySerializable
	{
		public AccountAddress	Account { get; set; }
		public int				Id { get; set; }

		public void Read(BinaryReader r)
		{
			Account = r.ReadAccount();
			Id = r.Read7BitEncodedInt();
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Account); 
			w.Write7BitEncodedInt(Id);
		}
	}

	public abstract class RdcInterface
	{
		public int								Failures;

 		public abstract Rp						Request<Rp>(RdcRequest rq) where Rp : RdcResponse;
 		public abstract	void					Send(RdcRequest rq);
 
		public TimeResponse						GetTime() => Request<TimeResponse>(new TimeRequest());
		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
		public DownloadTableResponse			DownloadTable(Tables table, ushort cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
		public NextRoundResponse				GetNextRound() => Request<NextRoundResponse>(new NextRoundRequest());
		public SendTransactionsResponse			SendTransactions(IEnumerable<Transaction> transactions) => Request<SendTransactionsResponse>(new SendTransactionsRequest{Transactions = transactions});
		public GetOperationStatusResponse		GetOperationStatus(IEnumerable<OperationAddress> operations) => Request<GetOperationStatusResponse>(new GetOperationStatusRequest{Operations = operations});
		public MembersResponse					GetMembers() => Request<MembersResponse>(new MembersRequest());
		public AuthorResponse					GetAuthorInfo(string author) => Request<AuthorResponse>(new AuthorRequest{Name = author});
		public AccountResponse					GetAccountInfo(AccountAddress account) => Request<AccountResponse>(new AccountRequest{Account = account});
		public ResourceResponse					FindResource(ResourceAddress resource) => Request<ResourceResponse>(new ResourceRequest {Resource = resource});
		public QueryResourceResponse			QueryResource(string query) => Request<QueryResourceResponse>(new QueryResourceRequest {Query = query });
		public LocateReleaseResponse			LocateRelease(byte[] hash, int count) => Request<LocateReleaseResponse>(new LocateReleaseRequest{Hash = hash, Count = count});
		public void								DeclareRelease(IEnumerable<DeclareReleaseItem> releases) => Send(new DeclareReleaseRequest{Releases = releases});
		//public ManifestResponse					GetManifest(ReleaseAddress release) => Request<ManifestResponse>(new ManifestRequest{Release = release});
		public DownloadReleaseResponse			DownloadRelease(ResourceAddress resource, byte[] hash, string file, long offset, long length) => Request<DownloadReleaseResponse>(new DownloadReleaseRequest{Resource = resource, Hash = hash, File = file, Offset = offset, Length = length});
		//public ReleaseHistoryResponse			GetReleaseHistory(RealizationAddress realization) => Request<ReleaseHistoryResponse>(new ReleaseHistoryRequest{Realization = realization});
	}

	public abstract class RdcPacket : ITypedBinarySerializable
	{
		public int			Id {get; set;}
		public Peer			Peer;

		public abstract byte TypeCode { get; }
	}

	public abstract class RdcRequest : RdcPacket
	{
		public override byte			TypeCode => (byte)Type;
		public ManualResetEvent			Event;
		public RdcResponse				Response;
		public Action					Process;
		public virtual bool				WaitResponse { get; protected set; } = true;

		public static RdcRequest FromType(Rdc type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(RdcRequest).Namespace + "." + type + "Request").GetConstructor(new System.Type[]{}).Invoke(new object[]{ }) as RdcRequest;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(RdcRequest)} type", ex);
			}
		}

		public Rdc Type
		{
			get
			{
				return Enum.Parse<Rdc>(GetType().Name.Remove(GetType().Name.IndexOf("Request")));
			}
		}

		public RdcRequest()
		{
			Event = new ManualResetEvent(false);
		}

		public abstract RdcResponse Execute(Core core);
	}

	public abstract class RdcResponse : RdcPacket
	{
		public override byte	TypeCode => (byte)Type;
		public Rdc				Type => Enum.Parse<Rdc>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
		public RdcResult		Result { get; set; }
		public byte				Error { get; set; }

		public static RdcResponse FromType(Rdc type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(RdcResponse).Namespace + "." + type + "Response").GetConstructor(new System.Type[]{}).Invoke(new object[]{}) as RdcResponse;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(RdcResponse)} type", ex);
			}
		}
	}

	public class ProxyRequest : RdcRequest
	{
		public byte[]			Guid { get; set; }
		public AccountAddress	Destination { get; set; }
		public RdcRequest		Request { get; set; }
		public override	bool	WaitResponse => Request.WaitResponse;

		public override RdcResponse Execute(Core core)
		{
			if(!core.Settings.Roles.HasFlag(Role.Base))					throw new RdcNodeException(RdcNodeError.NotBase);
			if(core.Synchronization != Synchronization.Synchronized)	throw new RdcNodeException(RdcNodeError.NotSynchronized);

			lock(core.Lock)
			{
				if(core.Connections.Any(i =>{
												lock(i.InRequests) 
													return i.InRequests.OfType<ProxyRequest>().Any(j => j.Destination == Destination && j.Guid == Guid);
											}))
					throw new RdcNodeException(RdcNodeError.CircularRoute);
			}

			if(core.Settings.Generators.Contains(Destination))
			{
				return new ProxyResponse {Response = Request.Execute(core)};
			}
			else
			{
				Member m;

				lock(core.Lock)
				{
					m = core.Chainbase.LastConfirmedRound.Members.Find(i => i.Account == Destination);
				}

				if(m?.Proxy != null)
				{
					core.Connect(m.Proxy, core.Workflow);
			
					return new ProxyResponse {Response = m.Proxy.Request<ProxyResponse>(new ProxyRequest {Guid = Guid, Destination = Destination, Request = Request}).Response};
				}
			}

			throw new RdcNodeException(RdcNodeError.NotOnlineYet);
		}
	}

	public class ProxyResponse : RdcResponse
	{
		public RdcResponse Response { get; set; }
	}

}

// any
// {
// 	#/windows/x64|x86|arm64/10+.1+.0
// 	#/ubuntu/x64|x86|arm64/10+.1+.0
// }
// 
// microsoft/windows/x64|x86|arm64/10+.1+.0
