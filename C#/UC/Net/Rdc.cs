using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Org.BouncyCastle.Security;

namespace UC.Net
{
	public enum Rdc : byte
	{
		Null, UploadBlocksPieces, DownloadRounds, GetMembers, NextRound, LastOperation, DelegateTransactions, GetOperationStatus, AuthorInfo, AccountInfo, 
		QueryRelease, ReleaseHistory, DeclareRelease, LocateRelease, Manifest, DownloadRelease,
		Stamp, TableStamp, DownloadTable
	}

	public enum RdcError
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

 	public class RdcException : Exception
 	{
		public RdcError Error;

 		public RdcException(RdcError erorr) : base(erorr.ToString())
		{
			Error = erorr;
		}
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

	public abstract class RdcInterface
	{
		public int								Failures;

 		public abstract Rp						Request<Rp>(RdcRequest rq) where Rp : class;
 
		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
		public DownloadTableResponse			DownloadTable(Tables table, ushort cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
		public NextRoundResponse				GetNextRound() => Request<NextRoundResponse>(new NextRoundRequest());
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

	public abstract class RdcRequest : ITypedBinarySerializable
	{
		public byte[]					Id {get; set;}
		public byte						TypeCode => (byte)Type;
		public Peer						Peer;
		public ManualResetEvent			Event;
		public RdcResponse				Response;
		public Action					Process;
		public virtual bool				WaitResponse { get; protected set; } = true;

		public const int				IdLength = 8;

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
			Id = new byte[IdLength];
			Cryptography.Random.NextBytes(Id);
			Event = new ManualResetEvent(false);
		}

		public abstract RdcResponse Execute(Core core);
	}

	public abstract class RdcResponse : ITypedBinarySerializable
	{
		public byte[]	Id { get; set; }
		public RdcError	Error { get; set; }
		public bool		Final { get; set; } = true;
		public Peer		Peer;

		public byte		TypeCode => (byte)Type;
		public Rdc		Type => Enum.Parse<Rdc>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));

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

	public class UploadBlocksPiecesRequest : RdcRequest
	{
		public IEnumerable<BlockPiece>	Pieces { get; set; }
		public override bool			WaitResponse => false;

		public override RdcResponse Execute(Core core)
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
					throw new RdcException(RdcError.NotBase);
				
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

	public class DownloadRoundsRequest : RdcRequest
	{
		public int From { get; set; }
		public int To { get; set; }
		
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)
					throw new RdcException(RdcError.NotBase);

				if(core.Database.LastNonEmptyRound == null)
					throw new RdcException(RdcError.TooEearly);

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

	public class DownloadRoundsResponse : RdcResponse
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

	public class NextRoundRequest : RdcRequest
	{
		public Account Generator;

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)
					throw new RdcException(RdcError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);

				var r = core.Database.LastConfirmedRound.Id + Database.Pitch * 2;
				
				return new NextRoundResponse {NextRoundId = r};
			}
		}
	}

	public class NextRoundResponse : RdcResponse
	{
		public int NextRoundId { get; set; }
	}

	public class StampRequest : RdcRequest
	{
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				if(core.Database.BaseState == null)
					throw new RdcException(RdcError.TooEearly);

				return new StampResponse {	BaseState				= core.Database.BaseState,
											BaseHash				= core.Database.BaseHash,
											LastCommitedRoundHash	= core.Database.LastCommittedRound.Hash,
											FirstTailRound			= core.Database.Tail.Last().Id,
											LastTailRound			= core.Database.Tail.First().Id,
											Accounts				= core.Database.Accounts.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Authors					= core.Database.Authors.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Products				= core.Database.Products.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Realizations			= core.Database.Realizations.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Releases				= core.Database.Releases.		SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray()};
			}
		}
	}

	public class StampResponse : RdcResponse
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

	public class TableStampRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public byte[]	SuperClusters { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				if(core.Database.BaseState == null)
					throw new RdcException(RdcError.TooEearly);

				switch(Table)
				{
					case Tables.Accounts	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Accounts		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors		: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Authors		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Products	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Products		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Realizations: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Realizations	.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Releases	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Releases		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					default:
						throw new RdcException(RdcError.InvalidRequest);
				}
			}
		}
	}

	public class TableStampResponse : RdcResponse
	{
		public class Cluster
		{
			public int		Id { get; set; }
			public int		Length { get; set; }
			public byte[]	Hash { get; set; }
		}
	
		public IEnumerable<Cluster>	Clusters { get; set; }
	}

	public class DownloadTableRequest : RdcRequest
	{
		public Tables	Table { get; set; }
		public int		ClusterId { get; set; }
		public long		Offset { get; set; }
		public long		Length { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(core.Database == null || !core.Database.Settings.Database.Base)
					throw new RdcException(RdcError.NotBase);

				var m = Table switch
							  {
									Tables.Accounts		=> core.Database.Accounts.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Authors		=> core.Database.Authors.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Products		=> core.Database.Products.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Realizations => core.Database.Realizations.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Releases		=> core.Database.Releases.Clusters.Find(i => i.Id == ClusterId)?.Main,
									_ => throw new RdcException(RdcError.InvalidRequest)
							  };

				if(m == null)
					throw new RdcException(RdcError.ClusterNotFound);
	
				var s = new MemoryStream(m);
				var r = new BinaryReader(s);
	
				s.Position = Offset;
	
				return new DownloadTableResponse{Data = r.ReadBytes((int)Length)};
			}
		}
	}

	public class DownloadTableResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}
	
	public class GetMembersRequest : RdcRequest
	{
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
 				if(core.Database != null)
 				{
 					if(core.Synchronization == Synchronization.Synchronized)
 						return new GetMembersResponse { Members = core.Database.Members };
 					else
 						throw new RdcException(RdcError.NotSynchronized);
 				}
 				else
 					return new GetMembersResponse { Members = core.Members };
		}
	}

	public class GetMembersResponse : RdcResponse
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

	public class DelegateTransactionsRequest : RdcRequest
	{
//		public byte[]					Data {get; set;}
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				else
				{
					var acc = core.ProcessIncoming(Transactions);

					return new DelegateTransactionsResponse {Accepted = acc	.SelectMany(i => i.Operations)
																			.Select(i => new OperationAddress {Account = i.Signer, Id = i.Id})
																			.ToList()};
				}
		}
	}

	public class DelegateTransactionsResponse : RdcResponse
	{
		public IEnumerable<OperationAddress> Accepted { get; set; }
	}

	public class GetOperationStatusRequest : RdcRequest
	{
		public IEnumerable<OperationAddress>	Operations { get; set; }

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Chain)
					throw new RdcException(RdcError.NotChain);
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
	
				return	new GetOperationStatusResponse
						{
							Operations = Operations.Select(o => new {	A = o,
																		O = core.Transactions.Where(t => t.Signer == o.Account && t.Operations.Any(i => i.Id == o.Id))
																							 .SelectMany(t => t.Operations)
																							 .FirstOrDefault(i => i.Id == o.Id)
																		?? 
																		core.Database.Accounts.FindLastOperation(o.Account, i => i.Id == o.Id)})
													.Select(i => new GetOperationStatusResponse.Item {	Account		= i.A.Account,
																										Id			= i.A.Id,
																										Placing		= i.O == null ? PlacingStage.FailedOrNotFound : i.O.Placing}).ToArray()
						};
			}
		}
	}

	public class GetOperationStatusResponse : RdcResponse
	{
		public class Item
		{
			public Account			Account { get; set; }
			public int				Id { get; set; }
			public PlacingStage		Placing { get; set; }
		}

		public IEnumerable<Item> Operations { get; set; }
	}

	public class AccountInfoRequest : RdcRequest
	{
		public bool			Confirmed {get; set;}
		public Account		Account {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				else
				{
					var ai = core.Database.GetAccountInfo(Account, Confirmed);

					if(ai == null)
						throw new RdcException(RdcError.AccountNotFound);

 					return new AccountInfoResponse{Info = ai};
				}
		}
	}

	public class AccountInfoResponse : RdcResponse
	{
		public AccountInfo Info {get; set;}
	}

	public class AuthorInfoRequest : RdcRequest
	{
		public bool				Confirmed {get; set;}
		public string			Name {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				else
 					return new AuthorInfoResponse{Xon = core.Database.GetAuthorInfo(Name, Confirmed, new XonTypedBinaryValueSerializator())};
		}
	}

	public class AuthorInfoResponse : RdcResponse
	{
		public XonDocument Xon {get; set;}
	}

	public class QueryReleaseRequest : RdcRequest
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
 				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				else
 					return new QueryReleaseResponse {Releases = Queries.Select(i => core.Database.QueryRelease(i, Confirmed))};
		}
	}
	
	public class QueryReleaseResponse : RdcResponse
	{
		public IEnumerable<QueryReleaseResult> Releases { get; set; }
	}

	public class QueryReleaseResult
	{
		public ReleaseRegistration				Registration { get; set; }
		//public IEnumerable<ReleaseAddress>	Dependencies { get; set; }
	}

	public class DeclareReleaseRequest : RdcRequest
	{
		public PackageAddressPack Packages { get; set; }

		public override RdcResponse Execute(Core core)
		{
			core.Hub.Add(Peer.IP, Packages.Items);

			return new DeclareReleaseResponse();
		}
	}
	
	public class DeclareReleaseResponse : RdcResponse
	{
	}

	public class LocateReleaseRequest : RdcRequest
	{
		public ReleaseAddress	Release { get; set; }
		public int				Count { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Hub == null)
			{
				throw new RdcException(RdcError.NotHub);
			}

			return new LocateReleaseResponse {Seeders = core.Hub.Locate(this)}; 
		}
	}
	
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}

	public class ManifestRequest : RdcRequest
	{
		public IEnumerable<ReleaseAddress>	Releases { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Filebase == null)
			{
				throw new RdcException(RdcError.NotSeed);
			}

			return new ManifestResponse{Manifests = Releases.Select(i => core.Filebase.FindRelease(i)?.Manifest).ToArray()};
		}
	}
	
	public class ManifestResponse : RdcResponse
	{
		public IEnumerable<Manifest> Manifests { get; set; }
	}

	public class DownloadReleaseRequest : RdcRequest
	{
		public ReleaseAddress		Package { get; set; }
		public Distributive			Distributive { get; set; }
		public long					Offset { get; set; }
		public long					Length { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Filebase == null)
			{
				throw new RdcException(RdcError.NotSeed);
			}

			return new DownloadReleaseResponse{Data = core.Filebase.ReadPackage(Package, Distributive, Offset, Length)};
		}
	}

	public class DownloadReleaseResponse : RdcResponse
	{
		public byte[] Data { get; set; }
	}

	public class ReleaseHistoryRequest : RdcRequest
	{
		public RealizationAddress	Realization { get; set; }
		public bool					Confirmed { get; set; }

		public override RdcResponse Execute(Core core)
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
				throw new RdcException(RdcError.ProductNotFound);
		}
	}
	
	public class ReleaseHistoryResponse : RdcResponse
	{
		public IEnumerable<ReleaseRegistration> Releases { get; set; }
	}
}
