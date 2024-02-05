using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Util;
using Nethereum.Web3;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void BlockDelegate(Vote b);
	public delegate void ConsensusDelegate(Round b, bool reached);
	public delegate void RoundDelegate(Round b);

	public class Mcv /// Mutual chain voting
	{
		public Log							Log;

		public const int					P = 8; /// pitch
		public const int					DeclareToGenerateDelay = P*2;
		public const int					TransactionPlacingLifetime = P*2;
		public const int					LastGenesisRound = 1+P + 1+P + P;
		///public const int					MembersRotation = 32;
		public static readonly Money		ResourceDataPerByteFee	= new Money(0.001);
		public static readonly Money		EntityAllocationFee		= new Money(0.1);
		public static readonly Money		AnalysisPerByteFee		= new Money(0.000_000_001);
		public static readonly Money		BalanceMin				= new Money(0.000_000_001);
		//public const int					EntityAllocation = 1000;
		public const int					EntityAllocationYearsMin = 1;
		public const int					EntityAllocationYearsMax = 32;


		public Zone							Zone;
		public McvSettings					Settings;
		public Role							Roles;

		public List<Round>					Tail = new();
		public Dictionary<int, Round>		LoadedRounds = new();

		public RocksDb						Database;
		public byte[]						BaseState;
		public byte[]						BaseHash;
		static readonly byte[]				BaseStateKey = new byte[] {0x01};
		static readonly byte[]				__BaseHashKey = new byte[] {0x02};
		static readonly byte[]				ChainStateKey = new byte[] {0x03};
		static readonly byte[]				GenesisKey = new byte[] {0x04};
		public AccountTable					Accounts;
		public AuthorTable					Authors;
		public ReleaseTable				Releases;
		public int							Size => BaseState == null ? 0 : (BaseState.Length + 
																			Accounts.Clusters.Sum(i => i.MainLength) +
																			Authors.Clusters.Sum(i => i.MainLength) +
																			Releases.Clusters.Sum(i => i.MainLength));
		public BlockDelegate				VoteAdded;
		public ConsensusDelegate			ConsensusConcluded;
		public RoundDelegate				Commited;

		public Round						LastConfirmedRound;
		public Round						LastCommittedRound;
		public Round						LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
		public Round						LastPayloadRound	=> Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;

		public const string					ChainFamilyName = "Chain";
		public ColumnFamilyHandle			ChainFamily	=> Database.GetColumnFamily(ChainFamilyName);

		public static int					GetValidityPeriod(int rid) => rid + P;

		public Mcv(Zone zone, Role roles, McvSettings settings, string databasepath)
		{
			Roles = roles & (Role.Base|Role.Chain);
			Zone = zone;
			Settings = settings;

			var dbo	= new DbOptions().SetCreateIfMissing(true)
									 .SetCreateMissingColumnFamilies(true);

			var cfs = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{	new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (AuthorTable.MetaColumnName,	new ()),
																new (AuthorTable.MainColumnName,	new ()),
																new (AuthorTable.MoreColumnName,	new ()),
																new (ReleaseTable.MetaColumnName,	new ()),
																new (ReleaseTable.MainColumnName,	new ()),
																new (ReleaseTable.MoreColumnName,	new ()),
																new (Mcv.ChainFamilyName,			new ())})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Authors = new (this);
			Releases = new (this);

			BaseHash = zone.Cryptography.ZeroHash;

			var g = Database.Get(GenesisKey);

			if(g == null)
			{
				Initialize();
			}
			else
			{
				if(g.SequenceEqual(Zone.Genesis.FromHex()))
				{
					Load();
				}
				else
				{ 
					Clear();
					Initialize();
				}
			}
		}

		public void Initialize()
		{
			if(Roles.HasFlag(Role.Chain))
			{
				Tail.Clear();
	
 				var rd = new BinaryReader(new MemoryStream(Zone.Genesis.FromHex()));
						
				for(int i = 0; i <=1+P + 1+P + P; i++)
				{
					var r = new Round(this);
					r.Read(rd);
		
					Tail.Insert(0, r);
	
					if(r.Id > 0)
					{
						r.ConfirmedTime = r.Confirmed ? r.ConfirmedTime : r.Votes.First().Time;
						r.ConfirmedExeunitMinFee = Zone.ExeunitMinFee;
					}
	
					if(i <= 1+P + 1+P)
					{
						if(i == 0)
							r.ConfirmedFundJoiners = new[] {Zone.Father0};

						if(i == 1)
							r.ConfirmedEmissions = new OperationId[] {new(0, 0, 0)};

						r.ConfirmedTransactions = r.OrderedTransactions.ToArray();

						r.Hashify();
						Confirm(r);
						Commit(r);
					}
				}
	
				if(Tail.Any(i => i.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null)))))
					throw new IntegrityException("Genesis construction failed");
			}
		
			Database.Put(GenesisKey, Zone.Genesis.FromHex());
		}
		
		public void Load()
		{
			BaseState = Database.Get(BaseStateKey);

			if(BaseState != null)
			{
				var r = new BinaryReader(new MemoryStream(BaseState));
		
				LastCommittedRound = new Round(this);
				LastCommittedRound.ReadBaseState(r);

				LoadedRounds.Add(LastCommittedRound.Id, LastCommittedRound);

				Hashify();

				if(!BaseHash.SequenceEqual(Database.Get(__BaseHashKey)))
				{
					throw new IntegrityException("");
				}
			}

			if(Roles.HasFlag(Role.Chain))
			{
				var s = Database.Get(ChainStateKey);

				var rd = new BinaryReader(new MemoryStream(s));

				var lcr = FindRound(rd.Read7BitEncodedInt());
					
				for(int i = lcr.Id - lcr.Id % Zone.TailLength; i <= lcr.Id; i++)
				{
					var r = FindRound(i);

					Tail.Insert(0, r);
		
					r.Confirmed = false;
					//Execute(r, r.ConfirmedTransactions);
					Confirm(r);
				}
			}
		}

		public void Clear()
		{
			Tail.Clear();

			BaseState = null;
			BaseHash = Zone.Cryptography.ZeroHash;

			LastCommittedRound = null;
			LastConfirmedRound = null;

			LoadedRounds.Clear();
			Accounts.Clear();
			Authors.Clear();
			Releases.Clear();

			Database.Remove(BaseStateKey);
			Database.Remove(__BaseHashKey);
			Database.Remove(ChainStateKey);
			Database.Remove(GenesisKey);

			Database.DropColumnFamily(ChainFamilyName);
			Database.CreateColumnFamily(new (), ChainFamilyName);
		}

		public string CreateGenesis(AccountKey god, AccountKey f0)
		{
			/// 0 - emission request
			/// 1 - vote for emission 
			/// 1+P	 - emited
			/// 1+P + 1 - candidacy declaration
			/// 1+P + 1+P - decalared
			/// 1+P + 1+P + P - joined

			Clear();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
				r.ConfirmedTransactions = r.OrderedTransactions.ToArray();
				r.Hashify();
				r.Write(w);
			}
	
			var v0 = new Vote(this) {RoundId = 0, Time = Time.Zero, ParentHash = Zone.Cryptography.ZeroHash};
			{
				var t = new Transaction {Zone = Zone, Nid = 0, Expiration = 0};
				t.AddOperation(new Emission(Web3.Convert.ToWei(1_000_000, UnitConversion.EthUnit.Ether), 0));
				//t.AddOperation(new AuthorBid("uo", null, 1));
				t.Sign(f0, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				v0.Sign(god);
				Add(v0);
				v0.FundJoiners = v0.FundJoiners.Append(Zone.Father0).ToArray();
				write(0);
			}
			
			/// UO Autor

			var v1 = new Vote(this) {RoundId = 1, Time = Time.Zero, ParentHash = Zone.Cryptography.ZeroHash};
			{
				v1.Emissions = new OperationId[] {new(0, 0, 0)};
	
				v1.Sign(god);
				Add(v1);
				write(1);
			}
	
			for(int i = 2; i <= 1+P + 1+P + P; i++)
			{
				var v = new Vote(this){	 RoundId	= i,
										 Time		= Time.Zero,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
										 ParentHash	= i < P ? Zone.Cryptography.ZeroHash : Summarize(GetRound(i - P)) };
		 
				if(i == 1+P + 1)
				{
					var t = new Transaction {Zone = Zone, Nid = 1, Expiration = i};
					t.AddOperation(new CandidacyDeclaration{Bail = 1_000_000,
															BaseRdcIPs = new [] {Zone.Father0IP},
															SeedHubRdcIPs = new [] {Zone.Father0IP} });
					t.Sign(f0, Zone.Cryptography.ZeroHash);
					v.AddTransaction(t);
				}
	
				v.Sign(god);
				Add(v);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		public bool Add(Vote vote)
		{
			var r = GetRound(vote.RoundId);

			vote.Round = r;

			r.Votes.Add(vote);
		
			if(vote.Transactions.Any())
			{
				foreach(var t in vote.Transactions)
				{
					t.Round = r;
					t.Placing = PlacingStage.Placed;
				}
			}
	
			if(r.FirstArrivalTime == DateTime.MaxValue)
			{
				r.FirstArrivalTime = DateTime.UtcNow;
			} 

			VoteAdded?.Invoke(vote);

			if(vote.RoundId > LastGenesisRound && r.Parent.Previous.Confirmed && !r.Parent.Confirmed)
			{
				if(r.ConsensusReached)
				{
					ConsensusConcluded(r, true);

					var hbm = r.Majority.Key;
	 		
					if(r.Parent.Hash == null || !hbm.SequenceEqual(r.Parent.Hash))
					{
						Summarize(r.Parent);
						
						if(!hbm.SequenceEqual((r.Parent.Hash)))
						{
							#if DEBUG
							var x = r.Eligible.Select(i => i.ParentHash.ToHex());
							var a = SunGlobals.Suns.Select(i => i.Mcv.FindRound(r.ParentId)?.Hash?.ToHex());
							#endif

							throw new ConfirmationException(r.Parent, hbm);
						}
					}

					Confirm(r.Parent);
					Commit(r.Parent);

					return true;
	
				}
				else if(ConsensusFailed(r))
				{
					r.FirstArrivalTime = DateTime.MaxValue;
					r.Try++;

					r.Parent.Hash = null;

					ConsensusConcluded(r, false);
				}
			}

			return false;
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
					return i;

			if(LoadedRounds.TryGetValue(rid, out var r))
				return r;

			var d = Database.Get(BitConverter.GetBytes(rid), ChainFamily);

			if(d != null)
			{
				r = new Round(this);
				r.Id			= rid; 
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
			if(LoadedRounds.Count > Zone.TailLength)
			{
				foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.Id).Skip(Zone.TailLength))
				{
					LoadedRounds.Remove(i.Key);
				}
			}
		}

		public List<Member> VotersOf(Round round)
		{
			return FindRound(round.VotersRound).Members/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public List<Analyzer> AnalyzersOf(int rid)
		{
			return FindRound(rid - P - 1).Analyzers/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool ConsensusFailed(Round r)
		{
			var m = VotersOf(r);

			var e = r.Eligible;
			
			var d = m.Count - e.Count();

			var q = r.RequiredVotes;

			return e.Any() && e.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).All(i => i.Count() + d < q);
		}

		///public Time CalculateTime(Round round, IEnumerable<Vote> votes)
		///{
 		///	if(round.Id == 0)
 		///	{
 		///		return round.ConfirmedTime;
 		///	}
		///	
 		///	if(!votes.Any())
 		///	{
		///		return round.Previous.ConfirmedTime + new Time(1);
		///	}
		///	
		///	if(votes.Count() < 3)
		///	{
		///		var a = votes.Sum(i => i.TimeDelta)/votes.Count();
		///		return round.Previous.ConfirmedTime + new Time(a);
		///	}
		///	else
		///	{
		///		var n = votes.Count();
		///		votes = votes.OrderBy(i => i.TimeDelta).Skip(n/3).Take(n/3);
		///		var a = votes.Sum(i => i.TimeDelta)/votes.Count();
		///	
		///		return round.Previous.ConfirmedTime + new Time(a);
		///	}
		///}
			
		public IEnumerable<AccountAddress> ProposeViolators(Round round)
		{
			var g = round.Id > P ? VotersOf(round) : new();
			var gv = round.VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			return gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();
		}

		public IEnumerable<AccountAddress> ProposeMemberLeavers(Round round, AccountAddress generator)
		{
			var prevs = Enumerable.Range(round.ParentId - P, P).Select(i => FindRound(i));

			var ls = VotersOf(round).Where(i =>	i.CastingSince <= round.ParentId &&/// in previous Pitch number of rounds
												!round.Parent.VotesOfTry.Any(v => v.Generator == i.Account) &&	/// ??? sent less than MinVotesPerPitch of required blocks
												!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Account)))) /// not yet proposed in prev [Pitch-1] rounds
									.Select(i => i.Account);
			return ls;
		}

		public void Hashify()
		{
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var i in Accounts.SuperClusters.OrderBy(i => i.Key))	BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Authors.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Releases.SuperClusters.OrderBy(i => i.Key))	BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}

		public byte[] Summarize(Round round)
		{
			var m = round.Id >= DeclareToGenerateDelay ? VotersOf(round) : new();
			var gq = m.Count * 2/3;
			var gv = round.VotesOfTry.Where(i => m.Any(j => i.Generator == j.Account)).ToArray();
			var gu = gv.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();
			var gf = gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();

			var tn = gu.Sum(i => i.Transactions.Length);
			
			round.ConfirmedExeunitMinFee = round.Id == 0 ? Zone.ExeunitMinFee	: round.Previous.ConfirmedExeunitMinFee;
			round.ConfirmedOverflowRound = round.Id == 0 ? 0					: round.Previous.ConfirmedOverflowRound;

			if(tn > Zone.TransactionsPerRoundLimit)
			{
				round.ConfirmedExeunitMinFee *= Zone.TransactionsFeeOverflowFactor;
				round.ConfirmedOverflowRound = round.Id;

				var e = tn - Zone.TransactionsPerRoundLimit;

				var gi = gu.AsEnumerable().GetEnumerator();

				do
				{
					if(!gi.MoveNext())
						gi.Reset();
					
					if(gi.Current.Transactions.Length > round.TransactionsPerVoteExecutionLimit)
					{
						e--;
						gi.Current.TransactionCountExcess++;
					}
				}
				while(e > 0);

				foreach(var i in gu.Where(i => i.TransactionCountExcess > 0))
				{
					var ts = new Transaction[i.Transactions.Length - i.TransactionCountExcess];
					Array.Copy(i.Transactions, i.TransactionCountExcess, ts, 0, ts.Length);
					i.Transactions = ts;
				}
			}
			else 
			{
				if(round.ConfirmedExeunitMinFee > Zone.ExeunitMinFee && round.Id - round.ConfirmedOverflowRound > P)
					round.ConfirmedExeunitMinFee /= Zone.TransactionsFeeOverflowFactor;
			}
			
			var txs = gu.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();
			//round.ConfirmedTime = CalculateTime(round, gu);

			var t = gu.GroupBy(x => x.Time).MaxBy(i => i.Count());

			if(t != null)
			{
				if(t.Count() >= gq && t.Key > round.Previous.ConfirmedTime)
					round.ConfirmedTime	= t.Key;
				else
					round.ConfirmedTime = round.Previous.ConfirmedTime;
			}

			Execute(round, txs);

			round.ConfirmedTransactions = txs.Where(i => i.Successful).ToArray();

			if(round.Id >= P)
			{
				round.ConfirmedMemberLeavers	= gu.SelectMany(i => i.MemberLeavers).Distinct()
													.Where(x => round.Members.Any(j => j.Account == x) && gu.Count(b => b.MemberLeavers.Contains(x)) >= gq)
													.Order().ToArray();

				round.ConfirmedViolators		= gu.SelectMany(i => i.Violators).Distinct()
													.Where(x => gu.Count(b => b.Violators.Contains(x)) >= gq)
													.Order().ToArray();

				round.ConfirmedEmissions		= gu.SelectMany(i => i.Emissions).Distinct()
													.Where(x => round.Emissions.Any(e => e.Id == x) && gu.Count(b => b.Emissions.Contains(x)) >= gq)
													.Order().ToArray();

				round.ConfirmedDomainBids		= gu.SelectMany(i => i.DomainBids).Distinct()
													.Where(x => round.DomainBids.Any(b => b.Id == x) && gu.Count(b => b.DomainBids.Contains(x)) >= gq)
													.Order().ToArray();

				round.ConfirmedAnalyzerJoiners	= gu.SelectMany(i => i.AnalyzerJoiners).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) == null && gu.Count(b => b.AnalyzerJoiners.Contains(x)) >= Zone.AnalizerMinimumVotes)
													.Order().ToArray();
				
				round.ConfirmedAnalyzerLeavers	= gu.SelectMany(i => i.AnalyzerLeavers).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) != null && gu.Count(b => b.AnalyzerLeavers.Contains(x)) >= Zone.AnalizerMinimumVotes)
													.Order().ToArray();

				round.ConfirmedFundJoiners		= gu.SelectMany(i => i.FundJoiners).Distinct()
													.Where(x => !round.Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
													.Order().ToArray();
				
				round.ConfirmedFundLeavers		= gu.SelectMany(i => i.FundLeavers).Distinct()
													.Where(x => round.Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
													.Order().ToArray();

				//round.ConfirmedAnalyses	= au.SelectMany(i => i.Analyses).DistinctBy(i => i.Resource)
				//							.Select(i => {
				//											var e = Authors.FindResource(i.Resource, round.Id);
				//													
				//											if(e == null)
				//												return null; /// Some analyzer(s) is buggy
				//													
				//											var v = au.Select(u => u.Analyses.FirstOrDefault(x => x.Resource == i.Resource)).Where(i => i != null);
				//
				//											if(v.Count() == a.Count || (e.AnalysisStage == AnalysisStage.HalfVotingReached && round.Id > e.RoundId + (e.AnalysisHalfVotingRound - e.RoundId) * 2))
				//											{ 
				//												var cln = v.Count(i => i.Result == AnalysisResult.Clean); 
				//												var inf = v.Count(i => i.Result == AnalysisResult.Infected);
				//
				//												return new AnalysisConclusion { Resource = i.Resource, Good = (byte)cln, Bad = (byte)inf };
				//											}
				//											else if(e.AnalysisStage == AnalysisStage.Pending && v.Count() >= a.Count/2)
				//												return new AnalysisConclusion { Resource = i.Resource, HalfReached = true};
				//											else
				//												return null;
				//										})
				//							.Where(i => i != null)
				//							.OrderBy(i => i.Resource).ToArray();
			}

			round.Hashify(); /// depends on BaseHash 

			return round.Hash;
		}

		public void Execute(Round round, IEnumerable<Transaction> transactions)
		{
			if(round.Confirmed)
				throw new IntegrityException();
	
			if(round.Id != 0 && round.Previous == null)
				return;

			foreach(var t in transactions)
				foreach(var o in t.Operations)
					o.Error = null;

			round.Members			 = round.Id == 0 ? new() : round.Previous.Members;
			round.Analyzers			 = round.Id == 0 ? new() : round.Previous.Analyzers;
			round.Funds				 = round.Id == 0 ? new() : round.Previous.Funds;
			round.Emissions			 = round.Id == 0 ? new() : round.Previous.Emissions;
			round.DomainBids		 = round.Id == 0 ? new() : round.Previous.DomainBids;
			round.AnalyzersIdCounter = round.Id == 0 ? new() : round.Previous.AnalyzersIdCounter;

			round.NextAccountIds	= new Dictionary<byte[], int>(Bytes.EqualityComparer);
			round.NextAuthorIds		= new Dictionary<byte[], int>(Bytes.EqualityComparer);
			round.NextAnalysisIds	= new Dictionary<byte[], int>(Bytes.EqualityComparer);

		start: 
			round.Fees		= 0;
			round.Emission	= round.Id == 0 ? 0 : round.Previous.Emission;

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();
			round.AffectedReleases.Clear();

			foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
			{
				var a = round.AffectAccount(t.Signer);

				if(t.Nid != a.LastTransactionNid + 1)
				{
					foreach(var o in t.Operations)
						o.Error = Operation.NotSequential;
					
					goto start;
				}

				foreach(var o in t.Operations.AsEnumerable().Reverse())
				{
					o.Execute(this, round);

					if(o.Error != null)
						goto start;

					if(a.Balance - t.Fee < 0)
					{
						o.Error = Operation.NotEnoughUNT;
						goto start;
					}
				}

				round.Fees += t.Fee;
				a.Balance -= t.Fee;
				a.LastTransactionNid++;
						
				if(Roles.HasFlag(Role.Chain))
				{
					round.AffectAccount(t.Signer).Transactions.Add(round.Id);
				}
			}
		}

		public void Confirm(Round round)
		{
			if(round.Confirmed)
				throw new IntegrityException();

			if(round.Id > 0 && LastConfirmedRound != null && LastConfirmedRound.Id + 1 != round.Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == round.Id");

			if(round.Id % 100 == 0 && LastCommittedRound != null && LastCommittedRound != round.Previous)
				throw new IntegrityException("round.Id % 100 == 0 && LastCommittedRound != round.Previous");

			Execute(round, round.ConfirmedTransactions);

			round.Members		= round.Members.ToList();
			round.Analyzers		= round.Analyzers.ToList();
			round.Funds			= round.Funds.ToList();
			round.Emissions		= round.Emissions.ToList();
			round.DomainBids	= round.DomainBids.ToList();

			foreach(var f in round.ConfirmedViolators)
			{
				var fe = round.AffectAccount(f);
				round.Fees = fe.Bail;
				fe.Bail = 0;
			}
			
			for(int ti = 0; ti < round.ConfirmedTransactions.Length; ti++)
			{
				for(int oi = 0; oi < round.ConfirmedTransactions[ti].Operations.Length; oi++)
				{
					var o = round.ConfirmedTransactions[ti].Operations[oi];

					if(o is Emission e)
						round.Emissions.Add(e);

					if(o is AuthorBid b && b.Tld.Any())
						round.DomainBids.Add(b);
				}
			}

			foreach(var i in round.ConfirmedEmissions)
			{
				var e = round.Emissions.Find(j => j.Id == i);
				e.ConsensusExecute(round);
				round.Emissions.Remove(e);
			}

			round.Emissions.RemoveAll(i => round.Id > i.Id.Ri + Zone.ExternalVerificationDurationLimit);

			foreach(var i in round.ConfirmedDomainBids)
			{
				var b = round.DomainBids.Find(j => j.Id == i);
				b.ConsensusExecute(round);
				round.DomainBids.Remove(b);
			}

			round.DomainBids.RemoveAll(i => round.Id > i.Id.Ri + Zone.ExternalVerificationDurationLimit);

	
			foreach(var t in round.OrderedTransactions)
			{
				t.Placing = round.ConfirmedTransactions.Contains(t) ? PlacingStage.Confirmed : PlacingStage.FailedOrNotFound;

				#if DEBUG
				//if(t.__ExpectedPlacing > PlacingStage.Placed && t.Placing != t.__ExpectedPlacing)
				//{
				//	Debugger.Break();
				//}
				#endif
			}

			foreach(var i in round.Members.Where(i => round.ConfirmedViolators.Contains(i.Account)))
				Log?.Report(this, $"Member violator removed {round.Id} - {i.Account}");

			round.Members.RemoveAll(i => round.ConfirmedViolators.Contains(i.Account));

			foreach(var i in round.Members.Where(i => round.ConfirmedMemberLeavers.Contains(i.Account)))
				Log?.Report(this, $"Member leaver removed {round.Id} - {i.Account}");

			round.Members.RemoveAll(i => round.ConfirmedMemberLeavers.Contains(i.Account));

			var js = round.ConfirmedTransactions.SelectMany(i => i.Operations)
												.OfType<CandidacyDeclaration>()
												.DistinctBy(i => i.Transaction.Signer)
												.Where(i => !round.ConfirmedViolators.Contains(i.Transaction.Signer) && !round.ConfirmedMemberLeavers.Contains(i.Transaction.Signer))
												.OrderByDescending(i => i.Bail)
												.ThenBy(i => i.Signer)
												.Take(Zone.MembersLimit - round.Members.Count);
 
			round.Members.AddRange(js.Select(i => new Member{CastingSince = round.Id + DeclareToGenerateDelay,
															 Account = i.Signer, 
															 BaseRdcIPs = i.BaseRdcIPs, 
															 SeedHubRdcIPs = i.SeedHubRdcIPs}));


			round.Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));
			round.Funds.AddRange(round.ConfirmedFundJoiners);

			round.Analyzers.RemoveAll(i => round.ConfirmedAnalyzerLeavers.Contains(i.Account));
			round.Analyzers.AddRange(round.ConfirmedAnalyzerJoiners.Select(i => new Analyzer {Id = (byte)round.AnalyzersIdCounter++, Account = i, JoinedAt = round.Id + P + 1}));
					
			round.Confirmed = true;

			LastConfirmedRound = round;
		}
	
		public void Commit(Round round)
		{
			using(var b = new WriteBatch())
			{
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
	
				if(Tail.Count(i => i.Id <= round.Id) >= Zone.TailLength)
				{
					var tail = Tail.AsEnumerable().Reverse().Take(Zone.TailLength);
		
					var f = Money.Zero;
					
					foreach(var i in tail)
					{
						f += i.Fees;
					}

					round.Distribute(f, round.Members.Where(i => i.CastingSince <= tail.First().Id).Select(i => i.Account), 9, round.Funds, 1); /// taking 10% we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding

					foreach(var i in tail)
					{
						Accounts.Save(b, i.AffectedAccounts.Values);
						Authors.Save(b, i.AffectedAuthors.Values);
						Releases.Save(b, i.AffectedReleases.Values);
					}
	
					LastCommittedRound = tail.Last();
		
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
		
					LastCommittedRound.WriteBaseState(w);
		
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
	
				Database.Write(b);
			}

			//if(round.Id > Pitch)
			{
				var ro = FindRound(round.Id - P-1);
				
				if(ro != null)
				{
					#if !DEBUG
					//ro.Votes.Clear();
					//ro.AnalyzerVoxes.Clear();
					#endif
				}
			}

			Commited?.Invoke(round);
			//round.JoinRequests.RemoveAll(i => i.RoundId < round.Id - Pitch);
		}

		public Transaction FindLastTailTransaction(Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var t in r.Transactions)
					if(transaction_predicate == null || transaction_predicate(t))
						return t;

			return null;
		}

		public IEnumerable<Transaction> FindLastTailTransactions(Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var t in transaction_predicate == null ? r.Transactions : r.Transactions.Where(transaction_predicate))
					yield return t;
		}

		public O FindLastTailOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
		}

		IEnumerable<O> FindLastTailOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops : ops.Where(op);
		}

		public IEnumerable<Resource> QueryResource(string query)
		{
			var r = ResourceAddress.Parse(query);
		
			var a = Authors.Find(r.Author, LastConfirmedRound.Id);

			if(a == null)
				yield break;

			foreach(var i in a.Resources.Where(i => i.Address.Resource.StartsWith(r.Resource)))
				yield return i;
		}
	}
}
