using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Nethereum.Web3;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void BlockDelegate(Block b);

	public class Database
	{
		//public ZoneGenesis					Genesis => Genesises.Find(i => i.Zone == Zone.Current && i.Crypto.GetType() == Cryptography.Current.GetType());
		public const int					Pitch = 8;
		public const int					AliveMinGeneratorVotes = 6;
		public const int					AliveMinHubVotes = 1;
		public const int					AliveMinAnalyzerVotes = 1;
		public const int					LastGenesisRound = Pitch * 3;
		public const int					GeneratorsMax = 1024;
		public const int					HubsMax = 256 * 16;
		public const int					AnalyzersMax = 32;
		public const int					NewMembersPerRoundMax = 1;
		public const int					MembersRotation = 32;
		public  int							TailLength => Dev != null && Dev.TailLength100 ? 100 : 1000;
		const int							LoadedRoundsMax = 1000;
		public static readonly Coin			BailMin = 1000;
		public static readonly Coin			FeePerByte = new Coin(0.000001);

		public Zone							Zone;
		public DatabaseSettings				Settings;
		DevSettings							Dev;
		public Role							Roles;

		public List<Round>					Tail = new();
		public Dictionary<int, Round>		LoadedRounds = new();
		//public List<Member>					Members	= new();
		//public List<Account>				Funds = new();

		public RocksDb						Engine;
		public byte[]						BaseState;
		static readonly byte[]				BaseStateKey = new byte[] {0x01};
		//public byte[]						__BaseStateHash;
		static readonly byte[]				__BaseHashKey = new byte[] {0x02};
		public byte[]						BaseHash;
		static readonly byte[]				ChainStateKey = new byte[] {0x03};
		static readonly byte[]				GenesisKey = new byte[] {0x04};
		public AccountTable					Accounts;
		public AuthorTable					Authors;
		public ProductTable					Products;
		public RealizationTable				Realizations;
		public ReleaseTable					Releases;
		public int							Size => BaseState == null ? 0 : (BaseState.Length + 
																			Accounts.Clusters.Sum(i => i.MainLength) +
																			Authors.Clusters.Sum(i => i.MainLength) +
																			Products.Clusters.Sum(i => i.MainLength) +
																			Realizations.Clusters.Sum(i => i.MainLength) +
																			Releases.Clusters.Sum(i => i.MainLength));
		public Log							Log;
		public BlockDelegate				BlockAdded;

		public Round						LastConfirmedRound;
		public Round						LastCommittedRound;
		public Round						LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Blocks.Any()) ?? LastConfirmedRound;
		public Round						LastPayloadRound	=> Tail.FirstOrDefault(i => i.Votes.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;

		public const string					ChainFamilyName = "Chain";
		public ColumnFamilyHandle			ChainFamily	=> Engine.GetColumnFamily(ChainFamilyName);

		public static int					GetValidityPeriod(int rid) => rid + Pitch;

		public Database(Zone zone, Role roles, DatabaseSettings settings, DevSettings dev, Log log, RocksDb engine)
		{
			Roles = roles&(Role.Base|Role.Chain);
			Zone = zone;
			Settings = settings;
			Dev = dev;
			Log = log;
			Engine = engine;

			Accounts = new (this);
			Authors = new (this);
			Products = new (this);
			Realizations = new (this);
			Releases = new (this);

			BaseHash = Zone.Cryptography.ZeroHash;
			BaseState = Engine.Get(BaseStateKey);

			if(BaseState != null)
			{
				var r = new BinaryReader(new MemoryStream(BaseState));
		
				LastCommittedRound				= new Round(this){Id = r.Read7BitEncodedInt()};
				LastCommittedRound.Hash			= r.ReadSha3();
				LastCommittedRound.ConfirmedTime= r.ReadTime();
				LastCommittedRound.WeiSpent		= r.ReadBigInteger();
				LastCommittedRound.Factor		= r.ReadCoin();
				LastCommittedRound.Emission		= r.ReadCoin();
				LastCommittedRound.Generators	= r.Read<Generator>(m => m.ReadForBase(r)).ToList();
				LastCommittedRound.Hubs			= r.Read<Hub>(m => m.ReadForBase(r)).ToList();
				LastCommittedRound.Analyzers	= r.Read<Analyzer>(m => m.ReadForBase(r)).ToList();
				LastCommittedRound.Funds		= r.ReadList<AccountAddress>();

				LoadedRounds.Add(LastCommittedRound.Id, LastCommittedRound);

				Hashify();

				if(!BaseHash.SequenceEqual(Engine.Get(__BaseHashKey)))
				{
					throw new IntegrityException("");
				}
			}

			if(Roles.HasFlag(Role.Chain))
			{
				var chainstate = Engine.Get(ChainStateKey);

				if(chainstate == null || !Engine.Get(GenesisKey).SequenceEqual(Zone.Genesis.HexToByteArray()))
				{
					Tail.Clear();
	
 					var rd = new BinaryReader(new MemoryStream(Zone.Genesis.HexToByteArray()));
						
					for(int i = 0; i <= 24; i++)
					{
						var r = new Round(this);
						r.Read(rd);
						r.Voted = true;
		
						Tail.Insert(0, r);
						
						if(i == 16)
						{
							r.ConfirmedGeneratorJoiners.Add(Zone.Father0);
							r.ConfirmedFundJoiners.Add(Zone.OrgAccount);
						}
	
						//foreach(var p in r.Payloads)
						//	p.Confirmed = true;
	
						if(r.Id > 0)
							r.ConfirmedTime = CalculateTime(r, r.Unique);
	
						if(i <= 16)
						{
							r.ConfirmedPayloads = r.Payloads.ToList();

							Confirm(r, true);

							if(i == 16)
							{
								r.Generators[0].IPs = new IPAddress[] {};
							}
						}
					}
	
					if(Tail.Any(i => i.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null)))))
					{
						throw new IntegrityException("Genesis construction failed");
					}
				
					Engine.Put(GenesisKey, Zone.Genesis.HexToByteArray());
				}
				else
				{
					var rd = new BinaryReader(new MemoryStream(chainstate));

					var lcr = FindRound(rd.Read7BitEncodedInt());
					
					for(int i = lcr.Id - lcr.Id % TailLength; i <= lcr.Id; i++)
					{
						var r = FindRound(i);

						Tail.Insert(0, r);
		
						r.Confirmed = false;
						Confirm(r, true);
					}
				}
			}
		}

		public string CreateGenesis(AccountKey gen, AccountKey org, AccountKey[] fathers)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
				r.ConfirmedPayloads = r.Payloads.ToList();
				r.Hashify(r.Id > 0 ? FindRound(rid - 1).Hash : Zone.Cryptography.ZeroHash);
				r.Write(w);
			}
	
			var b0 = new Vote(this)	{
										RoundId		= 0,
										TimeDelta	= 1,
										ParentSummary	= Zone.Cryptography.ZeroHash,
									};

			var t = new Transaction(Zone);
			t.AddOperation(new Emission(Zone.OrgAccount, Web3.Convert.ToWei(512_000, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
			t.AddOperation(new AuthorBid(Zone.OrgAccount, "uo", 1){ Id = 1 });
			t.Sign(org, gen, 0);
			b0.AddNext(t);
						
			void emmit(Dictionary<AccountAddress, AccountEntry> accs)
			{
				foreach(var f in fathers.OrderBy(j => j))
				{
					var t = new Transaction(Zone);
					t.AddOperation(new Emission(f, Web3.Convert.ToWei(1000, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
									
					if(accs != null)
					{
						t.AddOperation(new CandidacyDeclaration(f, accs[f].Balance - 1){ Id = 1 });
					}

					t.Sign(f, gen, 0);
	
					b0.AddNext(t);
				}

				b0.Sign(gen);
				Add(b0);
			}
						
			emmit(null);

			Execute(Tail.First(), Tail.First().Payloads, null);
			var accs = Tail.First().AffectedAccounts;
			Tail.Clear();
			b0.Transactions.Clear();
			b0.AddNext(t);

			emmit(accs);

			b0.FundJoiners.Add(Zone.OrgAccount);
	
			write(0);
			
			/// UO Autor

			var b1 = new Vote(this)	{
										RoundId		= 1,
										TimeDelta	= ((long)TimeSpan.FromDays(365).TotalMilliseconds + 1),  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
										ParentSummary	= Zone.Cryptography.ZeroHash,
									};
	
			t = new Transaction(Zone);
			t.AddOperation(new AuthorRegistration(org, "uo", "UO", 255){Id = 2});
			t.Sign(org, gen, 1);
			b1.AddNext(t);
			
			b1.Sign(gen);
			Add(b1);
			write(1);
	
			for(int i = 2; i <= 24; i++)
			{
				var b = new Vote(this)
						{
							RoundId		= i,
							TimeDelta	= 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
							ParentSummary	= i < 8 ? Zone.Cryptography.ZeroHash : Summarize(GetRound(i - Pitch))
						};
		 
				if(i == 16)
					b.GeneratorJoiners.Add(Zone.Father0);
	
				b.Sign(gen);
				Add(b);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		public void Add(Block b)
		{
			var r = GetRound(b.RoundId);

			b.Round = r;

			r.Blocks.Add(b);
			//r.Blocks = r.Blocks.OrderBy(i => i is Payload p ? p.OrderingKey : new byte[] {}, new BytesComparer()).ToList();
				
			if(b is Vote p && p.Transactions.Any())
			{
				foreach(var t in p.Transactions)
					foreach(var o in t.Operations)
						o.Placing = PlacingStage.Placed;

				//if(execute)
	 			//	for(int i = r.Id; i <= LastPayloadRound.Id; i++)
	 			//	{
	 			//		var ir = GetRound(i);
	 			//			
	 			//		if(ir.Payloads.Any() && ir.Previous != null)
				//		{
				//			ir.Time = CalculateTime(ir, ir.Unique);
				//			Execute(ir, ir.Payloads, null);
				//		}
	 			//		else
	 			//			break;
	 			//	}
			}
	
			if(r.FirstArrivalTime == DateTime.MaxValue)
			{
				r.FirstArrivalTime = DateTime.UtcNow;
			} 


// 			if(b is MembersJoinRequest jr)
// 			{
// 				//jr.Bail = Accounts.Find(jr.Generator, b.RoundId - Pitch).Bail;
// 				JoinRequests.Add(jr);
// 			}
	
			BlockAdded?.Invoke(b);
		}

		public void Add(IEnumerable<Block> bb)
		{
			foreach(var i in bb)
			{
				Add(i);
			}
		}

		public Round GetRound(int rid)
		{
			var r = FindRound(rid);

			if(r == null)
			{	
				r = new Round(this) {Id = rid};
				//r.LastAccessed = DateTime.UtcNow;
				Tail.Add(r);
				Tail = Tail.OrderByDescending(i => i.Id).ToList();
			}

			return r;
		}

		public Round FindRound(int rid)
		{
			foreach(var i in Tail)
				if(i.Id == rid)
				{
					//i.LastAccessed = DateTime.UtcNow;
					return i;
				}

			if(LoadedRounds.ContainsKey(rid))
			{
				var r = LoadedRounds[rid];
				//r.LastAccessed = DateTime.UtcNow;
				return r;
			}

			var d = Engine.Get(BitConverter.GetBytes(rid), ChainFamily);

			if(d != null)
			{
				var r = new Round(this);
				r.Id			= rid; 
				r.Voted			= true; 
				r.Confirmed		= true;
				//r.LastAccessed	= DateTime.UtcNow;

				r.Load(new BinaryReader(new MemoryStream(d)));
	
				LoadedRounds[r.Id] = r;
				//Recycle();
				
				return r;
			}
			else
				return null;
		}

		void Recycle()
		{
			if(LoadedRounds.Count > TailLength * 10)
			{
				foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.Id).Skip(TailLength * 10))
				{
					LoadedRounds.Remove(i.Key);
				}
			}
		}

		public List<Generator> GeneratorsOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Generators/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public List<Hub> HubsOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Hubs/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public List<Analyzer> AnalyzersOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Analyzers/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool QuorumReached(Round r)
		{
			var m = GeneratorsOf(r.Id);
			
			var q = m.Count() * 2 / 3;

			if(m.Count() * 2 % 3 != 0)
				q++;

			if(r.Unique.Count() < q)
				return false;

			var v = r.Unique.Where(i => m.Any(j => j.Account == i.Generator));
				
			if(v.Count() < q)
				return false;

			var n =	v.GroupBy(i => i.ParentSummary, new BytesEqualityComparer()).Max(i => i.Count());

			return q <= n;
		}

		public bool QuorumFailed(Round r)
		{
			var m = GeneratorsOf(r.Id);

			var v = r.Unique.Where(i => m.Any(j => j.Account == i.Generator));
			
			var d = m.Count - v.Count();

			var q = m.Count * 2 / 3;

			if(m.Count * 2 % 3 != 0)
				q++;

			return v.Any() && v.GroupBy(i => i.ParentSummary, new BytesEqualityComparer()).All(i => i.Count() + d < q);
		}

		public ChainTime CalculateTime(Round round, IEnumerable<Vote> votes)
		{
 			if(round.Id == 0)
 			{
 				return round.ConfirmedTime;
 			}

 			if(!votes.Any())
 			{
				return round.Previous.ConfirmedTime + new ChainTime(1);
			}

			///var t = 0L;
			///var n = 0;
			///
			///for(int i = Math.Max(0, round.Id - Pitch + 1); i <= round.Id; i++)
			///{
			///	var r = FindRound(i);
			///	t += r.Payloads.Sum(i => i.Time.Ticks);
			///	n += r.Payloads.Count();
			///}
			///
			///t /= n;

			votes = votes.OrderBy(i => i.Generator);

			return round.Previous.ConfirmedTime + new ChainTime(votes.Sum(i => i.TimeDelta)/votes.Count());
		}

		public IEnumerable<Transaction> CollectValidTransactions(IEnumerable<Transaction> txs, Round round)
		{
			//txs = txs.Where(i => round.Id <= i.RoundMax /*&& IsSequential(i, round.Id)*/);

			if(txs.Any())
			{
// 				var p = new Payload(this);
// 				p.Member	= Account.Zero;
// 				p.Time		= DateTime.UtcNow;
// 				p.Round		= round;
// 				p.TimeDelta	= 1;
// 					
// 				foreach(var i in txs)
// 				{
// 					p.AddNext(i);
// 				}
// 				
//  				Execute(round, new Payload[] {p}, null);
 	
 //				txs = txs.Where(t => t.SuccessfulOperations.Any());
			}

			return txs;
		}

		///public bool IsSequential(Operation transaction, int ridmax)
		///{
		///	var prev = Accounts.FindLastOperation(transaction.Signer, o => o.Successful, t => t.Successful, null, r => r.Id < ridmax);
		///
		///	if(transaction.Id == 0 && prev == null)
		///		return true;
		///
		///	if(transaction.Id == 0 && prev != null || transaction.Id != 0 && prev != null && prev.Id != transaction.Id - 1)
		///		return false;
		///
		///	/// STRICT: return prev != null && (prev.Payload.Confirmed || prev.Payload.Transactions.All(i => IsSequential(i, i.Payload.RoundId))); /// All transactions in a block containing 'prev' one must also be sequential
		///	return prev.Transaction.Payload.Confirmed || IsSequential(prev, prev.Transaction.Payload.RoundId);
		///}

		public IEnumerable<AccountAddress> ProposeGeneratorJoiners(Round round)
		{
			var o = round.Parent.GeneratorJoinRequests.Select(jr =>	{
																		var a = Accounts.Find(jr.Generator, LastConfirmedRound.Id);
																		return new {jr = jr, a = a};
																	})	/// round.ParentId - Pitch means to not join earlier than [Pitch] after declaration, and not redeclare after a join is requested
													.Where(i => i.a != null && 
																i.a.CandidacyDeclarationRid <= round.Id - Pitch * 2 &&  /// 2 = declared, requested
																i.a.Bail >= (Dev != null && Dev.DisableBailMin ? 0 : BailMin))
													.OrderByDescending(i => i.a.Bail)
													.ThenBy(i => i.a.Address)
													.Select(i => i.jr);

			var n = Math.Min(GeneratorsMax - GeneratorsOf(round.Id).Count(), o.Count());

			return o.Take(n).Select(i => i.Generator);
		}

		public IEnumerable<AccountAddress> ProposeGeneratorLeavers(Round round, AccountAddress generator)
		{
//			var joiners = ProposeGeneratorJoiners(round);

			var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));

			var leavers = GeneratorsOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
															prevs.Count(r => r.Votes.Any(b => b.Generator == i.Account)) < AliveMinGeneratorVotes &&	/// sent less than MinVotesPerPitch of required blocks
															!prevs.Any(r => r.Votes.Any(v => v.Generator == generator && v.GeneratorLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
												.Select(i => i.Account);
			return leavers;
		}

		public IEnumerable<AccountAddress> ProposeHubJoiners(Round round)
		{
			var o = round.Parent.HubJoinRequests.OrderBy(i => i.Account).Select(i => i.Account);
		
			var n = Math.Min(HubsMax - HubsOf(round.Id).Count(), o.Count());
		
			return o.Take(n);
		}
		
		public IEnumerable<AccountAddress> ProposeHubLeavers(Round round, AccountAddress generator)
		{
			var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));
		
			return HubsOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
												prevs.Count(r => r.HubVoxes.Any(b => b.Account == i.Account)) < AliveMinHubVotes &&	/// sent less than MinVotesPerPitch of required blocks
												!prevs.Any(r => r.Votes.Any(v => v.Generator == generator && v.HubLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
									.Select(i => i.Account);
		}

		public IEnumerable<AccountAddress> ProposeAnalyzerJoiners(Round round)
		{
			var o = round.Parent.AnalyzerJoinRequests.OrderBy(i => i.Account).Select(i => i.Account);
		
			var n = Math.Min(AnalyzersMax - AnalyzersOf(round.Id).Count(), o.Count());
		
			return o.Take(n);
		}
		
		public IEnumerable<AccountAddress> ProposeAnalyzerLeavers(Round round, AccountAddress generator)
		{
			var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));
		
			return AnalyzersOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
													prevs.Count(r => r.AnalyzerVoxes.Any(b => b.Account == i.Account)) < AliveMinAnalyzerVotes &&	/// sent less than MinVotesPerPitch of required blocks
													!prevs.Any(r => r.Votes.Any(v => v.Generator == generator && v.AnalyzerLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
									.Select(i => i.Account);
		}

		public byte[] Summarize(Round round)
		{
			if(round.Id > LastGenesisRound && !round.Parent.Confirmed)
				return null;

			var m = round.Id > Pitch ? GeneratorsOf(round.Id) : new();
			var v = round.Unique.Where(i => m.Any(j => i.Generator == j.Account)).ToArray();

			var payloads = v.Where(i => i.Transactions.Any());

			round.ConfirmedTime = CalculateTime(round, v);
			
			Execute(round, payloads, round.Forkers);

			round.ConfirmedPayloads = payloads.Where(i => i.SuccessfulTransactions.Any()).OrderBy(i => i.OrderingKey, new BytesComparer()).ToList();

			if(round.Id < Pitch)
			{
				round.ConfirmedGeneratorJoiners	= new();
				round.ConfirmedGeneratorLeavers	= new();
				round.ConfirmedHubJoiners		= new();
				round.ConfirmedHubLeavers		= new();
				round.ConfirmedAnalyzerJoiners	= new();
				round.ConfirmedAnalyzerLeavers	= new();
				round.ConfirmedFundJoiners		= new();
				round.ConfirmedFundLeavers		= new();
				round.ConfirmedViolators		= new();
			}
			else
			{
				var q = m.Count * 2 / 3;
	
				round.ConfirmedGeneratorJoiners	= v.SelectMany(i => i.GeneratorJoiners).Distinct().Where(x => v.Count(b => b.GeneratorJoiners.Contains(x)) >= q)						.OrderBy(i => i).ToList();
				round.ConfirmedGeneratorLeavers	= v.SelectMany(i => i.GeneratorLeavers).Distinct().Where(x => v.Count(b => b.GeneratorLeavers.Contains(x)) >= q)						.OrderBy(i => i).ToList();
				round.ConfirmedHubJoiners		= v.SelectMany(i => i.HubJoiners).		Distinct().Where(x => v.Count(b => b.HubJoiners.Contains(x)) >= q)								.OrderBy(i => i).ToList();
				round.ConfirmedHubLeavers		= v.SelectMany(i => i.HubLeavers).		Distinct().Where(x => v.Count(b => b.HubLeavers.Contains(x)) >= q)								.OrderBy(i => i).ToList();
				round.ConfirmedAnalyzerJoiners	= v.SelectMany(i => i.AnalyzerJoiners).	Distinct().Where(x => v.Count(b => b.AnalyzerJoiners.Contains(x)) >= q)							.OrderBy(i => i).ToList();
				round.ConfirmedAnalyzerLeavers	= v.SelectMany(i => i.AnalyzerLeavers).	Distinct().Where(x => v.Count(b => b.AnalyzerLeavers.Contains(x)) >= q)							.OrderBy(i => i).ToList();
				round.ConfirmedFundJoiners		= v.SelectMany(i => i.FundJoiners).		Distinct().Where(x => v.Count(b => b.FundJoiners.Contains(x)) >= Database.GeneratorsMax * 2 / 3).OrderBy(i => i).ToList();
				round.ConfirmedFundLeavers		= v.SelectMany(i => i.FundLeavers).		Distinct().Where(x => v.Count(b => b.FundLeavers.Contains(x)) >= Database.GeneratorsMax * 2 / 3).OrderBy(i => i).ToList();
				round.ConfirmedViolators		= v.SelectMany(i => i.Violators).		Distinct().Where(x => v.Count(b => b.Violators.Contains(x)) >= q)								.OrderBy(i => i).ToList();
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			//w.Write(Id >= Database.Pitch ? Parent.Hash : Database.Zone.Cryptography.ZeroHash);
			w.Write(round.Id > 0 ? round.Previous.Hash : Zone.Cryptography.ZeroHash);
			
			round.WriteConfirmed(w);

			round.Summary = Zone.Cryptography.Hash(s.ToArray());

			return round.Summary;
		}

		public void Execute(Round round, IEnumerable<Vote> payloads, IEnumerable<AccountAddress> forkers)
		{
			var prev = round.Previous;
				
			if(round.Id != 0 && prev == null)
				return;

			foreach(var b in payloads)
				foreach(var t in b.Transactions)
					foreach(var o in t.Operations)
						o.Error = null;

			start: 

			round.Emission		= round.Id == 0 ? 0						: prev.Emission;
			round.WeiSpent		= round.Id == 0 ? 0						: prev.WeiSpent;
			round.Factor		= round.Id == 0 ? Emission.FactorStart	: prev.Factor;
			round.Generators	= round.Id == 0 ? new()					: prev.Generators.ToList();
			round.Hubs			= round.Id == 0 ? new()					: prev.Hubs.ToList();
			round.Analyzers		= round.Id == 0 ? new()					: prev.Analyzers.ToList();
			round.Funds			= round.Id == 0 ? new()					: prev.Funds.ToList();

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();
			round.AffectedProducts.Clear();
			round.AffectedPlatforms.Clear();
			round.AffectedReleases.Clear();

			foreach(var b in payloads.AsEnumerable().Reverse())
			{
				foreach(var t in b.Transactions.AsEnumerable().Reverse())
				{
					if(t.Operations.Any(i => i.Error != null))
						continue;

					Coin fee = 0;

					foreach(var o in t.Operations.AsEnumerable().Reverse())
					{
						//if(o.Error != null)
						//	continue;

						var s = round.AffectAccount(t.Signer);
					
						if(o.Id <= s.LastOperationId)
						{
							o.Error = Operation.NotSequential;
							goto start;
						}
						
						o.Execute(this, round);

						if(o.Error != null)
							goto start;

						var f = o.CalculateFee(round.Factor);
	
						if(s.Balance - f < 0)
						{
							o.Error = Operation.NotEnoughUNT;
							goto start;
						}

						fee += f;
						s.Balance -= f;
						s.LastOperationId = o.Id;
					}
						
					if(t.SuccessfulOperations.Count() == t.Operations.Count)
					{
						if(Roles.HasFlag(Role.Chain))
						{
							round.AffectAccount(t.Signer).Transactions.Add(round.Id);
						}

						round.Distribute(fee, new[]{b.Generator}, 9, round.Funds, 1); /// taking 10% we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding
					}
				}
			}

			if(round.Id > LastGenesisRound)
			{
				var penalty = Coin.Zero;

				if(forkers != null && forkers.Any())
				{
					foreach(var f in forkers)
					{
						penalty += round.AffectAccount(f).Bail;
						round.AffectAccount(f).BailStatus = BailStatus.Siezed;
					}

					round.Distribute(penalty, round.Generators.Where(i => !forkers.Contains(i.Account)).Select(i => i.Account), 1, round.Funds, 1);
				}
			}
		}

		public void Hashify()
		{
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var i in Accounts.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Authors.SuperClusters.OrderBy(i => i.Key))			BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Products.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Realizations.SuperClusters.OrderBy(i => i.Key))	BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Releases.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}

		public void Confirm(Round round, bool confirmed)
		{
			if(round.Id > 0 && LastConfirmedRound != null && LastConfirmedRound.Id + 1 != round.Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == round.Id");

			if(!confirmed)
			{
				if(round.Summary == null)
				{
					Summarize(round);
				}

				var c = FindRound(round.Id + Pitch);
				var cm = GeneratorsOf(c.Id);
				var s = c.Unique.Where(i => cm.Any(j => j.Account == i.Generator)).GroupBy(i => i.ParentSummary, new BytesEqualityComparer()).MaxBy(i => i.Count()).Key;
	 		
				if(!s.SequenceEqual(round.Summary))
				{
					throw new ConfirmationException("Can't confirm", round);
				}
			}
			else
			{
				round.Blocks = round.ConfirmedPayloads.Cast<Block>().ToList();
				Execute(round, round.ConfirmedPayloads, round.ConfirmedViolators);
			}
								
			foreach(var b in round.Payloads)
			{
				foreach(var t in b.Transactions)
				{	
					foreach(var o in t.Operations)
					{	
						o.Placing = (round.ConfirmedPayloads.Contains(b) && o.Error == null) ? PlacingStage.Confirmed : PlacingStage.FailedOrNotFound;
						
						#if DEBUG
						if(o.__ExpectedPlacing > PlacingStage.Placed && o.Placing != o.__ExpectedPlacing)
						{
							Debugger.Break();
						}
						#endif
					}

					t.Operations.RemoveAll(i => i.Error != null);
				}

				b.Transactions.RemoveAll(t => !t.Operations.Any());
			}
			
			round.Generators.RemoveAll(i => round.ConfirmedViolators.Contains(i.Account));

			round.Generators.AddRange(round.ConfirmedGeneratorJoiners.Where(i => Accounts.Find(i, round.Id).CandidacyDeclarationRid <= round.Id - Pitch * 2)
																	 .Select(i => new Generator {Account = i, JoinedAt = round.Id + Pitch + 1}));
			round.Generators.RemoveAll(i => round.AnyOperation(o => o is CandidacyDeclaration d && d.Signer == i.Account && o.Placing == PlacingStage.Confirmed));  /// CandidacyDeclaration cancels membership
			round.Generators.RemoveAll(i => round.AffectedAccounts.ContainsKey(i.Account) && round.AffectedAccounts[i.Account].Bail < (Dev.DisableBailMin ? 0 : BailMin));  /// if Bail has exhausted due to penalties (CURRENTY NOT APPLICABLE, penalties are disabled)
			round.Generators.RemoveAll(i => round.ConfirmedGeneratorLeavers.Contains(i.Account));

			round.Hubs.AddRange(round.ConfirmedHubJoiners.Select(i => new Hub {Account = i, JoinedAt = round.Id + Pitch + 1}));
			round.Hubs.RemoveAll(i => round.ConfirmedHubLeavers.Contains(i.Account));

			round.Analyzers.AddRange(round.ConfirmedAnalyzerJoiners.Select(i => new Analyzer {Account = i, JoinedAt = round.Id + Pitch + 1}));
			round.Analyzers.RemoveAll(i => round.ConfirmedAnalyzerLeavers.Contains(i.Account));
	
			round.Funds.AddRange(round.ConfirmedFundJoiners);
			round.Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));

			using(var b = new WriteBatch())
			{
				if(Tail.Count(i => i.Id < round.Id) >= TailLength)
				{
					var tail = Tail.AsEnumerable().Reverse().Take(TailLength);
	
					foreach(var i in tail)
					{
						Accounts	.Save(b, i.AffectedAccounts.Values);
						Authors		.Save(b, i.AffectedAuthors.Values);
						Products	.Save(b, i.AffectedProducts.Values);
						Realizations.Save(b, i.AffectedPlatforms.Values);
						Releases	.Save(b, i.AffectedReleases.Values);
					}

					LastCommittedRound = tail.Last();
	
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
	
					w.Write7BitEncodedInt(LastCommittedRound.Id);
					w.Write(LastCommittedRound.Hash);
					w.Write(LastCommittedRound.ConfirmedTime);
					w.Write(LastCommittedRound.WeiSpent);
					w.Write(LastCommittedRound.Factor);
					w.Write(LastCommittedRound.Emission);
					w.Write(LastCommittedRound.Generators.OrderBy(i => i.Account), i => i.WriteForBase(w));
					w.Write(LastCommittedRound.Hubs.OrderBy(i => i.Account), i => i.WriteForBase(w));
					w.Write(LastCommittedRound.Analyzers.OrderBy(i => i.Account), i => i.WriteForBase(w));
					w.Write(LastCommittedRound.Funds.OrderBy(i => i));
	
					BaseState = s.ToArray();

					Hashify();
									
					b.Put(BaseStateKey, BaseState);
					b.Put(__BaseHashKey, BaseHash);

					foreach(var i in tail)
					{
						if(!LoadedRounds.ContainsKey(i.Id))
						{
							LoadedRounds.Add(i.Id, i);
						}
						
						Tail.Remove(i);
					}

					Recycle();
				}

				round.Hashify(round.Id > 0 ? round.Previous.Hash : Zone.Cryptography.ZeroHash); /// depends on BaseHash 

				if(Roles.HasFlag(Role.Chain))
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					w.Write7BitEncodedInt(round.Id);

					b.Put(ChainStateKey, s.ToArray());

					s = new MemoryStream();
					w = new BinaryWriter(s);
	
					round.Save(w);
	
					b.Put(BitConverter.GetBytes(round.Id), s.ToArray(), ChainFamily);
				}

				Engine.Write(b);
			}

			if(round.Id > Pitch)
			{
				var cjr = FindRound(round.Id - Pitch - 1);
				
				if(cjr != null)
				{
					cjr.Blocks.RemoveAll(i => i is not Vote v || !v.Transactions.Any());
				}
			}

			//round.JoinRequests.RemoveAll(i => i.RoundId < round.Id - Pitch);
		
			round.Confirmed = true;
			LastConfirmedRound = round;
		}
/*
		public ProductEntry FindProduct(ProductAddress product, int ridmax)
		{
			if(ridmax >= LastCommittedRound.Id)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedProducts.ContainsKey(product))
					return r.AffectedProducts[product];

			var e = Products.FindEntry(product);

			return e;
		}
*/
		public Transaction FindLastTailTransaction(Func<Transaction, bool> transaction_predicate, Func<Vote, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in b.Transactions)
						if(transaction_predicate == null || transaction_predicate(t))
							return t;

			return null;
		}

		public IEnumerable<Transaction> FindLastTailTransactions(Func<Transaction, bool> transaction_predicate, Func<Vote, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in transaction_predicate == null ? b.Transactions : b.Transactions.Where(transaction_predicate))
						yield return t;
		}

		public O FindLastTailOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Vote, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
		}

		IEnumerable<O> FindLastTailOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Vote, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops : ops.Where(op);
		}

		public Block FindLastBlock(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			for(int i = LastNonEmptyRound.Id; i >= maxrid; i--)
			{
				var r = FindRound(i);

				if(r != null)
				{
					foreach(var b in r.Blocks)
						if(f(b))
							return b;
				}
			}
			//foreach(var r in Rounds.Where(i => i.Id <= maxrid))
			//	foreach(var b in r.Blocks)
			//		if(f(b))
			//			return b;

			return null;
		}

		public IEnumerable<Block> FindLastBlocks(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			foreach(var r in Tail.Where(i => i.Id <= maxrid))
				foreach(var b in r.Blocks)
					if(f(b))
						yield return b;
		}


		public Release QueryRelease(ReleaseQuery query)
		{
			if(query.VersionQuery == VersionQuery.Latest)
			{
				var p = Products.Find(query.Realization.Product, LastConfirmedRound.Id);
	
				if(p != null)
				{
					//var r = p.Releases.Where(i => i.Realization == query.Platform && i.Channel == query.Channel).MaxBy(i => i.Version);
					//
					//if(r != null)
					//{
					//	var rr = FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	(RealizationAddress)m.Release == query.Realization && 
					//																		m.Release.Version == r.Version);
					//
					//	return new QueryReleaseResult{Registration = rr};
					//}

					return Releases.Where(	query.Realization.Product.Author, 
											query.Realization.Product.Name, 
											i => i.Address.Realization.Name == query.Realization.Name, 
											LastConfirmedRound.Id)
									.MaxBy(i => i.Address.Version);

				}

				return null;
			}

			if(query.VersionQuery == VersionQuery.Exact)
			{
				var p = Products.Find(query.Realization.Product, LastConfirmedRound.Id);
	
				if(p != null)
				{
					//var r = p.Releases.Where(i => i.Realization == query.Platform && i.Channel == query.Channel).MaxBy(i => i.Version);
					//
					//if(r != null)
					//{
					//	var rr = FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	(RealizationAddress)m.Release == query.Realization && 
					//																		m.Release.Version == r.Version);
					//
					//	return new QueryReleaseResult{Registration = rr};
					//}

					var r = query.Realization;

					return Releases.Find(new ReleaseAddress(r.Product, r.Name, query.Version), LastConfirmedRound.Id);
				}

				return null;
			}

			throw new ArgumentException("Unsupported VersionQuery");
		}
	}
}
