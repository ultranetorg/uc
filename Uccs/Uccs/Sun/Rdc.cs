using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public enum Rdc : byte
	{
		None, 
		Proxy, 
		MemberJoin, MemberVox, AnalyzerVox,
		PeersBroadcast, Time, Members, Member, Funds, AllocateTransaction, LastOperation, SendTransactions, TransactionStatus, Account, 
		Author, QueryResource, Resource, Subresources, DeclareRelease, LocateRelease, FileInfo, DownloadRelease,
		Stamp, TableStamp, DownloadTable, DownloadRounds,
		Analysis
	}

	public enum RdcResult : byte
	{
		None, Success, NodeException, EntityException
	}

	public enum RdcNodeError : byte
	{
		None,
		Connectivity,
		Integrity,
		Internal,
		Timeout,
		NoIP,
		//NotFound,
		NotBase,
		NotChain,
		NotSeed,
		NotMember,
		NotSynchronized,
		TooEearly,
		AllNodesFailed,
		NotOnlineYet,
		CircularRoute,
	}

	public enum RdcEntityError : byte
	{
		None,
		InvalidRequest,
		NotFound,
		RoundNotAvailable,
	}

 	public class RdcNodeException : Exception
 	{
		public RdcNodeError Error;
		public string		ErrorDetails;

 		public RdcNodeException(RdcNodeError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}

 		public RdcNodeException(RdcNodeError erorr, string errordetails) : base(erorr.ToString())
		{
			Error = erorr;
			ErrorDetails = errordetails;
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

	public abstract class RdcInterface
	{
		public int								Failures;

 		public abstract RdcResponse				Request(RdcRequest rq);
 		public Rp								Request<Rp>(RdcRequest rq) where Rp : RdcResponse => Request(rq) as Rp;
 		public abstract	void					Send(RdcRequest rq);

		public TimeResponse						GetTime() => Request<TimeResponse>(new TimeRequest());
		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
		public DownloadTableResponse			DownloadTable(Tables table, ushort cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
		//public AllocateTransactionResponse		AllocateTransaction() => Request<AllocateTransactionResponse>(new AllocateTransactionRequest());
		public SendTransactionsResponse			SendTransactions(IEnumerable<Transaction> transactions) => Request<SendTransactionsResponse>(new SendTransactionsRequest{Transactions = transactions});
		public TransactionStatusResponse		GetTransactionStatus(IEnumerable<TransactionsAddress> operations) => Request<TransactionStatusResponse>(new TransactionStatusRequest{Transactions = operations});
		public MembersResponse					GetMembers() => Request<MembersResponse>(new MembersRequest());
		public FundsResponse					GetFunds() => Request<FundsResponse>(new FundsRequest());
		public AuthorResponse					GetAuthorInfo(string author) => Request<AuthorResponse>(new AuthorRequest{Name = author});
		public AccountResponse					GetAccountInfo(AccountAddress account) => Request<AccountResponse>(new AccountRequest{Account = account});
		public ResourceResponse					FindResource(ResourceAddress resource) => Request<ResourceResponse>(new ResourceRequest {Resource = resource});
		public SubresourcesResponse				EnumerateSubresources(ResourceAddress resource) => Request<SubresourcesResponse>(new SubresourcesRequest {Resource = resource});
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
		public override byte			TypeCode => (byte)Class;
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

		public Rdc Class
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

		public abstract RdcResponse Execute(Sun sun);


		protected void RequireBase(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new RdcNodeException(RdcNodeError.NotBase);
		}

		protected void RequireSynchronizedBase(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new RdcNodeException(RdcNodeError.NotBase);

			if(sun.Synchronization != Synchronization.Synchronized)
				throw new RdcNodeException(RdcNodeError.NotSynchronized);
		}

		protected void RequireSynchronized(Sun sun)
		{
			if(sun.Synchronization != Synchronization.Synchronized)
				throw new RdcNodeException(RdcNodeError.NotSynchronized);
		}
	}

	public class RdcRequestJsonConverter : JsonConverter<RdcRequest>
	{
		public override void Write(Utf8JsonWriter writer, RdcRequest value, JsonSerializerOptions options)
		{
			//writer.WriteStartObject();
			//writer.WritePropertyName("Class");
			//writer.WriteStringValue(value.Type.ToString());
			//writer.WritePropertyName("Request");
			//writer.WriteRawValue(JsonSerializer.Serialize(value, value.GetType(), options));
			//writer.WriteEndObject();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			BinarySerializator.Serialize(w, value);
			
			writer.WriteStringValue(value.Class.ToString() + ":" + s.ToArray().ToHex());

		}

		public override RdcRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			//reader.Read();
			//reader.Read();
			//var t = RdcRequest.FromType(Enum.Parse<Rdc>(reader.GetString()));
			//reader.Read();
			//
			//return JsonSerializer.Deserialize(reader.GetString(), t, options);

			var s = reader.GetString().Split(':');
			var o = RdcRequest.FromType(Enum.Parse<Rdc>(s[0]));
 			
			var r = new BinaryReader(new MemoryStream(s[1].HexToByteArray()));

			return BinarySerializator.Deserialize<RdcRequest>(r,(t, b) =>	{ 
																				if(t == typeof(RdcRequest)) return RdcRequest.FromType((Rdc)b);

																				return null;
																			});
		}
	}


	public abstract class RdcResponse : RdcPacket
	{
		public override byte	TypeCode => (byte)Type;
		public Rdc				Type => Enum.Parse<Rdc>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));
		public RdcResult		Result { get; set; }
		public byte				Error { get; set; }
		public string			ErrorDetails { get; set; }

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

		public override RdcResponse Execute(Sun sun)
		{
			if(!sun.Roles.HasFlag(Role.Base))
				throw new RdcNodeException(RdcNodeError.NotBase);
			if(sun.Synchronization != Synchronization.Synchronized)
				throw new RdcNodeException(RdcNodeError.NotSynchronized);

			lock(sun.Lock)
			{
				if(sun.Connections.Any(i =>{
												lock(i.InRequests) 
													return i.InRequests.OfType<ProxyRequest>().Any(j => j.Destination == Destination && j.Guid == Guid);
											}))
					throw new RdcNodeException(RdcNodeError.CircularRoute);
			}

			if(sun.Settings.Generators.Contains(Destination))
			{
				return new ProxyResponse {Response = Request.Execute(sun)};
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
