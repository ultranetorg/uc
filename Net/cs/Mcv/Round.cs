using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public abstract class Round : IBinarySerializable
	{
		public int											Id;
		public int											ParentId => Id - Mcv.P;
		public int											VotersRound => Id - Mcv.DeclareToGenerateDelay;
		public Round										Previous =>	Mcv.FindRound(Id - 1);
		public Round										Next =>	Mcv.FindRound(Id + 1);
		public Round										Parent => Mcv.FindRound(ParentId);
		public Round										Child => Mcv.FindRound(Id + Mcv.P);
		public int											TransactionsPerVoteExecutionLimit		=> Mcv.Zone.TransactionsPerRoundLimit / Members.Count;
		public int											TransactionsPerVoteAllowableOverflow	=> TransactionsPerVoteExecutionLimit * Mcv.Zone.TransactionsPerVoteAllowableOverflowMuliplier;
		public int											OperationsPerVoteLimit					=> Mcv.Zone.OperationsPerRoundLimit / Members.Count;

		public int											Try = 0;
		public DateTime										FirstArrivalTime = DateTime.MaxValue;

		public List<Vote>									Votes = new();
		public IEnumerable<Vote>							VotesOfTry => Votes.Where(i => i.Try == Try);
		public IEnumerable<Vote>							Payloads => VotesOfTry.Where(i => i.Transactions.Any());
		public IEnumerable<Vote>							Eligible 
															{ 
																get 
																{ 
																	var v = Mcv.VotersOf(this);
																	return VotesOfTry.Where(i => v.Any(j => j.Account == i.Generator)).GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
																} 
															}
		public IGrouping<byte[], Vote>						Majority => Eligible.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

		public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
		public IEnumerable<Transaction>						Transactions => Confirmed ? ConsensusTransactions : OrderedTransactions;

		public Time											ConsensusTime;
		public Transaction[]								ConsensusTransactions = {};
		public AccountAddress[]								ConsensusMemberLeavers = {};
		public AccountAddress[]								ConsensusFundJoiners = {};
		public AccountAddress[]								ConsensusFundLeavers = {};
		public AccountAddress[]								ConsensusViolators = {};
		public Money										ConsensusExeunitFee;
		public int											ConsensusTransactionsOverflowRound;

		public bool											Confirmed = false;
		public byte[]										Hash;

		public Dictionary<AccountEntry, Money>				Rewards = [];
		public Money										Emission;
		public Money										RentPerBytePerDay;
		//public Money										RentPerEntityPerDay => RentPerBytePerDay * Mcv.EntityLength;
		public List<Member>									Members = new();
		public List<AccountAddress>							Funds;
#if ETHEREUM
		public List<Immission>								Emissions;
#endif
		public Dictionary<byte[], int>						NextAccountIds;
		public Dictionary<byte[], int>						NextDomainIds;
		public List<long>									Last365BaseDeltas;
		public long											PreviousDayBaseSize;

		public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();

		public Mcv											Mcv;
		public McvZone										Zone => Mcv.Zone;

		public abstract										Money AccountAllocationFee(Account account);

		public int RequiredVotes
		{
			get
			{ 
				var m = Mcv.VotersOf(this);

				int q;

				if(m.Count() == 1)		q = 1;
				else if(m.Count() == 2)	q = 2;
				else if(m.Count() == 4)	q = 3;
				else
					q = m.Count() * 2 / 3;

				return q;
			}
		}

		public bool ConsensusReached
		{
			get
			{ 
				int q = RequiredVotes;

				if(Eligible.Count() < q)
					return false;

				return Majority.Count() >= q;
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

		public virtual IEnumerable<object> AffectedByTable(TableBase table)
		{
			throw new IntegrityException();
		}

		public void Distribute(Money amount, IEnumerable<AccountAddress> a)
		{
			if(a.Any())
			{
				var x = amount/a.Count();
	
				foreach(var i in a.Skip(1))
					AffectAccount(i).Balance += x;
	
				AffectAccount(a.First()).Balance += amount - (x * (a.Count() - 1));
			}
		}

		public void Distribute(Dictionary<AccountEntry, Money> a, int ashare, IEnumerable<AccountAddress> b, int bshare)
		{
			var total = a.SumMoney(i => i.Value);
			var s = total * ashare/(ashare + bshare);

			if(a.Any())
			{
				var x = s/a.Count();
		
				foreach(var i in a)
				{
					AffectAccount(i.Key.Key).Balance += x;
				}
			}

			if(b.Any())
			{
				var x = (total - s)/b.Count();
		
				foreach(var i in b)
				{
					AffectAccount(i).Balance += x;
				}
			}
		}

		public AccountEntry AffectAccount(AccountAddress account)
		{
			if(AffectedAccounts.TryGetValue(account, out AccountEntry a))
				return a;
			
			var e = Mcv.Accounts.Find(account, Id - 1);	

			if(e != null)
				return AffectedAccounts[account] = e.Clone();
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

				return AffectedAccounts[account] = new AccountEntry(Mcv) {	Id = new EntityId(ci, ai), 
																			Address = account,
																			New = true};
			}
		}

		public virtual void Elect(Vote[] votes, int gq)
		{
		}

		public byte[] Summarize()
		{
			var m = Id >= Mcv.DeclareToGenerateDelay ? Mcv.VotersOf(this) : new();
			var gq = m.Count * 2/3;
			var gv = VotesOfTry.Where(i => m.Any(j => i.Generator == j.Account)).ToArray();
			var gu = gv.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First()).ToArray();
			var gf = gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();
						
			ConsensusExeunitFee					= Id == 0 ? Zone.ExeunitMinFee	: Previous.ConsensusExeunitFee;
			ConsensusTransactionsOverflowRound	= Id == 0 ? 0					: Previous.ConsensusTransactionsOverflowRound;

			var tn = gu.Sum(i => i.Transactions.Length);

			if(tn > Mcv.Zone.TransactionsPerRoundLimit)
			{
				ConsensusExeunitFee *= Mcv.Zone.TransactionsFeeOverflowFactor;
				ConsensusTransactionsOverflowRound = Id;

				var e = tn - Mcv.Zone.TransactionsPerRoundLimit;

				var gi = gu.AsEnumerable().GetEnumerator();

				do
				{
					if(!gi.MoveNext())
						gi.Reset();
					
					if(gi.Current.Transactions.Length > TransactionsPerVoteExecutionLimit)
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
				if(ConsensusExeunitFee > Zone.ExeunitMinFee && Id - ConsensusTransactionsOverflowRound > Mcv.P)
					ConsensusExeunitFee /= Zone.TransactionsFeeOverflowFactor;
			}
			
			var txs = gu.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();

			var t = gu.GroupBy(x => x.Time).MaxBy(i => i.Count());

			if(t != null)
			{
				if(t.Count() >= gq && t.Key > Previous.ConsensusTime)
					ConsensusTime = t.Key;
				else
					ConsensusTime = Previous.ConsensusTime;
			}

			Execute(txs);

			ConsensusTransactions = txs.Where(i => i.Successful).ToArray();

			if(Id >= Mcv.P)
			{
				ConsensusMemberLeavers	= gu.SelectMany(i => i.MemberLeavers).Distinct()
											.Where(x => Members.Any(j => j.Account == x) && gu.Count(b => b.MemberLeavers.Contains(x)) >= gq)
											.Order().ToArray();

				ConsensusViolators		= gu.SelectMany(i => i.Violators).Distinct()
											.Where(x => gu.Count(b => b.Violators.Contains(x)) >= gq)
											.Order().ToArray();

				//ConsensusFundJoiners	= gu.SelectMany(i => i.FundJoiners).Distinct()
				//							.Where(x => !Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
				//							.Order().ToArray();
				//
				//ConsensusFundLeavers	= gu.SelectMany(i => i.FundLeavers).Distinct()
				//							.Where(x => Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
				//							.Order().ToArray();
				//
				Elect(gu, gq);
			}

			Hashify(); /// depends on BaseHash 

			return Hash;
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

		public void Execute(IEnumerable<Transaction> transactions)
		{
			if(Confirmed)
				throw new IntegrityException();
	
			if(Id != 0 && Previous == null)
				return;

			foreach(var t in transactions)
				foreach(var o in t.Operations)
					o.Error = null;

			Members				= Id == 0 ? new()								: Previous.Members;
			Funds				= Id == 0 ? new()								: Previous.Funds;
#if IMMISSION
			Emissions			= Id == 0 ? new()								: Previous.Emissions;
#endif
			RentPerBytePerDay	= Id == 0 ? Mcv.Zone.RentPerBytePerDayMinimum	: Previous.RentPerBytePerDay;

			InitializeExecution();

		start: 
			Emission		= Id == 0 ? 0 : Previous.Emission;
			NextAccountIds	= new (Bytes.EqualityComparer);
			NextDomainIds	= new (Bytes.EqualityComparer);

			Rewards.Clear();
			AffectedAccounts.Clear();

			RestartExecution();

			foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
			{
				var a = AffectAccount(t.Signer);

				if(t.Nid != a.LastTransactionNid + 1)
				{
					foreach(var o in t.Operations)
						o.Error = Operation.NotSequential;
					
					goto start;
				}

				Money f = 0;
				Money r = 0;

				foreach(var o in t.Operations)
				{
					o.ExeUnits = 1;
					o.Reward = 0;

					o.Execute(Mcv, this);

					if(o.Error != null)
						goto start;

#if ETHEREUM
					if(o is not Immission)
					{
						f += o.ExeUnits * ConsensusExeunitFee;
					}
#endif

					r += o.Reward; 
				
					if(t.Fee < f || a.Balance - f < 0)
					{
						o.Error = Operation.NotEnoughUNT;
						goto start;
					}
				}

				if(t.Generator.Ei != -1)
				{
					var g = Mcv.Accounts.Find(t.Generator, Id);
					if(Rewards.TryGetValue(g, out var x))
						Rewards[g] = x + r + t.Fee;
					else
						Rewards[g] = r + t.Fee;
				}

				a.Balance -= t.Fee;
				a.LastTransactionNid++;
						
				if(Mcv.Settings.Base?.Chain != null)
				{
					a.Transactions.Add(Id);
				}
			}

			FinishExecution();
		}

		public virtual void CopyConfirmed(){}
		public virtual void RegisterForeign(Operation o){}
		public virtual void ConfirmForeign(){}

		public void Confirm()
		{
			if(Confirmed)
				throw new IntegrityException();

			if(Id > 0 && Mcv.LastConfirmedRound != null && Mcv.LastConfirmedRound.Id + 1 != Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == Id");

			Execute(ConsensusTransactions);

			Last365BaseDeltas	= Id == 0 ? Enumerable.Range(0, Time.FromYears(1).Days).Select(i => 0L).ToList() : Previous.Last365BaseDeltas.ToList();
			PreviousDayBaseSize	= Id == 0 ? 0 : Previous.PreviousDayBaseSize;
			Members				= Members.ToList();
			Funds				= Funds.ToList();
#if ETHEREUM
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

			foreach(var i in ConsensusViolators.Select(i => Members.Find(j => j.Account == i)))
			{
				AffectAccount(i.Account).AvarageUptime = 0;
				Rewards[Mcv.Accounts.Find(i.Account, Id)] += i.Bail;
				Members.Remove(i);
			}

			foreach(var i in ConsensusMemberLeavers.Select(i => Members.Find(j => j.Account == i)))
			{
				var a = AffectAccount(i.Account);
				a.Balance += i.Bail;
				a.AvarageUptime = (a.AvarageUptime + Id - i.CastingSince)/(a.AvarageUptime == 0 ? 1 : 2);
				Members.Remove(i);
			}

			var cds = ConsensusTransactions	.Where(i => !ConsensusViolators.Contains(i.Signer) && !ConsensusMemberLeavers.Contains(i.Signer))
											.SelectMany(i => i.Operations)
											.OfType<CandidacyDeclaration>()
											.ToList();

			foreach(var i in cds.GroupBy(i => i.Transaction.Signer)
								.Select(i => i.MaxBy(i => i.Bail))
								.OrderByDescending(i => Mcv.Accounts.Find(i.Signer, Id).AvarageUptime)
								.ThenBy(i => i.Bail)
								.ThenBy(i => i.Signer)
								.Take(Mcv.Zone.MembersLimit - Members.Count))
			{

				Members.Add(new Member{	CastingSince	= Id + Mcv.DeclareToGenerateDelay,
										Bail			= i.Bail,
										Account			= i.Signer, 
										BaseRdcIPs		= i.BaseRdcIPs, 
										SeedHubRdcIPs	= i.SeedHubRdcIPs});
				cds.Remove(i);
			}

			foreach(var i in cds) /// refund the rest
			{
				AffectAccount(i.Transaction.Signer).Balance += i.Bail;
			}

			Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
			Funds.AddRange(ConsensusFundJoiners);
			
			if(Mcv.IsCommitReady(this))
			{
				var tail = Mcv.Tail.AsEnumerable().Reverse().Take(Mcv.Zone.CommitLength);
				var r = Members.Where(i => i.CastingSince <= tail.First().Id).ToDictionary(i => Mcv.Accounts.Find(i.Account, Id), i => Money.Zero);

				foreach(var i in tail)
				{
					foreach(var j in i.Rewards)
					{
						if(r.TryGetValue(j.Key, out var x))
							r[j.Key] = x + j.Value;
						else
							r[j.Key] = j.Value;
					}
				}

				Distribute(r, 9, Funds, 1);
			}

			if(Id > 0 && ConsensusTime != Previous.ConsensusTime)
			{
				///var accs = new Dictionary<AccountAddress, AccountEntry>();
				///var doms = new Dictionary<string, DomainEntry>();
				///
				///foreach(var r in Mcv.Tail.SkipWhile(i => i != this))
				///{
				///	foreach(var i in r.AffectedAccounts)
				///		if(!accs.ContainsKey(i.Key))
				///			accs.Add(i.Key, i.Value);
				///
				///	foreach(var i in r.AffectedDomains)
				///		if(!doms.ContainsKey(i.Key))
				///			doms.Add(i.Key, i.Value);
				///}

				long s = Mcv.Size;
				
				foreach(var t in Mcv.Tables)
				{
					s += t.MeasureChanges(Mcv.Tail.SkipWhile(i => i != this));
				}

				/// + Mcv.Accounts.MeasureChanges(accs.Values) + Mcv.Domains.MeasureChanges(doms.Values);
								
				Last365BaseDeltas.RemoveAt(0);
				Last365BaseDeltas.Add(s - PreviousDayBaseSize);

				if(Last365BaseDeltas.Sum() > Mcv.Zone.TargetBaseGrowthPerYear)
				{
					RentPerBytePerDay = Mcv.Zone.RentPerBytePerDayMinimum * Last365BaseDeltas.Sum() / Mcv.Zone.TargetBaseGrowthPerYear;
				}

				PreviousDayBaseSize = s;
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
			writer.Write(ConsensusExeunitFee);
			writer.Write7BitEncodedInt(ConsensusTransactionsOverflowRound);
			
			writer.Write(RentPerBytePerDay);
			writer.Write7BitEncodedInt64(PreviousDayBaseSize);
			writer.Write(Last365BaseDeltas, writer.Write7BitEncodedInt64);
			
			writer.Write(Emission);
			writer.Write(Members, i => i.WriteBaseState(writer));
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
			ConsensusExeunitFee					= reader.Read<Money>();
			ConsensusTransactionsOverflowRound	= reader.Read7BitEncodedInt();
			
			RentPerBytePerDay		= reader.Read<Money>();
			PreviousDayBaseSize		= reader.Read7BitEncodedInt64();
			Last365BaseDeltas		= reader.ReadList(() => reader.Read7BitEncodedInt64());
			
			Emission				= reader.Read<Money>();
			Members					= reader.Read<Member>(m => m.ReadBaseState(reader)).ToList();
			Funds					= reader.ReadList<AccountAddress>();
#if ETHEREUM
			Emissions				= reader.Read<Immission>(m => m.ReadBaseState(reader)).ToList();
#endif
		}

		public virtual void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConsensusTime);
			writer.Write(ConsensusExeunitFee);
			writer.Write7BitEncodedInt(ConsensusTransactionsOverflowRound);
			writer.Write(ConsensusMemberLeavers);
			writer.Write(ConsensusFundJoiners);
			writer.Write(ConsensusFundLeavers);
			writer.Write(ConsensusViolators);
			writer.Write(ConsensusTransactions, i => i.WriteConfirmed(writer));
		}

		public virtual void ReadConfirmed(BinaryReader reader)
		{
			ConsensusTime						= reader.Read<Time>();
			ConsensusExeunitFee					= reader.Read<Money>();
			ConsensusTransactionsOverflowRound	= reader.Read7BitEncodedInt();
			ConsensusMemberLeavers				= reader.ReadArray<AccountAddress>();
			ConsensusFundJoiners				= reader.ReadArray<AccountAddress>();
			ConsensusFundLeavers				= reader.ReadArray<AccountAddress>();
			ConsensusViolators					= reader.ReadArray<AccountAddress>();
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
