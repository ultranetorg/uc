using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	public delegate void JoinDelegate(MemberJoinRequest b);
	public delegate void ConsensusDelegate(Round b, bool reached);
	public delegate void RoundDelegate(Round b);

	public class Mcv /// Mutual chain voting
	{
		public const int					Pitch = 8;
		public const int					AliveMinMemberVotes = 6;
		public const int					LastGenesisRound = Pitch * 3;
		public const int					MembersMax = 1024;
		public const int					HubsMax = 256 * 16;
		public const int					AnalyzersMax = 32;
		public const int					NewMembersPerRoundMax = 1;
		public const int					MembersRotation = 32;
		public int							TailLength => DevSettings.TailLength100 ? 100 : 1000;
		const int							LoadedRoundsMax = 1000;
		public static readonly Coin			TransactionFeePerByte	= new Coin(0.000001);
		public static readonly Coin			SpaceBasicFeePerByte	= new Coin(0.000001);
		public static readonly Coin			AnalysisFeePerByte		= new Coin(0.000000001);
		public static readonly Coin			AuthorFeePerYear		= new Coin(1);
		public const int					EntityAllocationBaseLength = 100;
		public const int					EntityAllocationYearsMin = 1;
		public const int					EntityAllocationYearsMax = 32;

		public Zone							Zone;
		public McvSettings					Settings;
		public Role							Roles;

		public List<Round>					Tail = new();
		public Dictionary<int, Round>		LoadedRounds = new();

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
		public int							Size => BaseState == null ? 0 : (BaseState.Length + 
																			Accounts.Clusters.Sum(i => i.MainLength) +
																			Authors.Clusters.Sum(i => i.MainLength));
		public Log							Log;
		public BlockDelegate				BlockAdded;
		public JoinDelegate					JoinAdded;
		public ConsensusDelegate			ConsensusConcluded;
		public RoundDelegate				Confirmed;

		public Round						LastConfirmedRound;
		public Round						LastCommittedRound;
		public Round						LastVotedRound		=> Tail.FirstOrDefault(i => i.Voted) ?? LastConfirmedRound;
		public Round						LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
		public Round						LastPayloadRound	=> Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;

		public const string					ChainFamilyName = "Chain";
		public ColumnFamilyHandle			ChainFamily	=> Engine.GetColumnFamily(ChainFamilyName);

		public static int					GetValidityPeriod(int rid) => rid + Pitch;

		public Mcv(Zone zone, Role roles, McvSettings settings, Log log, RocksDb engine)
		{
			Roles = roles&(Role.Base|Role.Chain);
			Zone = zone;
			Settings = settings;
			Log = log;
			Engine = engine;

			Accounts = new (this);
			Authors = new (this);

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
				LastCommittedRound.Members		= r.Read<Member>(m => m.ReadForBase(r)).ToList();
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
							r.ConfirmedMemberJoiners = new[] {Zone.Father0 };
							r.ConfirmedFundJoiners = new[] {Zone.OrgAccount};
						}
	
						//foreach(var p in r.Payloads)
						//	p.Confirmed = true;
	
						if(r.Id > 0)
							r.ConfirmedTime = CalculateTime(r, r.VotesOfTry);
	
						if(i <= 16)
						{
							r.ConfirmedTransactions = r.OrderedTransactions.ToArray();

							Confirm(r, true);

							if(i == 16)
							{
								r.Members[0].HubIPs = new IPAddress[]{zone.GenesisIP};
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
				r.ConfirmedTransactions = r.OrderedTransactions.ToArray();
				r.Hashify(r.Id > 0 ? FindRound(rid - 1).Hash : Zone.Cryptography.ZeroHash);
				r.Write(w);
			}
	
			var b0 = new Vote(this)	{
										RoundId		= 0,
										TimeDelta	= 1,
										ParentSummary	= Zone.Cryptography.ZeroHash,
									};

			var t = new Transaction(Zone){ Id = 0 };
			t.AddOperation(new Emission(Zone.OrgAccount, Web3.Convert.ToWei(512_000, UnitConversion.EthUnit.Ether), 0));
			t.AddOperation(new AuthorBid(Zone.OrgAccount, "uo", null, 1));
			t.Sign(org, gen, 0, Zone.Cryptography.ZeroHash);
			b0.AddNext(t);
			
			foreach(var f in fathers.OrderBy(j => j).ToArray())
			{
				t = new Transaction(Zone) {Id = 0};
				t.AddOperation(new Emission(f, Web3.Convert.ToWei(1000, UnitConversion.EthUnit.Ether), 0));
				t.AddOperation(new CandidacyDeclaration(f, 900_000));
			
				t.Sign(f, gen, 0, Zone.Cryptography.ZeroHash);
			
				b0.AddNext(t);
			}
			
			b0.Sign(gen);
			Add(b0);


			b0.FundJoiners.Add(Zone.OrgAccount);
	
			write(0);
			
			/// UO Autor

			var b1 = new Vote(this)	{
										RoundId		= 1,
										TimeDelta	= ((long)TimeSpan.FromDays(365).TotalMilliseconds + 1),  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
										ParentSummary	= Zone.Cryptography.ZeroHash,
									};
	
			t = new Transaction(Zone){Id = 1};
			t.AddOperation(new AuthorRegistration(org, "uo", "UO", EntityAllocationYearsMax));
			t.Sign(org, gen, 1, Zone.Cryptography.ZeroHash);
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
					b.MemberJoiners.Add(Zone.Father0);
	
				b.Sign(gen);
				Add(b);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		public static Coin CalculateSpaceFee(Coin factor, int basefee, byte years)
		{
			return ((Emission.FactorEnd - factor) / Emission.FactorEnd) * basefee * SpaceBasicFeePerByte * new Coin(1u << (years - 1));
		}

		public void Add(Vote b)
		{
			var r = GetRound(b.RoundId);

			b.Round = r;

			r.Votes.Add(b);
			//r.Blocks = r.Blocks.OrderBy(i => i is Payload p ? p.OrderingKey : new byte[] {}, new BytesComparer()).ToList();
				
			if(b.Transactions.Any())
			{
				foreach(var t in b.Transactions)
				{
					t.Round = r;
					t.Placing = PlacingStage.Placed;
				}
			}
	
			if(r.FirstArrivalTime == DateTime.MaxValue)
			{
				r.FirstArrivalTime = DateTime.UtcNow;
			} 

			BlockAdded?.Invoke(b);

			if(LastConfirmedRound != null)
			{
				r = GetRound(LastConfirmedRound.Id + 1 + Pitch);
		
				if(!r.Voted)
				{
					if(ConsensusReached(r) && r.Parent != null)
					{
						r.Voted = true;
	
						ConsensusConcluded(r, true);
	
						Confirm(r.Parent, false);
	
					}
					else if(ConsensusFailed(r) || (!DevSettings.DisableTimeouts && DateTime.UtcNow - r.FirstArrivalTime > TimeSpan.FromMinutes(5)))
					{
						ConsensusConcluded(r, false);
	
						r.FirstArrivalTime = DateTime.MaxValue;
						r.Try++;
					}
				}
			}
		}

		public void Add(IEnumerable<Vote> bb)
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

		public List<Member> MembersOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Members/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		//public List<Hub> HubsOf(int rid)
		//{
		//	return FindRound(rid - Pitch - 1).Hubs/*.Where(i => i.JoinedAt < r.Id)*/;
		//}

		public List<Analyzer> AnalyzersOf(int rid)
		{
			return FindRound(rid - Pitch - 1).Analyzers/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool ConsensusReached(Round r)
		{
			var m = MembersOf(r.Id);
			
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

		public bool ConsensusFailed(Round r)
		{
			var m = MembersOf(r.Id);

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
		
		public IEnumerable<AccountAddress> ProposeViolators(Round round)
		{
			var g = round.Id > Pitch ? MembersOf(round.Id) : new();
			var gv = round.VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			return gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();
		}

		public IEnumerable<AccountAddress> ProposeMemberJoiners(Round round)
		{
			var o = round.Parent.JoinRequests.Select(jr =>	{
																var a = Accounts.Find(jr.Generator, LastConfirmedRound.Id);
																return new {jr = jr, a = a};
															})	/// round.ParentId - Pitch means to not join earlier than [Pitch] after declaration, and not redeclare after a join is requested
													.Where(i => i.a != null && 
																i.a.CandidacyDeclarationRid <= round.Id - Pitch * 2 &&  /// 2 = declared, requested
																i.a.Bail >= Zone.BailMin)
													.OrderByDescending(i => i.a.Bail)
													.ThenBy(i => i.a.Address)
													.Select(i => i.jr);

			var n = Math.Min(MembersMax - MembersOf(round.Id).Count(), o.Count());

			return o.Take(n).Select(i => i.Generator);
		}

		public IEnumerable<AccountAddress> ProposeMemberLeavers(Round round, AccountAddress generator)
		{
//			var joiners = ProposeGeneratorJoiners(round);

			var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));

			var leavers = MembersOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
															prevs.Count(r => r.VotesOfTry.Any(b => b.Generator == i.Account)) < AliveMinMemberVotes &&	/// sent less than MinVotesPerPitch of required blocks
															!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
												.Select(i => i.Account);
			return leavers;
		}

// 		public IEnumerable<AccountAddress> ProposeHubJoiners(Round round)
// 		{
// 			var o = round.Parent.HubJoinRequests.OrderBy(i => i.Account).Select(i => i.Account);
// 		
// 			var n = Math.Min(HubsMax - HubsOf(round.Id).Count(), o.Count());
// 		
// 			return o.Take(n);
// 		}
// 		
// 		public IEnumerable<AccountAddress> ProposeHubLeavers(Round round, AccountAddress generator)
// 		{
// 			return new AccountAddress[]{};
// 			///var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));
// 			///
// 			///return HubsOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
// 			///									prevs.Count(r => r.HubVoxes.Any(b => b.Account == i.Account)) < AliveMinHubVotes &&	/// sent less than MinVotesPerPitch of required blocks
// 			///									!prevs.Any(r => r.Votes.Any(v => v.Generator == generator && v.HubLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
// 			///						.Select(i => i.Account);
// 		}

		//public IEnumerable<AccountAddress> ProposeAnalyzerJoiners(Round round)
		//{
		//	var o = round.Parent.AnalyzerJoinRequests.OrderBy(i => i.Account).Select(i => i.Account);
		//
		//	var n = Math.Min(AnalyzersMax - AnalyzersOf(round.Id).Count(), o.Count());
		//
		//	return o.Take(n);
		//}
		//
		//public IEnumerable<AccountAddress> ProposeAnalyzerLeavers(Round round, AccountAddress generator)
		//{
		//	var prevs = Enumerable.Range(round.ParentId, Pitch).Select(i => FindRound(i));
		//
		//	return AnalyzersOf(round.Id).Where(i =>	i.JoinedAt <= round.ParentId &&/// in previous Pitch number of rounds
		//											prevs.Count(r => r.AnalyzerVoxes.Any(b => b.Account == i.Account)) < AliveMinAnalyzerVotes &&	/// sent less than MinVotesPerPitch of required blocks
		//											!prevs.Any(r => r.Votes.Any(v => v.Generator == generator && v.AnalyzerLeavers.Contains(i.Account)))) /// not yet reported in prev [Pitch-1] rounds
		//							.Select(i => i.Account);
		//}

		public byte[] Summarize(Round round)
		{
			if(round.Id > LastGenesisRound && !round.Parent.Confirmed)
				return null;

			var g = round.Id > Pitch ? MembersOf(round.Id) : new();
			var gv = round.VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			var gu = gv.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();
			var gf = gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();

			var a = round.Id > Pitch ? AnalyzersOf(round.Id) : new();
			var av = round.AnalyzerVoxes.Where(i => a.Any(j => j.Account == i.Account)).ToArray();
			var au = av.GroupBy(i => i.Account).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();

			var txs = gu.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();

			round.ConfirmedTime = CalculateTime(round, gu);
			
			Execute(round, txs, gf);

			round.ConfirmedTransactions = txs.Where(i => i.Successful).ToArray();

			if(round.Id >= Pitch)
			{
				var gq = g.Count * 2/3;
	
				round.ConfirmedMemberJoiners	= gu.SelectMany(i => i.MemberJoiners).Distinct()
													.Where(x => !round.Members.Any(j => j.Account == x) && gu.Count(b => b.MemberJoiners.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedMemberLeavers	= gu.SelectMany(i => i.MemberLeavers).Distinct()
													.Where(x => round.Members.Any(j => j.Account == x) && gu.Count(b => b.MemberLeavers.Contains(x)) >= gq)
													.OrderBy(i => i).ToArray();

				round.ConfirmedAnalyzerJoiners	= gu.SelectMany(i => i.AnalyzerJoiners).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) == null && gu.Count(b => b.AnalyzerJoiners.Contains(x)) >= Mcv.MembersMax * 2/3)
													.OrderBy(i => i).ToArray();
				
				round.ConfirmedAnalyzerLeavers	= gu.SelectMany(i => i.AnalyzerLeavers).Distinct()
													.Where(x =>	round.Analyzers.Find(a => a.Account == x) != null && gu.Count(b => b.AnalyzerLeavers.Contains(x)) >= Mcv.MembersMax * 2/3)
													.OrderBy(i => i).ToArray();

				round.ConfirmedFundJoiners		= gu.SelectMany(i => i.FundJoiners).Distinct()
													.Where(x => !round.Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Mcv.MembersMax * 2/3)
													.OrderBy(i => i).ToArray();
				
				round.ConfirmedFundLeavers		= gu.SelectMany(i => i.FundLeavers).Distinct()
													.Where(x => round.Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Mcv.MembersMax * 2/3)
													.OrderBy(i => i).ToArray();

				round.ConfirmedViolators		= gu.SelectMany(i => i.Violators).Distinct()
													.Where(x => gu.Count(b => b.Violators.Contains(x)) >= gq)
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

																return new AnalysisConclusion {Resource = i.Resource, Good = (byte)cln, Bad = (byte)inf};
																
															}
															else if(e.AnalysisStage == AnalysisStage.Pending && v.Count() >= a.Count/2)
																return new AnalysisConclusion {Resource = i.Resource, HalfReached = true};
															else
																return null;
														})
											.Where(i => i != null)
											.OrderBy(i => i.Resource).ToArray();
			}

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(round.Id > 0 ? round.Previous.Hash : Zone.Cryptography.ZeroHash);
			
			round.WriteConfirmed(w);

			round.Summary = Zone.Cryptography.Hash(s.ToArray());

			return round.Summary;
		}

		public void Execute(Round round, IEnumerable<Transaction> transactions, IEnumerable<AccountAddress> forkers)
		{
			var prev = round.Previous;
				
			if(round.Id != 0 && prev == null)
				return;

			foreach(var t in transactions)
				foreach(var o in t.Operations)
					o.Error = null;

			start: 

			round.Fees			= 0;
			round.Emission		= round.Id == 0 ? 0						: prev.Emission;
			round.WeiSpent		= round.Id == 0 ? 0						: prev.WeiSpent;
			round.Factor		= round.Id == 0 ? Emission.FactorStart	: prev.Factor;
			round.Members		= round.Id == 0 ? new()					: prev.Members.ToList();
			round.Analyzers		= round.Id == 0 ? new()					: prev.Analyzers.ToList();
			round.Funds			= round.Id == 0 ? new()					: prev.Funds.ToList();

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();

			foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
			{
				//Coin fee = 0;

				var a = round.AffectAccount(t.Signer);

				if(t.Id != a.LastTransactionId + 1)
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

					var f = o.CalculateTransactionFee(round.Factor);
	
					if(a.Balance - f < 0)
					{
						o.Error = Operation.NotEnoughUNT;
						goto start;
					}

					round.Fees += f;
					a.Balance -= f;
				}

				a.LastTransactionId++;
						
				if(Roles.HasFlag(Role.Chain))
				{
					round.AffectAccount(t.Signer).Transactions.Add(round.Id);
				}

				round.Distribute(round.Fees, round.Members.Select(i => i.Account), 9, round.Funds, 1); /// taking 10% we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding
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

					round.Distribute(penalty, round.Members.Where(i => !forkers.Contains(i.Account)).Select(i => i.Account), 1, round.Funds, 1);
				}
			}
		}

		public void Hashify()
		{
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var i in Accounts.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Authors.SuperClusters.OrderBy(i => i.Key))			BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
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
				var cm = MembersOf(c.Id);
				var s = c.Unique.Where(i => cm.Any(j => j.Account == i.Generator)).GroupBy(i => i.ParentSummary, new BytesEqualityComparer()).MaxBy(i => i.Count()).Key;
	 		
				if(!s.SequenceEqual(round.Summary))
				{
					throw new ConfirmationException("Can't confirm", round);
				}
			}
			else
			{
				///round.Blocks = round.ConfirmedPayloads.ToList();
				Execute(round, round.ConfirmedTransactions, round.ConfirmedViolators);
			}
								
			foreach(var t in round.OrderedTransactions)
			{
				t.Placing = round.ConfirmedTransactions.Contains(t) ? PlacingStage.Confirmed : PlacingStage.FailedOrNotFound;

				foreach(var o in t.Operations)
				{
					#if DEBUG
					if(o.__ExpectedPlacing > PlacingStage.Placed && t.Placing != o.__ExpectedPlacing)
					{
						Debugger.Break();
					}
					#endif
				}
			}
			
			round.Members.RemoveAll(i => round.ConfirmedViolators.Contains(i.Account));

			round.Members.AddRange(round.ConfirmedMemberJoiners.Where(i => Accounts.Find(i, round.Id).CandidacyDeclarationRid <= round.Id - Pitch * 2)
																.Select(i => new Member {Account = i, JoinedAt = round.Id + Pitch + 1}));
			round.Members.RemoveAll(i => round.AnyOperation(o => o is CandidacyDeclaration d && d.Signer == i.Account && o.Transaction.Placing == PlacingStage.Confirmed));  /// CandidacyDeclaration cancels membership
			round.Members.RemoveAll(i => round.AffectedAccounts.ContainsKey(i.Account) && round.AffectedAccounts[i.Account].Bail < Zone.BailMin);  /// if Bail has exhausted due to penalties (CURRENTY NOT APPLICABLE, penalties are disabled)
			round.Members.RemoveAll(i => round.ConfirmedMemberLeavers.Contains(i.Account));

			//round.Hubs.RemoveAll(i => round.ConfirmedHubLeavers.Contains(i.Account));
			//round.Hubs.AddRange(round.ConfirmedHubJoiners.Select(i => new Hub {Account = i, JoinedAt = round.Id + Pitch + 1}));

			round.Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));
			round.Funds.AddRange(round.ConfirmedFundJoiners);

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

			round.Analyzers.RemoveAll(i => round.ConfirmedAnalyzerLeavers.Contains(i.Account));
			round.Analyzers.AddRange(round.ConfirmedAnalyzerJoiners.Select(i => new Analyzer {Account = i, JoinedAt = round.Id + Pitch + 1}));
			
			using(var b = new WriteBatch())
			{
				if(Tail.Count(i => i.Id < round.Id) >= TailLength)
				{
					var tail = Tail.AsEnumerable().Reverse().Take(TailLength);
	
					foreach(var i in tail)
					{
						Accounts.Save(b, i.AffectedAccounts.Values);
						Authors	.Save(b, i.AffectedAuthors.Values);
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
					w.Write(LastCommittedRound.Members, i => i.WriteForBase(w));
					w.Write(LastCommittedRound.Analyzers, i => i.WriteForBase(w));
					w.Write(LastCommittedRound.Funds);
	
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
					cjr.Votes.RemoveAll(i => i is not Vote v || !v.Transactions.Any());
				}
			}

			//round.JoinRequests.RemoveAll(i => i.RoundId < round.Id - Pitch);
		
			round.Confirmed = true;
			LastConfirmedRound = round;

			Confirmed?.Invoke(round);
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

		public IEnumerable<Resource> QueryRelease(string query)
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
