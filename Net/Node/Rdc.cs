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
		Author, QueryResource, ResourceByName, ResourceById, DeclareRelease, LocateRelease, FileInfo, DownloadRelease,
		Stamp, TableStamp, DownloadTable, DownloadRounds
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
		public int			Id {get; set;}
		public Peer			Peer;

		public abstract byte TypeCode { get; }
	}

	public abstract class RdcInterface
	{
 		//public abstract RdcResponse				Request(RdcRequest rq);
 		//public Rp								Request<Rp>(RdcRequest rq) where Rp : RdcResponse => Request(rq) as Rp;
 		public abstract	void					Send(RdcRequest rq);

		public Rp								Request<Rp>(RdcCall<Rp> rq) where Rp : RdcResponse => Request((RdcRequest)rq) as Rp;
		public abstract RdcResponse				Request(RdcRequest rq);
// 
// 		public TimeResponse						GetTime() => Request<TimeResponse>(new TimeRequest());
// 		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
// 		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
// 		public DownloadTableResponse			DownloadTable(Tables table, byte[] cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
// 		//public AllocateTransactionResponse		AllocateTransaction() => Request<AllocateTransactionResponse>(new AllocateTransactionRequest());
// 		public PlaceTransactionsResponse		SendTransactions(IEnumerable<Transaction> transactions) => Request<PlaceTransactionsResponse>(new PlaceTransactionsRequest{Transactions = transactions.ToArray()});
// 		public TransactionStatusResponse		GetTransactionStatus(IEnumerable<TransactionsAddress> transactions) => Request<TransactionStatusResponse>(new TransactionStatusRequest{Transactions = transactions.ToArray()});
// 		public MembersResponse					GetMembers() => Request<MembersResponse>(new MembersRequest());
// 		public FundsResponse					GetFunds() => Request<FundsResponse>(new FundsRequest());
// 		public AuthorResponse					GetAuthorInfo(string author) => Request<AuthorResponse>(new AuthorRequest{Name = author});
// 		public AccountResponse					GetAccountInfo(AccountAddress account) => Request<AccountResponse>(new AccountRequest{Account = account});
// 		public ResourceByNameResponse			FindResource(ResourceAddress resource) => Request<ResourceByNameResponse>(new ResourceByNameRequest {Name = resource});
// 		//public SubresourcesResponse				EnumerateSubresources(ResourceAddress resource) => Request<SubresourcesResponse>(new SubresourcesRequest {Resource = resource});
// 		public QueryResourceResponse			QueryResource(string query) => Request<QueryResourceResponse>(new QueryResourceRequest {Query = query });
// 		public LocateReleaseResponse			LocateRelease(ReleaseAddress address, int count) => Request<LocateReleaseResponse>(new LocateReleaseRequest{Address = address, Count = count});
// 		//public void								DeclareRelease(IEnumerable<DeclareReleaseItem> releases) => Send(new DeclareReleaseRequest{Releases = releases.ToArray()});
// 		//public ManifestResponse					GetManifest(ReleaseAddress release) => Request<ManifestResponse>(new ManifestRequest{Release = release});
// 		public DownloadReleaseResponse			DownloadRelease(ReleaseAddress address, string file, long offset, long length) => Request<DownloadReleaseResponse>(new DownloadReleaseRequest{Address = address, File = file, Offset = offset, Length = length});
// 		//public ReleaseHistoryResponse			GetReleaseHistory(RealizationAddress realization) => Request<ReleaseHistoryResponse>(new ReleaseHistoryRequest{Realization = realization});
	}

	public abstract class RdcCall<RP> : RdcRequest where RP : RdcResponse
	{
		public override abstract RdcResponse Execute(Sun sun);
	}

	public abstract class RdcRequest : RdcPacket
	{
		public override byte			TypeCode => (byte)Class;
		public ManualResetEvent			Event;
		public RdcResponse				Response;
		public virtual bool				WaitResponse { get; protected set; } = true;

		public abstract RdcResponse		Execute(Sun sun);

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
			return Assembly.GetExecutingAssembly().GetType(typeof(RdcRequest).Namespace + "." + type + "Request").GetConstructor(new System.Type[]{}).Invoke(new object[]{ }) as RdcRequest;
		}

		public RdcResponse SafeExecute(Sun sun)
		{
			if(WaitResponse)
			{
				RdcResponse rp;

				try
				{
					rp = Execute(sun);
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
					Execute(sun);
				}
				catch(Exception ex) when(!Debugger.IsAttached || ex is EntityException || ex is NodeException || ex is RequestException)
				{
				}

				return null;
			}
		}

		protected void RequireBase(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new NodeException(NodeError.NotBase);

			if(sun.Synchronization != Synchronization.Synchronized)
				throw new NodeException(NodeError.NotSynchronized);
		}
		protected void RequireMember(Sun sun)
		{
			RequireBase(sun);

			if(!sun.NextVoteMembers.Any(i => sun.Settings.Generators.Contains(i.Account))) 
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
				return Assembly.GetExecutingAssembly().GetType(typeof(RdcResponse).Namespace + "." + type + "Response").GetConstructor(new System.Type[]{}).Invoke(null) as RdcResponse;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(RdcResponse)} type", ex);
			}
		}
	}

	public class ProxyRequest : RdcCall<ProxyResponse>
	{
		public byte[]			Guid { get; set; }
		public AccountAddress	Destination { get; set; }
		public RdcRequest		Request { get; set; }
		public override	bool	WaitResponse => Request.WaitResponse;

		public override RdcResponse Execute(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new NodeException(NodeError.NotBase);
			if(sun.Synchronization != Synchronization.Synchronized)
				throw new NodeException(NodeError.NotSynchronized);

			lock(sun.Lock)
			{
				if(sun.Connections.Any(i =>{
												lock(i.InRequests) 
													return i.InRequests.OfType<ProxyRequest>().Any(j => j.Destination == Destination && j.Guid == Guid);
											}))
					throw new NodeException(NodeError.CircularRoute);
			}

			if(sun.Settings.Generators.Contains(Destination))
			{
				return new ProxyResponse {Response = Request.SafeExecute(sun)};
			}
			else
			{
				Member m;

				lock(sun.Lock)
				{
					m = sun.Mcv.LastConfirmedRound.Members.Find(i => i.Account == Destination);
				}

				if(m?.Proxy != null)
				{
					sun.Connect(m.Proxy, sun.Workflow);
			
					return new ProxyResponse {Response = m.Proxy.Request<ProxyResponse>(new ProxyRequest {Guid = Guid, Destination = Destination, Request = Request}).Response};
				}
			}

			throw new NodeException(NodeError.NotOnlineYet);
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
