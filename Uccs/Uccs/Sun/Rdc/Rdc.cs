using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml.Linq;
using NativeImport;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using static Uccs.Net.ChainReportResponse;
using static Uccs.Net.Download;
using static Uccs.Net.PiecesReportResponse;

namespace Uccs.Net
{
	public enum Rdc : byte
	{
		Null, GeneratorJoinBroadcast, GeneratorOnlineBroadcast, Time, PeersBroadcast, BlocksBroadcast, DownloadRounds, Members, NextRound, LastOperation, SendTransactions, GetOperationStatus, Author, Account, 
		QueryRelease, ReleaseHistory, DeclareRelease, LocateRelease, Manifest, DownloadRelease,
		Stamp, TableStamp, DownloadTable
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
		NotChain,
		NotBase,
		NotHub,
		NotSeed,
		NotSynchronized,
		TooEearly,
		AllNodesFailed,
	}

	public enum RdcEntityError : byte
	{
		Null,
		InvalidRequest,
		AccountNotFound,
		ProductNotFound,
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
		public AccountResponse					GetAccountInfo(AccountAddress account, bool confirmed) => Request<AccountResponse>(new AccountRequest{Account = account});
		public QueryReleaseResponse				QueryRelease(IEnumerable<ReleaseQuery> query, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = query, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(RealizationAddress realization, Version version, VersionQuery versionquery, string channel, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] {new ReleaseQuery(realization, version, versionquery, channel)}, Confirmed = confirmed });
		public LocateReleaseResponse			LocateRelease(ReleaseAddress package, int count) => Request<LocateReleaseResponse>(new LocateReleaseRequest{Release = package, Count = count});
		public void								DeclareRelease(Dictionary<ReleaseAddress, Distributive> packages) => Send(new DeclareReleaseRequest{Packages = new PackageAddressPack(packages)});
		public ManifestResponse					GetManifest(ReleaseAddress release) => Request<ManifestResponse>(new ManifestRequest{Release = release});
		public DownloadReleaseResponse			DownloadRelease(ReleaseAddress release, Distributive distributive, long offset, long length) => Request<DownloadReleaseResponse>(new DownloadReleaseRequest{Package = release, Distributive = distributive, Offset = offset, Length = length});
		public ReleaseHistoryResponse			GetReleaseHistory(RealizationAddress realization) => Request<ReleaseHistoryResponse>(new ReleaseHistoryRequest{Realization = realization});
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

		public static RdcRequest FromType(Database chaim, Rdc type)
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

		public static RdcResponse FromType(Database chaim, Rdc type)
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

}
