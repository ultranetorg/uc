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
	public enum RpcType : byte
	{
		Null, GetMembers, NextRound, LastOperation, DelegateTransactions, GetOperationStatus, AuthorInfo, AccountInfo, QueryRelease
	}

	public abstract class Request : ITypedBinarySerializable
	{
		public byte[]					Id {get; set;}

		public byte						TypeCode => (byte)Type;
		public AutoResetEvent			Event;
		public Response					RecievedResponse;
		public bool						Sent;

		public const int				IdLength = 8;
		static readonly SecureRandom	Random = new SecureRandom();

		public static Request FromType(Roundchain chaim, RpcType type)
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

		public RpcType Type
		{
			get
			{
				return Enum.Parse<RpcType>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Request))));
			}
		}

		public Request()
		{
			Id = new byte[IdLength];
			Random.NextBytes(Id);
			Event = new AutoResetEvent(false);
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

		public byte				TypeCode => (byte)Type;
		public RpcType			Type => Enum.Parse<RpcType>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Response))));

		public static Response FromType(Roundchain chaim, RpcType type)
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
		public byte[]	Data {get; set;}

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized || core.Generator == null)
					throw new RpcException("Not synchronized");
				else
				{
					var txs = core.Read(new MemoryStream(Data), r => { return	new Transaction(core.Settings)
																				{
																					Generator = core.Generator
																				};
																	});
					var acc = core.ProcessIncoming(txs);

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

	public class AuthorInfoResponse : Response
	{
		public XonDocument Xon {get; set;}
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
	
	public class QueryReleaseResponse : Response
	{
		public IEnumerable<XonDocument> Xons {get; set;}
	}

	public class QueryReleaseRequest : Request
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed {get; set;}

		public bool							Valid => Queries.All(i => i.Valid);

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RpcException("Not synchronized");
				else
 					return new QueryReleaseResponse{ Xons = Queries.Select(i => core.Chain.QueryRelease(i, Confirmed, new XonTypedBinaryValueSerializator())) };
		}
	}

	public class LocatePackageRequest : Request
	{
		public PackageAddress		Package { get; set; }

		public override Response Execute(Core core)
		{
			throw new NotImplementedException();
		}
	}
	
	public class LocatePackageResponse : Response
	{
		public IEnumerable<IPAddress>	Peers { get; set; }
	}

	public class DownloadPackageRequest : Request
	{
		public PackageDownload	Request { get; set; }
		public bool				Valid => Request.Valid;

		public override Response Execute(Core core)
		{
			throw new NotImplementedException();
		}
	}

	public class DownloadPackagerResponse : Response
	{
		public byte[]		Data { get; set; }
	}

	//public class ReleaseRequest : IBinarySerializable
	//{
	//	public ReleaseAddress	Address;
	//	public string			Localization; /// empty means default
	//
	//	public bool				Valid => Address.Valid;
	//
	//	public ReleaseRequest()
	//	{
	//	}
	//
	//	public ReleaseRequest(ReleaseAddress address, string localization)
	//	{
	//		Address = address;
	//		Localization = localization;
	//	}
	//
	//	public void Read(BinaryReader r)
	//	{
	//		Address = r.ReadReleaseAddress();
	//		Localization = r.ReadUtf8();
	//	}
	//
	//	public void Write(BinaryWriter w)
	//	{
	//		w.Write(Address);
	//		w.WriteUtf8(Localization);
	//	}
	//}
}
