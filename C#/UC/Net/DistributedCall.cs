using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Org.BouncyCastle.Security;
using static UC.Net.Download;

namespace UC.Net
{
	public enum DistributedCall : byte
	{
		Null, UploadBlocksPieces, DownloadRounds, GetMembers, NextRound, LastOperation, DelegateTransactions, GetOperationStatus, AuthorInfo, AccountInfo, 
		QueryRelease, ReleaseHistory, DeclareRelease, LocateRelease, Manifest, DownloadRelease,
		Stamp, TableStamp, DownloadTable
	}

	public enum Error
	{
		Null,
		Internal,
		Timeout,
		InvalidRequest,
		NotChain,
		NotBase,
		NotHub,
		NotSeed,
		NotSynchronized,
		TooEearly,
		AccountNotFound,
		ProductNotFound,
		ClusterNotFound,
		RoundNotAvailable,
		AllNodesFailed,
		UnknownTable,
	}

 	public class DistributedCallException : Exception
 	{
		//public Response	Response;
		public Error Error;

 		public DistributedCallException(Error erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
		//public DistributedCallException(string message) : base(message){}
		//public DistributedCallException(string message, Exception ex) : base(message, ex){}
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
		//public Account							Generator { get; set; }
		public int								Failures;

 		public abstract Rp						Request<Rp>(Request rq) where Rp : class;
 
		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
		public DownloadTableResponse			DownloadTable(Tables table, ushort cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
		public NextRoundResponse				GetNextRound() => Request<NextRoundResponse>(new NextRoundRequest());
		//public LastOperationResponse			GetLastOperation(Account account, string oclasss, PlacingStage placing) => Request<LastOperationResponse>(new LastOperationRequest{Account = account, Class = oclasss, Placing = placing});
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
		public Action					Process;
		public virtual bool				WaitResponse { get; protected set; } = true;

		public const int				IdLength = 8;

		public static Request FromType(Database chaim, DistributedCall type)
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
			Cryptography.Random.NextBytes(Id);
			Event = new ManualResetEvent(false);
		}

		public abstract Response Execute(Core core);
	}

	public abstract class Response : ITypedBinarySerializable
	{
		public byte[]			Id { get; set; }
		public Error			Error { get; set; }
		public bool				Final { get; set; } = true;
		public Peer				Peer;

		public byte				TypeCode => (byte)Type;
		public DistributedCall	Type => Enum.Parse<DistributedCall>(GetType().Name.Remove(GetType().Name.IndexOf(nameof(Response))));

		public static Response FromType(Database chaim, DistributedCall type)
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

	public class UploadBlocksPiecesRequest : Request
	{
		public IEnumerable<BlockPiece>	Pieces { get; set; }
		public override bool			WaitResponse => false;

		public override Response Execute(Core core)
		{
			void broadcast(IEnumerable<BlockPiece> pieces)
			{
				if(!pieces.Any())
				{
					return;
				}

				foreach(var i in core.Connections.Where(i => i.BaseRank > 0 && i != Peer).OrderBy(i => Guid.NewGuid()))
				{
					i.Request<object>(new UploadBlocksPiecesRequest{Pieces = pieces});
				}
			}

			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)
					throw new DistributedCallException(Error.NotBase);
				
				var accepted = new List<BlockPiece>();

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
					var a = Pieces.Where(p => !core.SyncBlockCache.Any(i => i.Signature.SequenceEqual(p.Signature))).ToArray(); /// !ToArray cause will be added to Chain below

					if(a.Any())
					{
						core.SyncBlockCache.AddRange(a);
						accepted.AddRange(a);
					}
				}

				if(core.Synchronization == Synchronization.Synchronized)
				{
					var notolder = core.Database.LastConfirmedRound.Id - Database.Pitch;
					var notnewer = core.Database.LastConfirmedRound.Id + Database.Pitch * 2;

					foreach(var p in Pieces)
					{
						if(Cryptography.Current.AccountFrom(p.Signature, p.Hashify()) != p.Generator)
						{
							continue;
						}

						if(notolder <= p.RoundId && p.RoundId <= notnewer)
						{
							var r = core.Database.GetRound(p.RoundId);
				
							var pp = r.BlockPieces.Find(i => i.Signature.SequenceEqual(p.Signature));

							if(pp == null)
							{
								accepted.Add(p);
								r.BlockPieces.Add(p);
				
								var ps = r.BlockPieces.Where(i => i.Generator == p.Generator && i.Guid.SequenceEqual(p.Guid)).OrderBy(i => i.Piece);
			
								if(ps.Count() == p.PiecesTotal && ps.Zip(ps.Skip(1), (x, y) => x.Piece + 1 == y.Piece).All(x => x))
								{
									var s = new MemoryStream();
									var w = new BinaryWriter(s);
				
									foreach(var i in ps)
									{
										s.Write(i.Data);
									}
				
									s.Position = 0;
									var rd = new BinaryReader(s);
				
									var b = Block.FromType(core.Database, (BlockType)rd.ReadByte());
									b.Read(rd);
				
									core.ProcessIncoming(new Block[] {b}, null);
		
									//r.BlockPieces.RemoveAll(i => ps.Contains(i));
								}
							}
							else
								if(pp.Peer != null && pp.Peer != Peer)
									pp.Distributed = true;
						}
						//else
						//{
 						//	accepted.Add(p);
						//}
					}
				}

				broadcast(accepted);
			}

			return null; 
		}
	}

	public class DownloadRoundsRequest : Request
	{
		public int From { get; set; }
		public int To { get; set; }
		
		public override Response Execute(Core core)
		{
			lock(core.Lock)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
			
				w.Write(Enumerable.Range(From, To - From + 1).Select(i => core.Database.FindRound(i)).Where(i => i != null), i => i.Write(w));
			
				return new DownloadRoundsResponse {	LastNonEmptyRound	= core.Database.LastNonEmptyRound.Id,
													LastConfirmedRound	= core.Database.LastConfirmedRound.Id,
													BaseHash			= core.Database.BaseHash,
													Rounds				= s.ToArray()};
			}
		}
	}

	public class DownloadRoundsResponse : Response
	{
		public int		LastNonEmptyRound { get; set; }
		public int		LastConfirmedRound { get; set; }
		public byte[]	BaseHash{ get; set; }
		public byte[]	Rounds { get; set; }

		public Round[] Read(Database chain)
		{
			var rd = new BinaryReader(new MemoryStream(Rounds));

			return rd.ReadArray<Round>(() =>{
												var r = new Round(chain);
												r.Read(rd);
												return r;
											});
		}
	}

	public class NextRoundRequest : Request
	{
		public Account Generator;

		public override Response Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)
					throw new DistributedCallException(Error.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);

				var r = core.Database.LastConfirmedRound.Id + Database.Pitch * 2;
				
				return new NextRoundResponse {NextRoundId = r};
			}
		}
	}

	public class NextRoundResponse : Response
	{
		public int NextRoundId { get; set; }
	}

	public class StampRequest : Request
	{
		public override Response Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);
				if(core.Database.BaseState == null)
					throw new DistributedCallException(Error.TooEearly);

				return new StampResponse {	BaseState				= core.Database.BaseState,
											BaseHash				= core.Database.BaseHash,
											LastCommitedRoundHash	= core.Database.LastCommittedRound.Hash,
											FirstTailRound			= core.Database.Rounds.Last().Id,
											LastTailRound			= core.Database.Rounds.First().Id,
											Accounts				= core.Database.Accounts.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Authors					= core.Database.Authors.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Products				= core.Database.Products.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Realizations			= core.Database.Realizations.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Releases				= core.Database.Releases.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray()};
			}
		}
	}

	public class StampResponse : Response
	{
		public class SuperCluster
		{
			public byte		Id { get; set; }
			public byte[]	Hash { get; set; }
		}

		public byte[]						BaseState { get; set; }
		public byte[]						BaseHash { get; set; }
		public int							FirstTailRound { get; set; }
		public int							LastTailRound { get; set; }
		public byte[]						LastCommitedRoundHash { get; set; }
		public IEnumerable<SuperCluster>	Accounts { get; set; }
		public IEnumerable<SuperCluster>	Authors { get; set; }
		public IEnumerable<SuperCluster>	Products { get; set; }
		public IEnumerable<SuperCluster>	Realizations { get; set; }
		public IEnumerable<SuperCluster>	Releases { get; set; }
	}

	public class TableStampRequest : Request
	{
		public Tables	Table { get; set; }
		public byte[]	SuperClusters { get; set; }

		public override Response Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);
				if(core.Database.BaseState == null)
					throw new DistributedCallException(Error.TooEearly);

				switch(Table)
				{
					case Tables.Accounts	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Accounts		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors		: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Authors		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Products	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Products		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Realizations: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Realizations	.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Releases	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Releases		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					default:
						throw new DistributedCallException(Error.InvalidRequest);
				}
			}
		}
	}

	public class TableStampResponse : Response
	{
		public class Cluster
		{
			public int		Id { get; set; }
			public int		Length { get; set; }
			public byte[]	Hash { get; set; }
		}
	
		public IEnumerable<Cluster>	Clusters { get; set; }
	}

	public class DownloadTableRequest : Request
	{
		public Tables	Table { get; set; }
		public int		ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override Response Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Database == null || !core.Database.Settings.Database.Base)
					throw new DistributedCallException(Error.NotBase);

				var m = Table switch
							  {
									Tables.Accounts		=> core.Database.Accounts.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Authors		=> core.Database.Authors.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Products		=> core.Database.Products.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Realizations => core.Database.Realizations.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Releases		=> core.Database.Releases.Clusters.Find(i => i.Id == ClusterId)?.Main,
									_ => throw new DistributedCallException(Error.InvalidRequest)
							  };

				if(m == null)
					throw new DistributedCallException(Error.ClusterNotFound);
	
				var s = new MemoryStream(m);
				var r = new BinaryReader(s);
	
				s.Position = Offset;
	
				return new DownloadTableResponse{Data = r.ReadBytes((int)Length)};
			}
		}
	}

	public class DownloadTableResponse : Response
	{
		public byte[] Data { get; set; }
	}
	
	public class GetMembersRequest : Request
	{
		public override Response Execute(Core core)
		{
			lock(core.Lock)
 				if(core.Database != null)
 				{
 					if(core.Synchronization == Synchronization.Synchronized)
 						return new GetMembersResponse { Members = core.Database.Members };
 					else
 						throw new DistributedCallException(Error.NotSynchronized);
 				}
 				else
 					return new GetMembersResponse { Members = core.Members };
		}
	}

	public class GetMembersResponse : Response
	{
		public IEnumerable<Member> Members {get; set;}
	}
	
// 	public class LastOperationRequest : Request
// 	{
// 		public Account		Account {get; set;}
// 		public string		Class {get; set;}
// 		public PlacingStage	Placing {get; set;} /// Null means any
// 
// 		public override Response Execute(Core core)
// 		{
// 			lock(core.Lock)
// 				if(core.Synchronization != Synchronization.Synchronized)
// 					throw new DistributedCallException(Error.NotSynchronized);
// 				else
// 				{
// 					var l = core.Database.Accounts.FindLastOperation(Account, i =>	(Class == null || i.GetType().Name == Class) && 
// 																					(Placing == PlacingStage.Null || i.Placing == Placing));
// 							
// 					return new LastOperationResponse {Operation = l};
// 				}
// 		}
// 	}
//
// 	public class LastOperationResponse : Response
// 	{
// 		public Operation Operation {get; set;}
// 	}

	public class DelegateTransactionsRequest : Request
	{
//		public byte[]					Data {get; set;}
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override Response Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);
				else
				{
					var acc = core.ProcessIncoming(Transactions);

					return new DelegateTransactionsResponse {Accepted = acc	.SelectMany(i => i.Operations)
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
			{
				if(!core.Settings.Database.Chain)
					throw new DistributedCallException(Error.NotChain);
				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);
	
				return	new GetOperationStatusResponse
						{
							Operations = Operations.Select(o => new {	A = o,
																		B = core.Transactions.Where(t => t.Signer == o.Account && t.Operations.Any(i => i.Id == o.Id))
																								.SelectMany(t => t.Operations)
																								.FirstOrDefault(i => i.Id == o.Id)
																		?? 
																		core.Database.Accounts.FindLastOperation(o.Account, i => i.Id == o.Id)})
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
		public bool			Confirmed {get; set;}
		public Account		Account {get; set;}

		public override Response Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new DistributedCallException(Error.NotSynchronized);
				else
				{
					var ai = core.Database.GetAccountInfo(Account, Confirmed);

					if(ai == null)
						throw new DistributedCallException(Error.AccountNotFound);

 					return new AccountInfoResponse{Info = ai};
				}
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
					throw new DistributedCallException(Error.NotSynchronized);
				else
 					return new AuthorInfoResponse{Xon = core.Database.GetAuthorInfo(Name, Confirmed, new XonTypedBinaryValueSerializator())};
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
					throw new DistributedCallException(Error.NotSynchronized);
				else
 					return new QueryReleaseResponse {Releases = Queries.Select(i => core.Database.QueryRelease(i, Confirmed))};
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
				throw new DistributedCallException(Error.NotHub);
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
				throw new DistributedCallException(Error.NotSeed);
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
				throw new DistributedCallException(Error.NotSeed);
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
			var db = core.Database;

			var rmax = Confirmed ? db.LastConfirmedRound : db.LastPayloadRound;
				
			var p = db.Products.Find(Realization, rmax.Id);
			
			if(p != null)
			{
				var ms = new List<ReleaseRegistration>();

				foreach(var r in p.Releases)
				{
					var rr = db.FindRound(r.Rid).FindOperation<ReleaseRegistration>(i => i.Release == Realization && i.Release.Version == r.Version);
					//var re = FindRound(r.Rid).FindProduct(query).Releases.Find(i => i.Platform == query.Platform && i.Version == query.Version);
					ms.Add(rr);
				}

				return new ReleaseHistoryResponse{Releases = ms};
			}
			else
				throw new DistributedCallException(Error.ProductNotFound);
		}
	}
	
	public class ReleaseHistoryResponse : Response
	{
		public IEnumerable<ReleaseRegistration> Releases { get; set; }
	}
}
