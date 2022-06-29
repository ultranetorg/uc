using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Org.BouncyCastle.Security;

namespace UC.Net
{
	public enum NciCall : byte
	{
		Null, GetMembers, NextRound, LastOperation, DelegateTransactions, GetOperationStatus, AuthorInfo, AccountInfo, 
		QueryRelease, DeclarePackage, LocatePackage, DownloadPackage
	}

	public abstract class Nci
	{
		public Account							Generator {get; set;} /// serializable
		public int								Failures;
		public int								ReachFailures;

 		public abstract Rp						Request<Rp>(Request rq) where Rp : class;
		//public abstract void					Send(Request rq);

		public NextRoundResponse				Send(NextRoundRequest call) => Request<NextRoundResponse>(call);
		public LastOperationResponse			Send(LastOperationRequest call) => Request<LastOperationResponse>(call);
		public DelegateTransactionsResponse		Send(DelegateTransactionsRequest call) => Request<DelegateTransactionsResponse>(call);
		public GetOperationStatusResponse		Send(GetOperationStatusRequest call) => Request<GetOperationStatusResponse>(call);
		public GetMembersResponse				Send(GetMembersRequest call) => Request<GetMembersResponse>(call);
		//public LocatePackageResponse			Send(LocatePackageRequest call) => Request<LocatePackageResponse>(call);

 
		public AuthorInfoResponse				GetAuthorInfo(string author, bool confirmed) => Request<AuthorInfoResponse>(new AuthorInfoRequest{ Name = author, Confirmed = confirmed });
		public AccountInfoResponse				GetAccountInfo(Account account, bool confirmed) => Request<AccountInfoResponse>(new AccountInfoRequest{ Account = account, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(ReleaseQuery query, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] {query}, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(ReleaseAddress release, VersionQuery version, string channel, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] {new ReleaseQuery(release, version, channel)}, Confirmed = confirmed });
		public LocatePackageResponse			LocatePackage(PackageAddress package, int count) => Request<LocatePackageResponse>(new LocatePackageRequest{ Package = package, Count = count  });
		public DeclarePackageResponse			DeclarePackage(IEnumerable<PackageAddress> packages) => Request<DeclarePackageResponse>(new DeclarePackageRequest{Packages = packages});
		public DownloadPackageResponse			DownloadPackage(PackageAddress package, long offset, long length) => Request<DownloadPackageResponse>(new DownloadPackageRequest{Package = package, Offset = offset, Length = length});
	}

	public abstract class Request : ITypedBinarySerializable
	{
		public byte[]					Id {get; set;}

		public byte						TypeCode => (byte)Type;
		public Peer						Peer;
		public ManualResetEvent			Event;
		public Response					RecievedResponse;
		//public bool						Sent;
		public Action					Process;

		public const int				IdLength = 8;
		static readonly SecureRandom	Random = new SecureRandom();

		public static Request FromType(Roundchain chaim, NciCall type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(GetMembersRequest).Namespace + "." + type + nameof(Request)).GetConstructor(new System.Type[]{}).Invoke(new object[]{ }) as Request;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Request)} type", ex);
			}
		}

		public NciCall Type
		{
			get
			{
				return Enum.Parse<NciCall>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Request))));
			}
		}

		public Request()
		{
			Id = new byte[IdLength];
			Random.NextBytes(Id);
			Event = new ManualResetEvent(false);
		}

		public abstract Response Execute(Core core);
	}

	public enum ResponseStatus
	{
		Null, OK, Failed
	}

	public abstract class Response : ITypedBinarySerializable
	{
		public byte[]			Id {get; set;}
		public ResponseStatus	Status {get; set;}
		public bool				Final {get; set;} = true;

		public byte				TypeCode => (byte)Type;
		public NciCall			Type => Enum.Parse<NciCall>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Response))));

		public static Response FromType(Roundchain chaim, NciCall type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(GetMembersRequest).Namespace + "." + type + nameof(Response)).GetConstructor(new System.Type[]{}).Invoke(new object[]{ }) as Response;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Response)} type", ex);
			}
		}
	}

	public class NextRoundRequest : Request
	{
		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
					return new NextRoundResponse {NextRoundId = core.GetNextAvailableRound().Id};
		}
	}

	public class NextRoundResponse : Response
	{
		public int NextRoundId { get;set; }
	}

	public class GetMembersRequest : Request
	{
		public override Response Execute(Core core)
		{
			lock(core.Lock)
 				if(core.Chain != null)
 				{
 					if(core.Synchronization == Synchronization.Synchronized)
 						return new GetMembersResponse { Members = core.Chain.Members };
 					else
 						throw new RpcException("Not synchronized");
 				}
 				else
 				{
 					return new GetMembersResponse { Members = core.Members };
 				}
		}
	}

	public class GetMembersResponse : Response
	{
		public IEnumerable<Peer> Members {get; set;}
	}
	
	public class LastOperationRequest : Request
	{
		public Account			Account {get; set;}
		public string			Class {get; set;}

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
				{
					var l = core.Chain.Accounts.FindLastOperation(Account, i => Class == null || i.GetType().Name == Class);
							
					return new LastOperationResponse {Operation = l};
				}
		}
	}

	public class LastOperationResponse : Response
	{
		public Operation Operation {get; set;}
	}

	public class DelegateTransactionsRequest : Request
	{
//		public byte[]					Data {get; set;}
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized || core.Generator == null)
					throw new RpcException("Not synchronized");
				else
				{
 					//var txs = new Packet(PacketType.Null, new MemoryStream(Data)).Read(r => {	return new Transaction(core.Settings)
 					//																			{
 					//																				Generator = core.Generator
 					//																			};
 					//																		});
					//var acc = core.ProcessIncoming(txs);
					
					var acc = core.ProcessIncoming(Transactions);

					return new DelegateTransactionsResponse {Accepted = acc.SelectMany(i => i.Operations)
																			.Select(i => new OperationAddress {Account = i.Signer, Id = i.Id})
																			.ToList()};
				}
		}
	}

	public class DelegateTransactionsResponse : Response
	{
		public IEnumerable<OperationAddress> Accepted { get; set; }
	}

	public class GetOperationStatusRequest : Request
	{
		public IEnumerable<OperationAddress>	Operations { get; set; }

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
				{
					return	new GetOperationStatusResponse
							{
								//LastConfirmedRound = core.Chain.LastConfirmedRound.Id,
								Operations = Operations.Select(o => new {	A = o,
																			B = core.Transactions.Where(t => t.Signer == o.Account && t.Operations.Any(i => i.Id == o.Id))
																									.SelectMany(t => t.Operations)
																									.FirstOrDefault(i => i.Id == o.Id)
																			?? 
																			core.Chain.Accounts.FindLastOperation(o.Account, i => i.Id == o.Id)})
														.Select(i => new GetOperationStatusResponse.Item {	Account		= i.A.Account,
																											Id			= i.A.Id,
																											Placing		= i.B == null ? PlacingStage.FailedOrNotFound : i.B.Placing}).ToArray()
							};
				}
		}
	}

	public class GetOperationStatusResponse : Response
	{
		public class Item
		{
			public Account			Account { get; set; }
			public int				Id { get; set; }
			public PlacingStage		Placing { get; set; }
		}

		public IEnumerable<Item> Operations { get; set; }
	}

	public class AccountInfoRequest : Request
	{
		public bool				Confirmed {get; set;}
		public Account			Account {get; set;}

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
 					return new AccountInfoResponse{ Info = core.Chain.GetAccountInfo(Account, Confirmed) };
		}
	}

	public class AccountInfoResponse : Response
	{
		public AccountInfo Info {get; set;}
	}

	public class AuthorInfoRequest : Request
	{
		public bool				Confirmed {get; set;}
		public string			Name {get; set;}

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
 					return new AuthorInfoResponse{Xon = core.Chain.GetAuthorInfo(Name, Confirmed, new XonTypedBinaryValueSerializator())};
		}
	}

	public class AuthorInfoResponse : Response
	{
		public XonDocument Xon {get; set;}
	}

	public class QueryReleaseRequest : Request
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }

		public bool							Valid => Queries.All(i => i.Valid);

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
 					return new QueryReleaseResponse {Manifests = Queries.Select(i => core.Chain.QueryRelease(i, Confirmed, new XonTypedBinaryValueSerializator()))};
		}
	}
	
	public class QueryReleaseResponse : Response
	{
		public IEnumerable<XonDocument> Manifests { get; set; }
	}

	public class DeclarePackageRequest : Request
	{
		public IEnumerable<PackageAddress> Packages { get; set; }

		public override Response Execute(Core core)
		{
			core.Hub.Declare(Peer.IP, Packages);

			return new DeclarePackageResponse();
		}
	}
	
	public class DeclarePackageResponse : Response
	{
	}

	public class LocatePackageRequest : Request
	{
		public PackageAddress	Package { get; set; }
		public int				Count { get; set; }

		public override Response Execute(Core core)
		{
			if(core.Hub == null)
			{
				throw new RequirementException("Is not Hub");
			}

			return new LocatePackageResponse {Seeders = core.Hub.Locate(this)};
		}
	}
	
	public class LocatePackageResponse : Response
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}

	public class DownloadPackageRequest : Request
	{
		public PackageAddress		Package { get; set; }
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public override Response Execute(Core core)
		{
			if(core.Filebase == null)
			{
				throw new RequirementException("Is not Filebase");
			}

			return new DownloadPackageResponse{Data = core.Filebase.ReadPackage(Package, Offset, Length)};
		}
	}

	public class DownloadPackageResponse : Response
	{
		public byte[] Data { get; set; }
	}
}
