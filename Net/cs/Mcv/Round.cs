using System.Diagnostics;

namespace Uccs.Net
{
	public abstract class Round : IBinarySerializable
	{
		public int											Id;
		public int											ParentId	=> Id - Mcv.P;
		public int											VotersId	=> Id - Mcv.JoinToVote;
		public Round										VotersRound	=> Mcv.FindRound(VotersId);
		public Round										Previous	=> Mcv.FindRound(Id - 1);
		public Round										Next		=> Mcv.FindRound(Id + 1);
		public Round										Parent		=> Mcv.FindRound(ParentId);
		public Round										Child		=> Mcv.FindRound(Id + Mcv.P);
		public long											PerVoteTransactionsLimit		=> Mcv.Zone.TransactionsPerRoundAbsoluteLimit / Members.Count;
		public long											PerVoteOperationsLimit			=> Mcv.Zone.ExecutionsCyclesPerRoundMaximum / Members.Count;
		public long											PerVoteBandwidthAllocationLimit	=> Mcv.Zone.BandwidthAllocationPerRoundMaximum / Members.Count;

		public bool											IsLastInCommit => (Id + 1) % Zone.CommitLength == 0; ///Tail.Count(i => i.Id <= round.Id) >= Zone.CommitLength; 

		public int											Try = 0;
		public DateTime										FirstArrivalTime = DateTime.MaxValue;

		public IEnumerable<Generator>						Voters => Mcv.FindRound(VotersId).Members.Take(Mcv.VotesMaximum);

		public List<Vote>									Votes = new();
		public List<AccountAddress>							Forkers = new();
		public IEnumerable<Vote>							VotesOfTry => Votes.Where(i => i.Try == Try);
		public IEnumerable<Vote>							Payloads => VotesOfTry.Where(i => i.Transactions.Any());
		public IEnumerable<Vote>							Selected
															{
																get
																{
																	var vr = Mcv.FindRound(VotersId);

																	return VotesOfTry.OrderByHash(i => i.Generator.Bytes, vr.Hash).Take(Mcv.VotesMaximum);
																}
															}
		public IGrouping<byte[], Vote>						MajorityByParentHash => Selected.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

		public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
		public IEnumerable<Transaction>						Transactions => Confirmed ? ConsensusTransactions : OrderedTransactions;

		public Time											ConsensusTime;
		public Transaction[]								ConsensusTransactions = {};
		public EntityId[]									ConsensusMemberLeavers = {};
		public EntityId[]									ConsensusViolators = {};
		public AccountAddress[]								ConsensusFundJoiners = {};
		public AccountAddress[]								ConsensusFundLeavers = {};
		public long											ConsensusExecutionFee;
		public int											ConsensusTransactionsOverflowRound;

		public bool											Confirmed = false;
		public byte[]										Hash;

		public Dictionary<AccountEntry, long>				BYRewards = [];
		public Dictionary<AccountEntry, long>				ECRewards = [];
		public List<Generator>								Candidates = new();
		public List<Generator>								Members = new();
		public List<AccountAddress>							Funds;
#if ETHEREUM
		public List<Immission>								Emissions;
#endif
		public long[]										NextBandwidthAllocations = [];
		//public long										PreviousDayBaseSize;

		public Dictionary<byte[], int>						NextAccountIds;
		public Dictionary<byte[], int>						NextDomainIds;
		public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
		public Dictionary<EntityId, Generator>				AffectedCandidates = new();

		public Mcv											Mcv;
		public McvZone										Zone => Mcv.Zone;

		public abstract long								AccountAllocationFee(Account account);
		public virtual void									CopyConfirmed(){}
		public virtual void									RegisterForeign(Operation o){}
		public virtual void									ConfirmForeign(){}

		public int											MinimumForConsensus => VotesMinimumOf(Voters.Count());

		public bool ConsensusReached
		{
			get
			{ 
				if(VotesOfTry.Count() < MinimumForConsensus)
					return false;

				var voters = Voters.ToArray();
			
				return MajorityByParentHash.Count(i => voters.Any(v => v.Address == i.Generator)) >= MinimumForConsensus;
			}
		}

		public Round(Mcv c)
		{
			Mcv = c;
		}

		public override string ToString()
		{
			return $"Id={Id}, VoT/P={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), Members={Members?.Count}, ConfirmedTime={ConsensusTime}, {(Confirmed ? "Confirmed, " : "")}Hash={Hash?.ToHex()}";
		}

		public static int VotesMinimumOf(int n)
		{
			if(n == 1)	return 1;
			if(n == 2)	return 2;
			if(n == 4)	return 3;
		
			return Math.Min(n, Mcv.VotesMaximum) * 2/3;
		}

		public virtual IEnumerable<object> AffectedByTable(TableBase table)
		{
			throw new IntegrityException();
		}

		public AccountEntry AffectAccount(AccountAddress account)
		{
			if(AffectedAccounts.TryGetValue(account, out AccountEntry a))
				return a;
			
			var e = Mcv.Accounts.Find(account, Id - 1);	

			if(e != null)
				a = AffectedAccounts[account] = e.Clone();
			else
			{
				var ci = Mcv.Accounts.KeyToCluster(account).ToArray();
				var c = Mcv.Accounts.Clusters.FirstOrDefault(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextAccountIds[ci] = 0;
				else
					NextAccountIds[ci] = c.NextEntityId;
				
				ai = NextAccountIds[ci]++;

				a = AffectedAccounts[account] = new AccountEntry(Mcv) {	Id = new EntityId(ci, ai), 
																		Address = account,
																		New = true};
			}

			return a;
		}

		public Generator AffectCandidate(EntityId id)
		{
			if(AffectedCandidates.TryGetValue(id, out Generator a))
				return a;

			if(Id > 0 && Candidates == Previous.Candidates)
			{
				Candidates = Previous.Candidates.ToList();
			}

			var c = Candidates.Find(i => i.Id == id);

			if(c == null)
			{
				c = AffectedCandidates[id] = Mcv.CreateGenerator();
				Candidates.Add(c);
			}
			else
			{
				Candidates.Remove(c);
				c = AffectedCandidates[id] = c.Clone();
				Candidates.Add(c);
			}

			return c;
		}

		public virtual void Elect(Vote[] votes, int gq)
		{
		}

		public byte[] Summarize()
		{
			var voters = Id < Mcv.JoinToVote ? [] : Voters;
			var min = VotesMinimumOf(voters.Count());
			var all = VotesOfTry.ToArray();
			var s = Id < Mcv.JoinToVote ? [] : Selected.ToArray();
						
			ConsensusExecutionFee				= Id == 0 ? 0 : (Previous.Id == 0 ? 1 : Previous.ConsensusExecutionFee);
			ConsensusTransactionsOverflowRound	= Id == 0 ? 0 : Previous.ConsensusTransactionsOverflowRound;

			var tn = all.Sum(i => i.Transactions.Length);

			if(tn > Mcv.Zone.TransactionsPerRoundExecutionLimit)
			{
				ConsensusExecutionFee *= Mcv.Zone.TransactionsOverflowFeeFactor;
				ConsensusTransactionsOverflowRound = Id;

				var e = tn - Mcv.Zone.TransactionsPerRoundExecutionLimit;

				var gi = all.AsEnumerable().GetEnumerator();

				do
				{
					if(!gi.MoveNext())
						gi.Reset();
					
					if(gi.Current.Transactions.Length > PerVoteTransactionsLimit)
					{
						e--;
						gi.Current.TransactionCountExcess++;
					}
				}
				while(e > 0);

				foreach(var i in all.Where(i => i.TransactionCountExcess > 0))
				{
					var ts = new Transaction[i.Transactions.Length - i.TransactionCountExcess];
					Array.Copy(i.Transactions, i.TransactionCountExcess, ts, 0, ts.Length);
					i.Transactions = ts;
				}
			}
			else 
			{
				if(ConsensusExecutionFee > 1 && Id - ConsensusTransactionsOverflowRound > Mcv.P)
					ConsensusExecutionFee /= Zone.TransactionsOverflowFeeFactor;
			}
			
			if(Id > 0)
			{
				var t = all.GroupBy(x => x.Time).MaxBy(i => i.Count());
	
				if(t.Count() >= min && t.Key > Previous.ConsensusTime)
					ConsensusTime = t.Key;
				else
					ConsensusTime = Previous.ConsensusTime;
			}

			var txs = all.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();

			Execute(txs);

			ConsensusTransactions = txs.Where(i => i.Successful).ToArray();

			if(Id >= Mcv.P)
			{
				ConsensusMemberLeavers = s.SelectMany(i => i.MemberLeavers).Distinct()
										  .Where(x => Members.Any(j => j.Id == x) && s.Count(b => b.MemberLeavers.Contains(x)) >= min)
										  .Order().ToArray();

				ConsensusViolators = s.SelectMany(i => i.Violators).Distinct()
									  .Where(x => s.Count(b => b.Violators.Contains(x)) >= min)
									  .Order().ToArray();

				//ConsensusFundJoiners	= gu.SelectMany(i => i.FundJoiners).Distinct()
				//							.Where(x => !Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
				//							.Order().ToArray();
				//
				//ConsensusFundLeavers	= gu.SelectMany(i => i.FundLeavers).Distinct()
				//							.Where(x => Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
				//							.Order().ToArray();
				//
				Elect(s, min);
			}

			Hashify(); /// depends on Mcv.BaseHash 

			return Hash;
		}
		
		public IEnumerable<EntityId> ProposeViolators()
		{
			//var g = Id > Mcv.P ? Voters : [];
			//var gv = VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			//
			//return gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key);

			return Forkers.Select(i => Mcv.Accounts.Find(i, Previous.Id).Id);
		}

		public IEnumerable<EntityId> ProposeMemberLeavers(AccountAddress generator)
		{
			var prevs = Enumerable.Range(ParentId - Mcv.P, Mcv.P).Select(Mcv.FindRound);

			var l = Parent.Voters.Where(i => !Parent.VotesOfTry.Any(v => v.Generator == i.Address) && /// did not sent a vote
											!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Id)))) /// not yet proposed in prev [Pitch-1] rounds
								.Select(i => i.Id);

// 			if(l.Any())
// 			{
// 				Debugger.Break();
// 			}

			return l;
		}

		public virtual void InitializeExecution()
		{
		}

		public virtual void RestartExecution()
		{
		}

		public virtual void FinishExecution()
		{
		}

		public void Execute(IEnumerable<Transaction> transactions, bool trying = false)
		{
			if(Confirmed)
				throw new IntegrityException();
	
			if(Id != 0 && Previous == null)
				return;

			foreach(var t in transactions)
				foreach(var o in t.Operations)
					o.Error = null;

			Candidates					= Id == 0 ? new()																					: Previous.Candidates;
			Members						= Id == 0 ? new()																					: Previous.Members;
			Funds						= Id == 0 ? new()																					: Previous.Funds;
			NextBandwidthAllocations	= Id == 0 ? Enumerable.Range(0, Zone.BandwidthAllocationDaysMaximum + 1).Select(i => 0L).ToArray()	: Previous.NextBandwidthAllocations.ToArray();

			#if IMMISSION
			Emissions			= Id == 0 ? new()								: Previous.Emissions;
			#endif
			///RentPerBytePerDay	= Id == 0 ? Mcv.Zone.RentPerBytePerDayMinimum	: Previous.RentPerBytePerDay;

			InitializeExecution();

		start: 
			#if IMMISSION
			//Emission		= Id == 0 ? 0 : Previous.Emission;
			#endif
			NextAccountIds	= new (Bytes.EqualityComparer);
			NextDomainIds	= new (Bytes.EqualityComparer);

			BYRewards.Clear();
			ECRewards.Clear();
			AffectedCandidates.Clear();
			AffectedAccounts.Clear();

			RestartExecution();

			foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
			{
				var s = AffectAccount(t.Signer);

				if(t.Nid != s.LastTransactionNid + 1)
				{
					foreach(var o in t.Operations)
						o.Error = Operation.NotSequential;
					
					goto start;
				}

				t.ECSpent = 0;
				//t.ECReward = 0;
				t.BYReward = 0;

				foreach(var o in t.Operations)
				{
					o.Signer = s;
					t.ECSpent += ConsensusExecutionFee;

					o.Execute(Mcv, this);

					if(o.Error != null)
						goto start;

					#if IMMISION
					if(o is not Immission)
					{
						f += o.ExeUnits * ConsensusExeunitFee;
					}
					#endif
					
					if(t.ECFee == 0 && s.BandwidthExpiration >= ConsensusTime)
					{
						if(s.BandwidthTodayTime < ConsensusTime) /// switch to this day
						{	
							s.BandwidthTodayTime		= ConsensusTime;
							s.BandwidthTodayAvailable	= s.BandwidthNext;
						}

						s.BandwidthTodayAvailable -= t.ECSpent;

						if(s.BandwidthTodayAvailable < 0)
						{
							o.Error = Operation.NotEnoughEC;
							goto start;
						}
					}
					else if(s.GetECBalance(ConsensusTime) < t.ECSpent || (!trying && (s.GetECBalance(ConsensusTime) < t.ECFee || t.ECSpent > t.ECFee)))
					{
						o.Error = Operation.NotEnoughEC;
						goto start;
					}
					
					if(s.BYBalance < 0)
					{
						o.Error = Operation.NotEnoughBY;
						goto start;
					}
				}

				if(t.Generator.Ei != -1)
				{
					var g = Mcv.Accounts.Find(t.Generator, Id);

					if(BYRewards.TryGetValue(g, out var x))
						BYRewards[g] = x + t.BYReward;
					else
						BYRewards[g] = t.BYReward;

					//if(ECRewards.TryGetValue(g, out x))
					//	ECRewards[g] = x + t.ECFee + t.ECReward;
					//else
					//	ECRewards[g] = t.ECFee + t.ECReward;
				}

				//s.STBalance -= t.STReward;
				s.ECBalanceSubtract(ConsensusTime, t.ECFee);
				s.LastTransactionNid++;
						
				if(Mcv.Settings.Base?.Chain != null)
				{
					s.Transactions.Add(Id);
				}
			}

			FinishExecution();
		}

		public void Confirm()
		{
			if(Mcv.Node != null)
				if(!Monitor.IsEntered(Mcv.Lock))
					Debugger.Break();

			if(Confirmed)
				throw new IntegrityException();

			if(Id > 0 && Mcv.LastConfirmedRound != null && Mcv.LastConfirmedRound.Id + 1 != Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == Id");

			Execute(ConsensusTransactions);

			///Last365BaseDeltas		= Id == 0 ? Enumerable.Range(0, Time.FromYears(1).Days).Select(i => 0L).ToList() : Previous.Last365BaseDeltas.ToList();
			///PreviousDayBaseSize		= Id == 0 ? 0 : Previous.PreviousDayBaseSize;
			//Candidates			= Candidates.ToList();
			Members				= Members.ToList();
			Funds				= Funds.ToList();
			#if IMMISION
			Emissions			= Emissions.ToList();
			#endif

			CopyConfirmed();
			
			foreach(var t in ConsensusTransactions)
			{
				foreach(var o in t.Operations)
				{
#if ETHEREUM
					if(o is Immission e)
					{
						e.Generator = e.Transaction.Generator;
						Emissions.Add(e);
					}
#endif

					RegisterForeign(o);
				}
			}

			ConfirmForeign();
	
			foreach(var t in OrderedTransactions)
			{
				t.Status = ConsensusTransactions.Contains(t) ? TransactionStatus.Confirmed : TransactionStatus.FailedOrNotFound;

				#if DEBUG
				//if(t.__ExpectedPlacing > PlacingStage.Placed && t.Placing != t.__ExpectedPlacing)
				//{
				//	Debugger.Break();
				//}
				#endif
			}

			foreach(var i in ConsensusViolators.Select(i => Members.Find(j => j.Id == i)))
			{
				AffectAccount(i.Address).AverageUptime = 0;
				Members.Remove(i);
			}

			foreach(var i in ConsensusMemberLeavers.Select(i => Members.Find(j => j.Id == i)))
			{
				var a = AffectAccount(i.Address);
				//a.MRBalance += i.Pledge;
				a.AverageUptime = (a.AverageUptime + Id - i.CastingSince)/(a.AverageUptime == 0 ? 1 : 2);
				Members.Remove(i);
			}

// 			var cds = ConsensusTransactions	.Where(i => !ConsensusViolators.Contains(AffectedAccounts[i.Signer].Id) && !ConsensusMemberLeavers.Contains(AffectedAccounts[i.Signer].Id))
// 											.SelectMany(i => i.Operations)
// 											.OfType<CandidacyDeclaration>()
// 											.ToList();
// 
// 			foreach(var i in cds.GroupBy(i => i.Transaction.Signer)
// 								.Select(i => i.MaxBy(i => i.Pledge))
// 								.OrderByDescending(i => i.Signer.AverageUptime)
// 								.ThenBy(i => i.Pledge)
// 								.ThenBy(i => i.Transaction.Signer)
// 								.Take(Mcv.Zone.MembersLimit - Members.Count))
// 			{
// 
// 				Members.Add(Mcv.CreateMember(this, i));
// 				cds.Remove(i);
// 			}

// 			foreach(var i in Candidates	.Where(i => i.Registered == ConsensusTime)
// 										.Select(i => Mcv.Accounts.Find(i.Id, Id))
// 										.OrderByHash(i => i.Address.Bytes, Hash)
// 										.ToArray())
// 			{
// 				var a = AffectAccount(i.Address);
// 					
// 				if(a.GetECBalance(ConsensusTime) >= Zone.JoinCost)
// 				{
// 					a.ECBalanceSubtract(Zone.JoinCost);
// 
// 					var c = AffectCandidate(i.Id);
// 
// 					c.CastingSince = Id + Mcv.JoinToVote;
// 						
// 					Candidates.Remove(c);
// 					Members.Add(c);
// 				}
// 				else
// 				{
// 					var c = AffectCandidate(i.Id);
// 					Candidates.Remove(c);
// 				}
// 
// 				if(Members.Count == Zone.MembersLimit)
// 					break;
// 			}

			foreach(var i in Candidates	.Where(i => i.Registered == ConsensusTime)
										.OrderByHash(i => i.Address.Bytes, Hash)
										.Take(Mcv.Zone.MembersLimit - Members.Count).ToArray())
			{
				var c = AffectCandidate(i.Id);
				
				c.CastingSince = Id + Mcv.JoinToVote;
				
				Candidates.Remove(c);
				Members.Add(c);
			}

			Candidates.RemoveAll(i => i.Registered != ConsensusTime);
			Members = Members.OrderByHash(i => i.Address.Bytes, Hash).ToList();

// 			foreach(var i in cds) /// refund the rest
// 			{
// 				AffectAccount(i.Transaction.Signer).MRBalance += i.Pledge;
// 			}

			Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
			Funds.AddRange(ConsensusFundJoiners);
			
			if(IsLastInCommit)
			{
				///var tail = Mcv.Tail.AsEnumerable().Reverse().Take(Mcv.Zone.CommitLength);
				///
				///var members = Members.Where(i => i.CastingSince <= tail.First().Id).Select(i => AffectAccount(i.Account)).ToArray();
				///
				///long distribute(Func<Round, Dictionary<AccountEntry, long>> getroundrewards, Action<AccountEntry, long> credit)
				///{
				///	long a = 0;
				///
				///	foreach(var r in tail)
				///	{
				///		foreach(var rmr in getroundrewards(r))
				///		{
				///			credit(AffectAccount(rmr.Key.Address), rmr.Value * 50/100); /// 50% to a member itself
				///			a += rmr.Value;
				///		}
				///	}
				///	
				///	foreach(var j in members)
				///	{
				///		credit(j, a*40/100/members.Length); /// 40% evenly among all
				///	}
				///
				///	if(Funds.Any())
				///	{
				///		foreach(var i in Funds)
				///		{
				///			credit(AffectAccount(i), a/10/Funds.Count); /// 10% to funds
				///		}
				///	}
				///
				///	return a;
				///}
				///
				///var by = distribute(r => r.BYRewards, (a, r) => a.BYBalance += r);
				///var ec = distribute(r => r.ECRewards, (a, r) => a.ECBalance += r);
				///
				///foreach(var i in members)
				///	i.BYBalance += Zone.BYCommitEmission/members.Length;
				///
				///if(ec < Zone.OperationsPerRoundLimit)
				///	foreach(var i in members)
				///		i.ECBalance += Zone.ECCommitEmission/members.Length;
				///
				///foreach(var j in members)
				///	j.MRBalance += Zone.MRCommitEmission/members.Length;
			}

			if(Id > 0 && ConsensusTime != Previous.ConsensusTime)
			{
				NextBandwidthAllocations = NextBandwidthAllocations.Skip(1).Append(0).ToArray();

				foreach(var i in Members./*Where(i => i.CastingSince <= tail.First().Id).*/Select(i => AffectAccount(i.Address)))
				{
					i.ECBalanceAdd(new ExecutionReservation (ConsensusTime + Zone.ECLifetime, Zone.ECDayEmission / Members.Count));
					i.BYBalance += Zone.BYDayEmission / Members.Count;
				}

				///long s = Mcv.Size;
				///
				///foreach(var t in Mcv.Tables)
				///{
				///	s += t.MeasureChanges(Mcv.Tail.SkipWhile(i => i != this));
				///}
				///
				///Last365BaseDeltas.RemoveAt(0);
				///Last365BaseDeltas.Add(s - PreviousDayBaseSize);
				///
				///if(Last365BaseDeltas.Sum() > Mcv.Zone.TargetBaseGrowthPerYear)
				///{
				///	RentPerBytePerDay = Mcv.Zone.RentPerBytePerDayMinimum * Last365BaseDeltas.Sum() / Mcv.Zone.TargetBaseGrowthPerYear;
				///}
				///
				///PreviousDayBaseSize = s;
				///
			}
			
			Confirmed = true;
			Mcv.LastConfirmedRound = this;
		}

		public void Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Mcv.BaseHash);
			w.Write(Id > 0 ? Previous.Hash : Mcv.Zone.Cryptography.ZeroHash);
			WriteConfirmed(w);

			Hash = Cryptography.Hash(s.ToArray());
		}

		public virtual void WriteBaseState(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Id);
			writer.Write(Hash);
			writer.Write(ConsensusTime);
			writer.Write7BitEncodedInt64(ConsensusExecutionFee);
			writer.Write7BitEncodedInt(ConsensusTransactionsOverflowRound);
			
			///writer.Write(RentPerBytePerDay);
			///writer.Write7BitEncodedInt64(PreviousDayBaseSize);
			///writer.Write(Last365BaseDeltas, writer.Write7BitEncodedInt64);
			writer.Write(NextBandwidthAllocations, writer.Write7BitEncodedInt64);
			
			//writer.Write(Emission);
			writer.Write(Funds);
			#if ETHEREUM
			writer.Write(Emissions, i => i.WriteBaseState(writer));
			#endif
		}

		public virtual void ReadBaseState(BinaryReader reader)
		{
			Id									= reader.Read7BitEncodedInt();
			Hash								= reader.ReadHash();
			ConsensusTime						= reader.Read<Time>();
			ConsensusExecutionFee				= reader.Read7BitEncodedInt64();
			ConsensusTransactionsOverflowRound	= reader.Read7BitEncodedInt();
			
			//RentPerBytePerDay		= reader.Read<Money>();
			//PreviousDayBaseSize	= reader.Read7BitEncodedInt64();
			//Last365BaseDeltas		= reader.ReadList(() => reader.Read7BitEncodedInt64());
			NextBandwidthAllocations			= reader.ReadArray(reader.Read7BitEncodedInt64);
			
			//Emission							= reader.Read<Money>();
			Funds								= reader.ReadList<AccountAddress>();

			#if ETHEREUM
			Emissions							= reader.Read<Immission>(m => m.ReadBaseState(reader)).ToList();
			#endif
		}

		public virtual void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConsensusTime);
			writer.Write7BitEncodedInt64(ConsensusExecutionFee);
			writer.Write7BitEncodedInt(ConsensusTransactionsOverflowRound);
			writer.Write(ConsensusMemberLeavers);
			writer.Write(ConsensusViolators);
			writer.Write(ConsensusFundJoiners);
			writer.Write(ConsensusFundLeavers);
			writer.Write(ConsensusTransactions, i => i.WriteConfirmed(writer));
		}

		public virtual void ReadConfirmed(BinaryReader reader)
		{
			ConsensusTime						= reader.Read<Time>();
			ConsensusExecutionFee				= reader.Read7BitEncodedInt64();
			ConsensusTransactionsOverflowRound	= reader.Read7BitEncodedInt();
			ConsensusMemberLeavers				= reader.ReadArray<EntityId>();
			ConsensusViolators					= reader.ReadArray<EntityId>();
			ConsensusFundJoiners				= reader.ReadArray<AccountAddress>();
			ConsensusFundLeavers				= reader.ReadArray<AccountAddress>();
			ConsensusTransactions				= reader.Read(() =>	new Transaction {Mcv = Mcv, Round = this}, t => t.ReadConfirmed(reader)).ToArray();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Id);
			w.Write(Confirmed);
			
			if(Confirmed)
			{
// #if DEBUG
// 				w.Write(Hash);
// #endif
				WriteConfirmed(w);
				w.Write(Hash);
				//w.Write(JoinRequests, i => i.Write(w)); /// for [LastConfimed-Pitch..LastConfimed]
			} 
			else
			{
				w.Write(Votes, i => {
										i.WriteForRoundUnconfirmed(w); 
									 });
			}
		}

		public void Read(BinaryReader r)
		{
			Id			= r.Read7BitEncodedInt();
			Confirmed	= r.ReadBoolean();
			
			if(Confirmed)
			{
// #if DEBUG
// 				Hash = r.ReadSha3();
// #endif
				ReadConfirmed(r);
				Hash = r.ReadHash();
				//JoinRequests.AddRange(r.ReadArray(() =>	{
				//											var b = new MemberJoinOperation();
				//											b.RoundId = Id;
				//											b.Read(r, Mcv.Zone);
				//											return b;
				//										}));
			} 
			else
			{
				Votes = r.ReadList(() => {
											var v = Mcv.CreateVote();
											v.RoundId = Id;
											v.Round = this;
											v.ReadForRoundUnconfirmed(r);
												
											foreach(var i in v.Transactions)
											{
												i.Mcv = Mcv;
												i.Zone = Mcv.Zone;
												i.Round = this;
											}

											return v;
										 });
			}
		}

		public void Save(BinaryWriter w)
		{
			WriteConfirmed(w);
			
			w.Write(Hash);
		}

		public void Load(BinaryReader r)
		{
			ReadConfirmed(r);

			Hash = r.ReadHash();
		}
	}
}
