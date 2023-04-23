using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using NativeImport;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using static Uccs.Net.Download;

namespace Uccs.Net
{
	public enum Rdc : byte
	{
		Null, Time, BlocksPieces, DownloadRounds, GetMembers, NextRound, LastOperation, SendTransactions, GetOperationStatus, Author, Account, 
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

 		public abstract Rp						Request<Rp>(RdcRequest rq) where Rp : class;
 
		public TimeResponse						GetTime() => Request<TimeResponse>(new TimeRequest());
		public StampResponse					GetStamp() => Request<StampResponse>(new StampRequest());
		public TableStampResponse				GetTableStamp(Tables table, byte[] superclusters) => Request<TableStampResponse>(new TableStampRequest() {Table = table, SuperClusters = superclusters});
		public DownloadTableResponse			DownloadTable(Tables table, ushort cluster, long offset, long length) => Request<DownloadTableResponse>(new DownloadTableRequest{Table = table, ClusterId = cluster, Offset = offset, Length = length});
		public NextRoundResponse				GetNextRound() => Request<NextRoundResponse>(new NextRoundRequest());
		public SendTransactionsResponse			SendTransactions(IEnumerable<Transaction> transactions) => Request<SendTransactionsResponse>(new SendTransactionsRequest{Transactions = transactions});
		public GetOperationStatusResponse		GetOperationStatus(IEnumerable<OperationAddress> operations) => Request<GetOperationStatusResponse>(new GetOperationStatusRequest{Operations = operations});
		public GetMembersResponse				GetMembers() => Request<GetMembersResponse>(new GetMembersRequest());
		public AuthorResponse					GetAuthorInfo(string author) => Request<AuthorResponse>(new AuthorRequest{Name = author});
		public AccountResponse					GetAccountInfo(AccountAddress account, bool confirmed) => Request<AccountResponse>(new AccountRequest{Account = account});
		public QueryReleaseResponse				QueryRelease(IEnumerable<ReleaseQuery> query, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = query, Confirmed = confirmed });
		public QueryReleaseResponse				QueryRelease(RealizationAddress realization, Version version, VersionQuery versionquery, string channel, bool confirmed) => Request<QueryReleaseResponse>(new QueryReleaseRequest{ Queries = new [] {new ReleaseQuery(realization, version, versionquery, channel)}, Confirmed = confirmed });
		public LocateReleaseResponse			LocateRelease(ReleaseAddress package, int count) => Request<LocateReleaseResponse>(new LocateReleaseRequest{Release = package, Count = count});
		public DeclareReleaseResponse			DeclareRelease(Dictionary<ReleaseAddress, Distributive> packages) => Request<DeclareReleaseResponse>(new DeclareReleaseRequest{Packages = new PackageAddressPack(packages)});
		public ManifestResponse					GetManifest(ReleaseAddress release) => Request<ManifestResponse>(new ManifestRequest{Release = release});
		public DownloadReleaseResponse			DownloadRelease(ReleaseAddress release, Distributive distributive, long offset, long length) => Request<DownloadReleaseResponse>(new DownloadReleaseRequest{Package = release, Distributive = distributive, Offset = offset, Length = length});
		public ReleaseHistoryResponse			GetReleaseHistory(RealizationAddress realization, bool confirmed) => Request<ReleaseHistoryResponse>(new ReleaseHistoryRequest{Realization = realization, Confirmed = confirmed});
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
		public RdcError			Error { get; set; }
		public override byte	TypeCode => (byte)Type;
		public Rdc				Type => Enum.Parse<Rdc>(GetType().Name.Remove(GetType().Name.IndexOf("Response")));

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

	public class TimeRequest : RdcRequest
	{
		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)							throw new RdcException(RdcError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);
				
				return new TimeResponse {Time = core.Database.LastConfirmedRound.Time};
			}
		}
	}

	public class TimeResponse : RdcResponse
	{
		public ChainTime Time { get; set; }
	}

	public class BlocksPiecesRequest : RdcRequest
	{
		public IEnumerable<BlockPiece>	Pieces { get; set; }
		public override bool			WaitResponse => false;

		public override RdcResponse Execute(Core core)
		{
			var accepted = new List<BlockPiece>();

			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)
					throw new RdcException(RdcError.NotBase);

				if(core.Synchronization == Synchronization.Null || core.Synchronization == Synchronization.Downloading || core.Synchronization == Synchronization.Synchronizing)
				{
 					var min = core.SyncBlockCache.Any() ? core.SyncBlockCache.Max(i => i.Key) - Database.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
					foreach(var i in Pieces)
					{
						if(i.RoundId < min || (core.SyncBlockCache.ContainsKey(i.RoundId) && core.SyncBlockCache[i.RoundId].Contains(i)))
						{
							continue;
						}

						List<BlockPiece> l;
						
						if(!core.SyncBlockCache.TryGetValue(i.RoundId, out l))
						{
							l = core.SyncBlockCache[i.RoundId] = new();
						}

						l.Add(i);
	 					accepted.Add(i);
 					}
					
					foreach(var i in core.SyncBlockCache.Keys)
					{
						if(i < min)
						{
							core.SyncBlockCache.Remove(i);
						}
					}					
				}
				else if(core.Synchronization == Synchronization.Synchronized)
				{
					//var notolder = core.Database.LastConfirmedRound.Id - Database.Pitch;
					//var notnewer = core.Database.LastConfirmedRound.Id + Database.Pitch * 2;

					var d = core.Database;

					var good = Pieces.Where(p => { 


													if(p.Type == BlockType.JoinMembersRequest)
													{
														for(int i = p.RoundId; i > p.RoundId - Database.Pitch * 2; i--) /// not more than 1 request per [2 x Pitch] rounds
															if(d.FindRound(i) is Round r && r.JoinRequests.Any(j => j.Generator == p.Generator))
																return false;
													}
													else
													{
														if(p.RoundId <= d.LastConfirmedRound.Id || d.LastConfirmedRound.Id + Database.Pitch * 2 < p.RoundId)
															return false;
													}

													return true;
												}).ToArray();

					foreach(var p in good)
					{
						var r = core.Database.GetRound(p.RoundId);
				
						var ep = r.BlockPieces.Find(i => i.Equals(p));

						if(ep == null)
						{
							accepted.Add(p);
							r.BlockPieces.Add(p);
				
							var ps = r.BlockPieces.Where(i => i.Generator == p.Generator && i.Try == p.Try).OrderBy(i => i.Index);
			
							if(ps.Count() == p.Total && ps.Zip(ps.Skip(1), (x, y) => x.Index + 1 == y.Index).All(x => x))
							{
								var s = new MemoryStream();
								var w = new BinaryWriter(s);
				
								foreach(var i in ps)
								{
									s.Write(i.Data);
								}
				
								s.Position = 0;
								var rd = new BinaryReader(s);
				
								var b = Block.FromType(core.Database, p.Type);
								b.Read(rd);

								if(b.Generator != p.Generator)
									continue;
				
								core.ProcessIncoming(new Block[] {b});
							}
						}
						else
							if(ep.Peers != null && !ep.Peers.Contains(Peer))
								ep.Broadcasted = true;
					}
				}

				if(accepted.Any())
				{
					foreach(var i in core.Connections.Where(i => i.BaseRank > 0 && i != Peer))
					{
						i.Request<object>(new BlocksPiecesRequest{Pieces = accepted});
					}
				}
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
				if(!core.Settings.Database.Base)			throw new RdcException(RdcError.NotBase);
				if(core.Database.LastNonEmptyRound == null)	throw new RdcException(RdcError.TooEearly);

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
		public AccountAddress Generator;

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
			{
				if(!core.Settings.Database.Base)							throw new RdcException(RdcError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);

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
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);
				if(core.Database.BaseState == null)							throw new RdcException(RdcError.TooEearly);

				var r = new StampResponse {	BaseState				= core.Database.BaseState,
											BaseHash				= core.Database.BaseHash,
											LastCommitedRoundHash	= core.Database.LastCommittedRound.Hash,
											FirstTailRound			= core.Database.Tail.Last().Id,
											LastTailRound			= core.Database.Tail.First().Id,
											Accounts				= core.Database.Accounts.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Authors					= core.Database.Authors.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Products				= core.Database.Products.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Platforms				= core.Database.Platforms.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray(),
											Releases				= core.Database.Releases.	SuperClusters.Select(i => new StampResponse.SuperCluster{Id = i.Key, Hash = i.Value}).ToArray()};
				return r;
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
		public IEnumerable<SuperCluster>	Platforms { get; set; }
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
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);
				if(core.Database.BaseState == null)							throw new RdcException(RdcError.TooEearly);

				switch(Table)
				{
					case Tables.Accounts	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Accounts		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Authors		: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Authors		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Products	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Products		.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
					case Tables.Platforms	: return new TableStampResponse{Clusters = SuperClusters.SelectMany(s => core.Database.Platforms	.Clusters.Where(c => c.Id>>8 == s).Select(i => new TableStampResponse.Cluster{Id = i.Id, Length = i.MainLength, Hash = i.Hash})).ToArray()};
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
				if(core.Database == null || !core.Database.Settings.Base)
					throw new RdcException(RdcError.NotBase);

				var m = Table switch
							  {
									Tables.Accounts		=> core.Database.Accounts.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Authors		=> core.Database.Authors.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Products		=> core.Database.Products.Clusters.Find(i => i.Id == ClusterId)?.Main,
									Tables.Platforms	=> core.Database.Platforms.Clusters.Find(i => i.Id == ClusterId)?.Main,
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
 						return new GetMembersResponse { Members = core.Database.LastConfirmedRound.Members };
 					else
 						throw new RdcException(RdcError.NotSynchronized);
 				}
 				else
 					return new GetMembersResponse {Members = core.Members};
		}
	}

	public class GetMembersResponse : RdcResponse
	{
		public IEnumerable<Member> Members {get; set;}
	}
	
	public class SendTransactionsRequest : RdcRequest
	{
		public IEnumerable<Transaction>	Transactions {get; set;}

		public override RdcResponse Execute(Core core)
		{
			lock(core.Lock)
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
				else
				{
					var acc = core.ProcessIncoming(Transactions);

					return new SendTransactionsResponse {Accepted = acc.Select(i => i.Signature).ToList()};
				}
		}
	}

	public class SendTransactionsResponse : RdcResponse
	{
		public IEnumerable<byte[]> Accepted { get; set; }
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
			public AccountAddress	Account { get; set; }
			public int				Id { get; set; }
			public PlacingStage		Placing { get; set; }
		}

		public IEnumerable<Item> Operations { get; set; }
	}

	public class AccountRequest : RdcRequest
	{
		public AccountAddress		Account {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{
	 			if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);

				var ai = core.Database.Accounts.Find(Account, core.Database.LastConfirmedRound.Id);

				if(ai == null)
					throw new RdcException(RdcError.AccountNotFound);

 				return new AccountResponse{Account = ai};
			}
		}
	}

	public class AccountResponse : RdcResponse
	{
		public AccountEntry Account {get; set;}
	}

	public class AuthorRequest : RdcRequest
	{
		public string Name {get; set;}

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(!AuthorEntry.IsValid(Name))								throw new RdcException(RdcError.InvalidRequest);
				if(!core.Database.Settings.Base)							throw new RdcException(RdcError.NotBase);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);

				return new AuthorResponse{Entry = core.Database.Authors.Find(Name, core.Database.LastConfirmedRound.Id)};
			}
		}
	}

	public class AuthorResponse : RdcResponse
	{
		public AuthorEntry Entry {get; set;}
	}

	public class QueryReleaseRequest : RdcRequest
	{
		public IEnumerable<ReleaseQuery>	Queries { get; set; }
		public bool							Confirmed { get; set; }

		public override RdcResponse Execute(Core core)
		{
 			lock(core.Lock)
			{	
				if(core.Synchronization != Synchronization.Synchronized)
					throw new RdcException(RdcError.NotSynchronized);
 				
				return new QueryReleaseResponse {Releases = Queries.Select(i => core.Database.QueryRelease(i)).ToArray()};
			}
		}
	}
	
	public class QueryReleaseResponse : RdcResponse
	{
		public IEnumerable<Release> Releases { get; set; }
	}

	public class DeclareReleaseRequest : RdcRequest
	{
		public PackageAddressPack Packages { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Seedbase == null)
				throw new RdcException(RdcError.NotHub);

			core.Seedbase.Add(Peer.IP, Packages.Items);

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
			if(core.Seedbase == null)
				throw new RdcException(RdcError.NotHub);

			return new LocateReleaseResponse {Seeders = core.Seedbase.Locate(this)}; 
		}
	}
	
	public class LocateReleaseResponse : RdcResponse
	{
		public IEnumerable<IPAddress>	Seeders { get; set; }
	}

	public class ManifestRequest : RdcRequest
	{
		public ReleaseAddress	Release { get; set; }

		public override RdcResponse Execute(Core core)
		{
			if(core.Filebase == null)
				throw new RdcException(RdcError.NotSeed);

			return new ManifestResponse{Manifest = core.Filebase.FindRelease(Release)?.Manifest};
		}
	}
	
	public class ManifestResponse : RdcResponse
	{
		public Manifest Manifest { get; set; }
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
 			lock(core.Lock)
			{
				if(!core.Database.Settings.Chain)							throw new RdcException(RdcError.NotChain);
				if(core.Synchronization != Synchronization.Synchronized)	throw new RdcException(RdcError.NotSynchronized);

				var db = core.Database;
				
				return new ReleaseHistoryResponse {Releases = db.Releases.Where(Realization.Product.Author, 
																				Realization.Product.Name, 
																				i => i.Address.Platform == Realization.Platform, 
																				db.LastConfirmedRound.Id).ToArray()};
			}
		}
	}
	
	public class ReleaseHistoryResponse : RdcResponse
	{
		public IEnumerable<Release> Releases { get; set; }
	}
}
