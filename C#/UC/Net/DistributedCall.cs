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
	public enum DistributedCall : byte
	{
		Null, DownloadRounds, GetMembers, NextRound, LastOperation, DelegateTransactions, GetOperationStatus, AuthorInfo, AccountInfo, 
		QueryRelease, ReleaseHistory, DeclareRelease, LocateRelease, Manifest, DownloadRelease
	}

 	public class DistributedCallException : Exception
 	{
		public Response	Response;

 		public DistributedCallException(Response response) : base(response.Error){}
		public DistributedCallException(string message) : base(message){}
		public DistributedCallException(string message, Exception ex) : base(message, ex){}
 	}

	public class OperationAddress : IBinarySerializable
	{
		public Account	Account { get; set; }
		public int		Id { get; set; }

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

	public abstract class Dci
	{
		public Account							Generator { get; set; }
		public int								Failures;

 		public abstract Rp						Request<Rp>(Request rq) where Rp : class;
 
		public NextRoundResponse				GetNextRound() => Request<NextRoundResponse>(new NextRoundRequest());
		public LastOperationResponse			GetLastOperation(Account account, string oclasss) => Request<LastOperationResponse>(new LastOperationRequest{Account = account, Class = oclasss});
		public DelegateTransactionsResponse		DelegateTransactions(IEnumerable<Transaction> transactions) => Request<DelegateTransactionsResponse>(new DelegateTransactionsRequest{Transactions = transactions});
		public GetOperationStatusResponse		GetOperationStatus(IEnumerable<OperationAddress> operations) => Request<GetOperationStatusResponse>(new GetOperationStatusRequest{Operations = operations});
		public GetMembersResponse				GetMembers() => Request<GetMembersResponse>(new GetMembersRequest());
		public AuthorInfoResponse				GetAuthorInfo(string author, bool confirmed) => Request<AuthorInfoResponse>(new AuthorInfoRequest{Name = author, Confirmed = confirmed });
		public AccountInfoResponse				GetAccountInfo(Account account, bool confirmed) => Request<AccountInfoResponse>(new AccountInfoRequest{Account = account, Confirmed = confirmed});
		public QueryReleaseResponse				QueryRelease(IEnumerable<ReleaseQuery> query, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = query, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(RealizationAddress realization, Version version, VersionQuery versionquery, string channel, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] {new ReleaseQuery(realization, version, versionquery, channel)}, Confirmed = confirmed });
		public LocateReleaseResponse			LocateRelease(ReleaseAddress package, int count) => Request<LocateReleaseResponse>(new LocateReleaseRequest{Release = package, Count = count});
		public DeclareReleaseResponse			DeclareRelease(Dictionary<ReleaseAddress, Distributive> packages) => Request<DeclareReleaseResponse>(new DeclareReleaseRequest{Packages = new PackageAddressPack(packages)});
		public ManifestResponse					GetManifest(ReleaseAddress packages) => Request<ManifestResponse>(new ManifestRequest{Releases = new[]{packages}});
		public DownloadReleaseResponse			DownloadRelease(ReleaseAddress release, Distributive distributive, long offset, long length) => Request<DownloadReleaseResponse>(new DownloadReleaseRequest{Package = release, Distributive = distributive, Offset = offset, Length = length});
		public ReleaseHistoryResponse			GetReleaseHistory(RealizationAddress realization, bool confirmed) => Request<ReleaseHistoryResponse>(new ReleaseHistoryRequest{Realization = realization, Confirmed = confirmed});
	}

	public abstract class Request : ITypedBinarySerializable
	{
		public byte[]					Id {get; set;}

		public byte						TypeCode => (byte)Type;
		public Peer						Peer;
		public ManualResetEvent			Event;
		public Response					Response;
		//public bool						Sent;
		public Action					Process;

		public const int				IdLength = 8;
		static readonly SecureRandom	Random = new SecureRandom();

		public static Request FromType(Roundchain chaim, DistributedCall type)
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

		public DistributedCall Type
		{
			get
			{
				return Enum.Parse<DistributedCall>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Request))));
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

	public abstract class Response : ITypedBinarySerializable
	{
		public byte[]			Id {get; set;}
		public string			Error {get; set;}
		public bool				Final {get; set;} = true;
		public Peer				Peer;

		public byte				TypeCode => (byte)Type;
		public DistributedCall	Type => Enum.Parse<DistributedCall>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Response))));

		public static Response FromType(Roundchain chaim, DistributedCall type)
		{
			try
			{
				return Assembly.GetExecutingAssembly().GetType(typeof(GetMembersRequest).Namespace + "." + type + nameof(Response)).GetConstructor(new System.Type[]{}).Invoke(new object[]{}) as Response;
			}
			catch(Exception ex)
			{
				throw new IntegrityException($"Wrong {nameof(Response)} type", ex);
			}
		}
	}

	public class DownloadRoundsRequest : Request
	{
		public int From { get; set; }
		public int To { get; set; }
		
		public override Response Execute(Core core)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write(Enumerable.Range(From, To - From + 1).Select(i => core.Chain.FindRound(i)).Where(i => i != null),  i => i.Write(w));
			
			return new DownloadRoundsResponse{Rounds = s.ToArray()};
		}
	}

	public class DownloadRoundsResponse : Response
	{
		public byte[] Rounds { get; set; }

		public Round[] Read(Roundchain chain)
		{
			var rd = new BinaryReader(new MemoryStream(Rounds));

			return rd.ReadArray<Round>(() =>	{
													var r = new Round(chain);
													r.Read(rd);
													return r;
												});
		}
	}

	public class NextRoundRequest : Request
	{
		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RequirementException("Not synchronized");
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
 						throw new RequirementException("Not synchronized");
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
		public Account	Account {get; set;}
		public string	Class {get; set;}

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RequirementException("Not synchronized");
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
					throw new RequirementException("Not synchronized");
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
					throw new RequirementException("Not synchronized");
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
					throw new RequirementException("Not synchronized");
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
					throw new RequirementException("Not synchronized");
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

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RequirementException("Not synchronized");
				else
 					return new QueryReleaseResponse {Releases = Queries.Select(i => core.Chain.QueryRelease(i, Confirmed))};
		}
	}
	
	public class QueryReleaseResponse : Response
	{
		public IEnumerable<QueryReleaseResult> Releases { get; set; }
	}

	public class QueryReleaseResult
	{
		public ReleaseRegistration				Registration { get; set; }
		//public IEnumerable<ReleaseAddress>	Dependencies { get; set; }
	}

	public class DeclareReleaseRequest : Request
	{
		public PackageAddressPack Packages { get; set; }

		public override Response Execute(Core core)
		{
			core.Hub.Add(Peer.IP, Packages.Items);

			return new DeclareReleaseResponse();
		}
	}
	
	public class DeclareReleaseResponse : Response
	{
	}

	public class LocateReleaseRequest : Request
	{
		public ReleaseAddress	Release { get; set; }
		public int				Count { get; set; }

		public override Response Execute(Core core)
		{
			if(core.Hub == null)
			{
				throw new RequirementException("Is not Hub");
			}

			return new LocateReleaseResponse {Seeders = core.Hub.Locate(this)};
		}
	}
	
	public class LocateReleaseResponse : Response
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}

	public class ManifestRequest : Request
	{
		public IEnumerable<ReleaseAddress>	Releases { get; set; }

		public override Response Execute(Core core)
		{
			if(core.Filebase == null)
			{
				throw new RequirementException("Is not Filebase");
			}

			return new ManifestResponse{Manifests = Releases.Select(i => core.Filebase.FindRelease(i)?.Manifest).ToArray()};
		}
	}
	
	public class ManifestResponse : Response
	{
		public IEnumerable<Manifest> Manifests { get; set; }
	}

	public class DownloadReleaseRequest : Request
	{
		public ReleaseAddress		Package { get; set; }
		public Distributive			Distributive { get; set; }
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public override Response Execute(Core core)
		{
			if(core.Filebase == null)
			{
				throw new RequirementException("Is not Filebase");
			}

			return new DownloadReleaseResponse{Data = core.Filebase.ReadPackage(Package, Distributive, Offset, Length)};
		}
	}

	public class DownloadReleaseResponse : Response
	{
		public byte[] Data { get; set; }
	}

	public class ReleaseHistoryRequest : Request
	{
		public RealizationAddress	Realization { get; set; }
		public bool					Confirmed { get; set; }

		public override Response Execute(Core core)
		{
			return core.Chain.GetReleaseHistory(Realization, Confirmed);
		}
	}
	
	public class ReleaseHistoryResponse : Response
	{
		public IEnumerable<ReleaseRegistration> Registrations { get; set; }
	}

}
