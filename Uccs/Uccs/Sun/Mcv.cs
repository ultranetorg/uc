using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Hex.HexConvertors.Extensions;
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

		public const int					Pitch = 8;
		public const int					LastGenesisRound = 18;
		public const int					AnalyzersMax = 32;
		///public const int					MembersRotation = 32;
		public static readonly Money		SpaceBasicFeePerByte	= new Money(0.000001);
		public static readonly Money		AnalysisFeePerByte		= new Money(0.000000001);
		//public static readonly Money		AuthorFeePerYear		= new Money(1);
		public static readonly Money		AccountAllocationFee	= new Money(1);
		public const int					EntityAllocationAverageLength = 100;
		public const int					EntityAllocationYearsMin = 1;
		public const int					EntityAllocationYearsMax = 32;

		public Zone							Zone;
		public McvSettings					Settings;
		public Role							Roles;

		public List<Round>					Tail = new();
		public Dictionary<int, Round>		LoadedRounds = new();

		public RocksDb						Engine;
		public byte[]						BaseState;
		public byte[]						BaseHash;
		static readonly byte[]				BaseStateKey = new byte[] {0x01};
		static readonly byte[]				__BaseHashKey = new byte[] {0x02};
		static readonly byte[]				ChainStateKey = new byte[] {0x03};
		static readonly byte[]				GenesisKey = new byte[] {0x04};
		public AccountTable					Accounts;
		public AuthorTable					Authors;
		public int							Size => BaseState == null ? 0 : (BaseState.Length + 
																			Accounts.Clusters.Sum(i => i.MainLength) +
																			Authors.Clusters.Sum(i => i.MainLength));
		public BlockDelegate				VoteAdded;
		public ConsensusDelegate			ConsensusConcluded;
		public RoundDelegate				Commited;

		public Round						LastConfirmedRound;
		public Round						LastCommittedRound;
		public Round						LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
		public Round						LastPayloadRound	=> Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;

		public const string					ChainFamilyName = "Chain";
		public ColumnFamilyHandle			ChainFamily	=> Engine.GetColumnFamily(ChainFamilyName);

		public static int					GetValidityPeriod(int rid) => rid + Pitch;

		public Mcv(Zone zone, Role roles, McvSettings settings, RocksDb engine)
		{
			Roles = roles & (Role.Base|Role.Chain);
			Zone = zone;
			Settings = settings;
			Engine = engine;

			Accounts = new (this);
			Authors = new (this);

			BaseHash = Zone.Cryptography.ZeroHash;
			BaseState = Engine.Get(BaseStateKey);

			if(BaseState != null)
			{
				var r = new BinaryReader(new MemoryStream(BaseState));
		
				LastCommittedRound = new Round(this);
				LastCommittedRound.ReadBaseState(r);

				LoadedRounds.Add(LastCommittedRound.Id, LastCommittedRound);

				Hashify();

				if(!BaseHash.SequenceEqual(Engine.Get(__BaseHashKey)))
				{
					throw new IntegrityException("");
				}
			}

			Initialize();
		}

		void Initialize()
		{
			if(Roles.HasFlag(Role.Chain))
			{
				var chainstate = Engine.Get(ChainStateKey);

				if(chainstate == null || !Engine.Get(GenesisKey).SequenceEqual(Zone.Genesis.HexToByteArray()))
				{
					Tail.Clear();
	
 					var rd = new BinaryReader(new MemoryStream(Zone.Genesis.HexToByteArray()));
						
					for(int i = 0; i <=1+8 + 1+8; i++)
					{
						var r = new Round(this);
						r.Read(rd);
		
						Tail.Insert(0, r);
	
						if(r.Id > 0)
						{
							r.ConfirmedTime = CalculateTime(r, r.VotesOfTry);
							r.ConfirmedExeunitMinFee = Zone.ExeunitMinFee;
						}
	
						if(i <= 1+8 + 1)
						{
							if(i == 0)
								r.ConfirmedFundJoiners = new[] {Zone.Father0};

							if(i == 1)
								r.ConfirmedEmissions = new OperationId[] {new(0, 0, 0)};

							r.ConfirmedTransactions = r.OrderedTransactions.ToArray();

							r.Hashify();
							Execute(r, r.ConfirmedTransactions);
							Confirm(r);
							Commit(r);
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
					
					for(int i = lcr.Id - lcr.Id % Zone.TailLength; i <= lcr.Id; i++)
					{
						var r = FindRound(i);

						Tail.Insert(0, r);
		
						r.Confirmed = false;
						Execute(r, r.ConfirmedTransactions);
						Confirm(r);
					}
				}
			}
		}

		public void Clear()
		{
			BaseState = null;
			BaseHash = Zone.Cryptography.ZeroHash;

			LastCommittedRound = null;
			LastConfirmedRound = null;

			LoadedRounds.Clear();
			Accounts.Clear();
			Authors.Clear();

			Engine.Remove(BaseStateKey);
			Engine.Remove(__BaseHashKey);
			Engine.Remove(ChainStateKey);
			Engine.Remove(GenesisKey);

			Engine.DropColumnFamily(ChainFamilyName);
			Engine.CreateColumnFamily(new (), ChainFamilyName);

			Initialize();
		}

		public string CreateGenesis(AccountKey god, AccountKey[] fathers)
		{
			/// 0 - emission request
			/// 1 - vote for emission 
			/// 1+8	 - emited
			/// 1+8+1 - candidacy declaration
			/// 1+8+1 + 8 - decalared
			/// 1+8+1 + 8+1 - join request
			/// 1+8+1 + 8+1+8 - joined

			var f0 = fathers[0];

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
				r.ConfirmedTransactions = r.OrderedTransactions.ToArray();
				r.Hashify();
				r.Write(w);
			}
	
			var v0 = new Vote(this){ RoundId = 0, TimeDelta = 1, ParentHash = Zone.Cryptography.ZeroHash};
			{
				var t = new Transaction(Zone) {Nid = 0, Member = god, Expiration = 0};
				t.AddOperation(new Emission(Web3.Convert.ToWei(fathers.Length * 1000 + 1_000_000, UnitConversion.EthUnit.Ether), 0));
				//t.AddOperation(new AuthorBid("uo", null, 1));
				t.Sign(f0, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				//foreach(var f in fathers.OrderBy(j => j).ToArray())
				//{
				//	t = new Transaction(Zone) {Id = 0, Generator = god, Expiration = 0};
				//	t.AddOperation(new Emission(Web3.Convert.ToWei(1000, UnitConversion.EthUnit.Ether), 0));
				//	t.Sign(f, Zone.Cryptography.ZeroHash);
				//	b0.AddTransaction(t);
				//}
			
				v0.Sign(god);
				Add(v0);
				v0.FundJoiners = v0.FundJoiners.Append(Zone.Father0).ToArray();
				write(0);
			}
			
			/// UO Autor

			var v1 = new Vote(this){ RoundId = 1, TimeDelta = 1, ParentHash = Zone.Cryptography.ZeroHash};
			{
		
				//t = new Transaction(Zone){Id = 1, Generator = god, Expiration = 1};
				//t.AddOperation(new AuthorRegistration("uo", "UO", EntityAllocationYearsMax));
				//t.Sign(org, Zone.Cryptography.ZeroHash);
				//b1.AddTransaction(t);
	
				//var ops = FindRound(0).Transactions.SelectMany(i => i.Operations).ToList();
				//b1.Emissions = ops.OfType<Emission>().Select(i => new OperationId {Round = 0, Index = ops.IndexOf(i)}).ToList();
				v1.Emissions = new OperationId[] {new(0, 0, 0)};
	
				v1.Sign(god);
				Add(v1);
				write(1);
			}
	
			for(int i = 2; i <= 1+8 + 1+8; i++)
			{
				var v = new Vote(this){	 RoundId		= i,
										 TimeDelta		= 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
										 ParentHash	= i < 8 ? Zone.Cryptography.ZeroHash : Summarize(GetRound(i - Pitch)) };
		 
				if(i == 1+8 + 1)
				{
					var t = new Transaction(Zone) {Nid = 1, Member = god, Expiration = i};
					t.AddOperation(new CandidacyDeclaration {	Bail = 1_000_000,
																BaseRdcIPs = new IPAddress[] {Zone.Father0IP},
																SeedHubRdcIPs = new IPAddress[] {Zone.Father0IP} });
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
			//r.Blocks = r.Blocks.OrderBy(i => i is Payload p ? p.OrderingKey : new byte[] {}, new BytesComparer()).ToList();
				
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
							if(DevSettings.Suns[0].Mcv == this)
							{
								r=r;
							}

							var x = r.Eligible.Select(i => i.ParentHash.ToHex());
							var a = DevSettings.Suns.Select(i => i.Mcv.FindRound(r.ParentId)?.Hash?.ToHex());
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

			var d = Engine.Get(BitConverter.GetBytes(rid), ChainFamily);

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

		//public List<Hub> HubsOf(int rid)
		//{
		//	return FindRound(rid - Pitch - 1).Hubs/*.Where(i => i.JoinedAt < r.Id)*/;
		//}

		public List<Analyzer> AnalyzersOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Analyzers/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool ConsensusFailed(Round r)
		{
			var m = VotersOf(r);

			var e = r.Eligible;
			
			var d = m.Count - e.Count();

			var q = r.RequiredVotes;

			return e.Any() && e.GroupBy(i => i.ParentHash, new BytesEqualityComparer()).All(i => i.Count() + d < q);
		}

		public Time CalculateTime(Round round, IEnumerable<Vote> votes)
		{
 			if(round.Id == 0)
 			{
 				return round.ConfirmedTime;
 			}

 			if(!votes.Any())
 			{
				return round.Previous.ConfirmedTime + new Time(1);
			}

			if(votes.Count() < 3)
			{
				var a = votes.Sum(i => i.TimeDelta)/votes.Count();
				return round.Previous.ConfirmedTime + new Time(a);
			}
			else
			{
				var n = votes.Count();
				votes = votes.OrderBy(i => i.TimeDelta).Skip(n/3).Take(n/3);
				var a = votes.Sum(i => i.TimeDelta)/votes.Count();

				return round.Previous.ConfirmedTime + new Time(a);
			}
		}

//		public IEnumerable<Transaction> CollectValidTransactions(IEnumerable<Transaction> txs, Round round)
//		{
//			//txs = txs.Where(i => round.Id <= i.RoundMax /*&& IsSequential(i, round.Id)*/);
//
//			if(txs.Any())
//			{
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
//			}
//
//			return txs;
//		}

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
		
		public IEnumerable<AccountAddress> ProposeViolators(Round round)
		{
			var g = round.Id > Pitch ? VotersOf(round) : new();
			var gv = round.VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			return gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();
		}

// 		public IEnumerable<AccountAddress> ProposeMemberJoiners(Round round)
// 		{
// 			var o = round.Parent.JoinRequests.Select(jr =>	{
// 																var a = Accounts.Find(jr.Generator, LastConfirmedRound.Id);
// 																return new {jr = jr, a = a};
// 															})	/// round.ParentId - Pitch means to not join earlier than [Pitch] after declaration, and not redeclare after a join is requested
// 													.Where(i => i.a != null && 
// 																i.a.CandidacyDeclarationRid < round.Id &&
// 																i.a.Bail >= Zone.BailMin)
// 													.OrderByDescending(i => i.a.Bail)
// 													.ThenBy(i => i.a.Address)
// 													.Select(i => i.jr);
// 
// 			var n = Math.Min(Zone.MembersLimit - MembersOf(round.Id).Count(), o.Count());
// 
// 			return o.Take(n).Select(i => i.Generator);
// 		}

		public IEnumerable<AccountAddress> ProposeMemberLeavers(Round round, AccountAddress generator)
		{
			var prevs = Enumerable.Range(round.ParentId - Pitch, Pitch).Select(i => FindRound(i));

			var ls = VotersOf(round).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
												!round.Parent.VotesOfTry.Any(v => v.Generator == i.Account) &&	/// ??? sent less than MinVotesPerPitch of required blocks
												!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Account)))) /// not yet proposed in prev [Pitch-1] rounds
									.Select(i => i.Account);
//foreach(var i in ls)
//{
//	Log?.Report(this, $"Proposed leaver for {round.Id} - {i} - {prevs.Count(r => r.VotesOfTry.Any(b => b.Generator == i))} - {prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i)))}");
//}
			return ls;
		}

		public void Hashify()
		{
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var i in Accounts.SuperClusters.OrderBy(i => i.Key))	BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Authors.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}

		public byte[] Summarize(Round round)
		{
			//if(round.Id > LastGenesisRound && !round.Parent.Confirmed)
			//	return null;

			var m = round.Id > Pitch ? VotersOf(round) : new();
			var gv = round.VotesOfTry.Where(i => m.Any(j => i.Generator == j.Account)).ToArray();
			var gu = gv.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();
			var gf = gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();

			var a = round.Id > Pitch ? AnalyzersOf(round.Id) : new();
			var av = round.AnalyzerVoxes.Where(i => a.Any(j => j.Account == i.Account)).ToArray();
			var au = av.GroupBy(i => i.Account).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();

			var tn = gu.Sum(i => i.Transactions.Length);
			
			round.ConfirmedExeunitMinFee = round.Id == 0 ? Zone.ExeunitMinFee	: round.Previous.ConfirmedExeunitMinFee;
			round.ConfirmedOverflowRound = round.Id == 0 ? 0					: round.Previous.ConfirmedOverflowRound;

			if(tn > Zone.TransactionsPerRoundLimit)
			{
				round.ConfirmedExeunitMinFee *= Zone.TransactionsOverflowFactor;
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
				if(round.ConfirmedExeunitMinFee > Zone.ExeunitMinFee && round.Id - round.ConfirmedOverflowRound > Pitch)
					round.ConfirmedExeunitMinFee /= Zone.TransactionsOverflowFactor;
			}
			
			var txs = gu.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();
			round.ConfirmedTime = CalculateTime(round, gu);

			Execute(round, txs);

			round.ConfirmedTransactions = txs.Where(i => i.Successful).ToArray();

			if(round.Id >= Pitch)
			{
				var gq = m.Count * 2/3;
	
				round.ConfirmedMemberLeavers	= gu.SelectMany(i => i.MemberLeavers).Distinct()
													.Where(x => round.Members.Any(j => j.Account == x) && gu.Count(b => b.MemberLeavers.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedViolators		= gu.SelectMany(i => i.Violators).Distinct()
													.Where(x => gu.Count(b => b.Violators.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedEmissions		= gu.SelectMany(i => i.Emissions).Distinct()
													.Where(x => round.Emissions.Any(e => e.Id == x) && gu.Count(b => b.Emissions.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedDomainBids		= gu.SelectMany(i => i.DomainBids).Distinct()
													.Where(x => round.DomainBids.Any(b => b.Id == x) && gu.Count(b => b.DomainBids.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedAnalyzerJoiners	= gu.SelectMany(i => i.AnalyzerJoiners).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) == null && gu.Count(b => b.AnalyzerJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
													.OrderBy(i => i).ToArray();
				
				round.ConfirmedAnalyzerLeavers	= gu.SelectMany(i => i.AnalyzerLeavers).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) != null && gu.Count(b => b.AnalyzerLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
													.OrderBy(i => i).ToArray();

				round.ConfirmedFundJoiners		= gu.SelectMany(i => i.FundJoiners).Distinct()
													.Where(x => !round.Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
													.OrderBy(i => i).ToArray();
				
				round.ConfirmedFundLeavers		= gu.SelectMany(i => i.FundLeavers).Distinct()
													.Where(x => round.Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
													.OrderBy(i => i).ToArray();

				round.ConfirmedAnalyses	= au.SelectMany(i => i.Analyses).DistinctBy(i => i.Resource)
											.Select(i => {
															var e = Authors.FindResource(i.Resource, round.Id);
																	
															if(e == null)
																return null; /// Some analyzer(s) is buggy
																	
															var v = au.Select(u => u.Analyses.FirstOrDefault(x => x.Resource == i.Resource)).Where(i => i != null);

															if(v.Count() == a.Count || (e.AnalysisStage == AnalysisStage.HalfVotingReached && e.RoundId + (e.AnalysisHalfVotingRound - e.RoundId) * 2 == round.Id))
															{ 
																var cln = v.Count(i => i.Result == AnalysisResult.Clean); 
																var inf = v.Count(i => i.Result == AnalysisResult.Infected);

																return new AnalysisConclusion { Resource = i.Resource, Good = (byte)cln, Bad = (byte)inf };
																
															}
															else if(e.AnalysisStage == AnalysisStage.Pending && v.Count() >= a.Count/2)
																return new AnalysisConclusion { Resource = i.Resource, HalfReached = true};
															else
																return null;
														})
											.Where(i => i != null)
											.OrderBy(i => i.Resource).ToArray();
			}

			//var s = new MemoryStream();
			//var w = new BinaryWriter(s);
			//
			//w.Write(BaseHash);
			//w.Write(round.Id > 0 ? round.Previous.Hash : Zone.Cryptography.ZeroHash);
			//round.WriteConfirmed(w);
			//
			//round.Summary = Zone.Cryptography.Hash(s.ToArray());

			round.Hashify(); /// depends on BaseHash 

			return round.Hash;
		}

		public void Execute(Round round, IEnumerable<Transaction> transactions)
		{
			if(round.Confirmed)
				throw new IntegrityException();

			var prev = round.Previous;

			round.Members		= round.Id == 0 ? new()	: round.Previous.Members.ToList();
			round.Emissions		= round.Id == 0 ? new()	: round.Previous.Emissions.ToList();
			round.DomainBids	= round.Id == 0 ? new()	: round.Previous.DomainBids.ToList();
			round.Analyzers		= round.Id == 0 ? new()	: round.Previous.Analyzers.ToList();
			round.Funds			= round.Id == 0 ? new()	: round.Previous.Funds.ToList();
				
			if(round.Id != 0 && prev == null)
				return;

			foreach(var t in transactions)
				foreach(var o in t.Operations)
					o.Error = null;

		start: 
	
			round.Fees			= 0;
			round.Emission		= round.Id == 0 ? 0		: round.Previous.Emission;

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();

			foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
			{
				//Coin fee = 0;

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

					//var f = o.CalculateTransactionFee(round.TransactionPerByteFee);
	
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

// 			if(summarize)
// 			{
// 				if(round.Summary == null)
// 				{
// 					Summarize(round);
// 				}
// 
// 				var cm = VotersOf(round.Child);
// 				var s = round.Child.Unique.Where(i => cm.Any(j => j.Account == i.Generator)).GroupBy(i => i.ParentSummary, new BytesEqualityComparer()).MaxBy(i => i.Count()).Key;
// 	 		
// 				if(!s.SequenceEqual(round.Summary))
// 				{
// 					#if DEBUG
// 					var x = round.Child.Unique.Where(i => cm.Any(j => j.Account == i.Generator)).Select(i => i.ParentSummary.ToHex());
// 					var a = DevSettings.Suns.Select(i => i.Mcv.FindRound(round.Id)?.Summary?.ToHex());
// 					#endif
// 
// 					throw new ConfirmationException(round, s);
// 				}
// 			}
// 			else
// 			{
// 				Execute(round, round.ConfirmedTransactions, round.ConfirmedViolators);
// 			}

			//var ops = round.ConfirmedTransactions.SelectMany(t => t.Operations).ToArray();

			foreach(var f in round.ConfirmedViolators)
			{
				var fe = round.AffectAccount(f);
				round.Fees = fe.Bail;
				fe.Bail = 0;
			}
			
			round.Distribute(round.Fees, round.Members.Select(i => i.Account), 9, round.Funds, 1); /// taking 10% we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding


			for(int ti = 0; ti < round.ConfirmedTransactions.Length; ti++)
			{
				//round.ConfirmedTransactions[ti].Hid = new (round.Id, ti);

				for(int oi = 0; oi < round.ConfirmedTransactions[ti].Operations.Length; oi++)
				{
					var o = round.ConfirmedTransactions[ti].Operations[oi];
					//
					//o.Hid = new (round.Id, ti, oi);
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

			foreach(var i in round.ConfirmedAnalyses)
			{
				var e = round.AffectAuthor(i.Resource.Author).AffectResource(i.Resource);
				
				if(i.Finished)
				{
					e.Good			= i.Good;
					e.Bad			= i.Bad;
					e.AnalysisStage = AnalysisStage.Finished;

					round.Distribute(e.AnalysisFee, round.Analyzers.Select(i => i.Account));
				}
				else if(e.AnalysisStage == AnalysisStage.Pending && i.HalfReached)
				{
					e.AnalysisStage = AnalysisStage.HalfVotingReached;
				}
			}
								
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

			var js = round.ConfirmedTransactions.SelectMany(i => i.Operations)
												.OfType<CandidacyDeclaration>()
												.DistinctBy(i => i.Transaction.Signer)
												.OrderByDescending(i => i.Bail)
												.ThenBy(i => i.Signer);
 
			round.Members.AddRange(js.Take(Math.Min(Zone.MembersLimit - round.Members.Count, js.Count())).Select(i => new Member{ JoinedAt = round.Id + Pitch + 1,
																																  Account = i.Signer, 
																																  BaseRdcIPs = i.BaseRdcIPs, 
																																  SeedHubRdcIPs = i.SeedHubRdcIPs}));
foreach(var i in round.Members.Where(i => round.ConfirmedViolators.Contains(i.Account)))
	Log?.Report(this, $"Member violator removed {round.Id} - {i.Account}");

			round.Members.RemoveAll(i => round.ConfirmedViolators.Contains(i.Account));

//foreach(var i in round.Members.Where(i => round.AffectedAccounts.TryGetValue(i.Account, out var a) && a.CandidacyDeclarationRid == round.Id))
//	Log?.Report(this, $"Member removed due to CandidacyDeclarationRid == round.Id {round.Id} - {i.Account}");
			//round.Members.RemoveAll(i => round.AffectedAccounts.TryGetValue(i.Account, out var a) && a.CandidacyDeclarationRid == round.Id);  /// CandidacyDeclaration cancels membership

foreach(var i in round.Members.Where(i => round.ConfirmedMemberLeavers.Contains(i.Account)))
	Log?.Report(this, $"Member leaver removed {round.Id} - {i.Account}");

			round.Members.RemoveAll(i => round.ConfirmedMemberLeavers.Contains(i.Account));

			round.Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));
			round.Funds.AddRange(round.ConfirmedFundJoiners);

			round.Analyzers.RemoveAll(i => round.ConfirmedAnalyzerLeavers.Contains(i.Account));
			round.Analyzers.AddRange(round.ConfirmedAnalyzerJoiners.Select(i => new Analyzer {Account = i, JoinedAt = round.Id + Pitch + 1}));
					
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
		
					foreach(var i in tail)
					{
						Accounts.Save(b, i.AffectedAccounts.Values);
						Authors	.Save(b, i.AffectedAuthors.Values);
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
	
				Engine.Write(b);
			}

			//if(round.Id > Pitch)
			{
				var ro = FindRound(round.Id - Pitch-1);
				
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
