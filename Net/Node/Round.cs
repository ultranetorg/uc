using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class Round : IBinarySerializable
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
		public OperationId[]								ConsensusEmissions = {};
		public OperationId[]								ConsensusMigrations = {};
		public Money										ConsensusExeunitFee;
		public int											ConsensusTransactionsOverflowRound;

		public bool											Confirmed = false;
		public byte[]										Hash;

		public Money										Rewards;
		public Money										Emission;
		public Money										RentPerBytePerDay;
		//public Money										RentPerEntityPerDay => RentPerBytePerDay * Mcv.EntityLength;
		public List<Member>									Members = new();
		public List<AccountAddress>							Funds;
		public List<Emission>								Emissions;
		public List<AuthorMigration>						Migrations;
		public Dictionary<byte[], int>						NextAccountIds;
		public Dictionary<byte[], int>						NextAuthorIds;
		public List<long>									Last365BaseDeltas;
		public long											PreviousDayBaseSize;

		public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>				AffectedAuthors = new();

		public Mcv											Mcv;
		public Zone											Zone => Mcv.Zone;

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
			return $"Id={Id}, VoT/P={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), Members={Members?.Count}, ConfirmedTime={ConsensusTime}, {(Confirmed ? "Confirmed " : "")}, Hash={Hash?.ToHex()}";
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

		public void Distribute(Money amount, IEnumerable<AccountAddress> a, int ashare, IEnumerable<AccountAddress> b, int bshare)
		{
			var s = amount * new Money(ashare)/new Money(ashare + bshare);

			if(a.Any())
			{
				var x = s/a.Count();
		
				foreach(var i in a.Skip(1))
				{
					//RewardOrPay(i, x);
					AffectAccount(i).Balance += x;
				}

				var v = s - (x * (a.Count() - 1));
				
				//RewardOrPay(a.First(), v);
				AffectAccount(a.First()).Balance += v;
			}

			if(b.Any())
			{
				s = amount - s;
				var x = s/b.Count();
		
				foreach(var i in b.Skip(1))
				{
					//RewardOrPay(i, x);
					AffectAccount(i).Balance += x;
				}

				var v = s - (x * (b.Count() - 1));
				//RewardOrPay(b.First(), v);
				AffectAccount(b.First()).Balance += v;
			}
		}

		public AccountEntry AffectAccount(AccountAddress account/*, Operation operation*/)
		{
			if(AffectedAccounts.TryGetValue(account, out AccountEntry a))
				return a;
			
			var e = Mcv.Accounts.Find(account, Id - 1);	

			if(e != null)
				return AffectedAccounts[account] = e.Clone();
			else
			{
				var ci = Mcv.Accounts.KeyToCluster(account).ToArray();
				var c = Mcv.Accounts.Clusters.Find(i => i.Id.SequenceEqual(ci));

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

		public AuthorEntry AffectAuthor(string author)
		{
			if(AffectedAuthors.TryGetValue(author, out AuthorEntry a))
				return a;
			
			var e = Mcv.Authors.Find(author, Id - 1);

			if(e != null)
			{
				AffectedAuthors[author] = e.Clone();
				AffectedAuthors[author].Affected  = true;;
				return AffectedAuthors[author];
			}
			else
			{
				var ci = Mcv.Authors.KeyToCluster(author).ToArray();
				var c = Mcv.Authors.Clusters.Find(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextAuthorIds[ci] = 0;
				else
					NextAuthorIds[ci] = c.NextEntityId;
				
				ai = NextAuthorIds[ci]++;

				return AffectedAuthors[author] = new AuthorEntry(Mcv){	Affected = true,
																		New = true,
																		Id = new EntityId(ci, ai), 
																		Name = author};
			}
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
					ConsensusTime	= t.Key;
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

				ConsensusEmissions		= gu.SelectMany(i => i.Emissions).Distinct()
											.Where(x => Emissions.Any(e => e.Id == x) && gu.Count(b => b.Emissions.Contains(x)) >= gq)
											.Order().ToArray();

				ConsensusMigrations		= gu.SelectMany(i => i.Migrations).Distinct()
											.Where(x => Migrations.Any(b => b.Id == x) && gu.Count(b => b.Migrations.Contains(x)) >= gq)
											.Order().ToArray();

				ConsensusFundJoiners	= gu.SelectMany(i => i.FundJoiners).Distinct()
											.Where(x => !Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Zone.MembersLimit * 2/3)
											.Order().ToArray();
				
				ConsensusFundLeavers	= gu.SelectMany(i => i.FundLeavers).Distinct()
											.Where(x => Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Zone.MembersLimit * 2/3)
											.Order().ToArray();
			}

			Hashify(); /// depends on BaseHash 

			return Hash;
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
			Emissions			= Id == 0 ? new()								: Previous.Emissions;
			Migrations			= Id == 0 ? new()								: Previous.Migrations;
			RentPerBytePerDay	= Id == 0 ? Mcv.Zone.RentPerBytePerDayMinimum	: Previous.RentPerBytePerDay;

		start: 
			Rewards			= 0;
			Emission		= Id == 0 ? 0 : Previous.Emission;

			NextAccountIds	= new (Bytes.EqualityComparer);
			NextAuthorIds	= new (Bytes.EqualityComparer);

			AffectedAccounts.Clear();
			AffectedAuthors.Clear();

			//foreach(var t in transactions)
			//	foreach(var o in t.Operations)
			//		o.Fee = ConsensusExeunitFee;

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

				foreach(var o in t.Operations.AsEnumerable().Reverse())
				{
					o.ExeUnits = 1;

					o.Execute(Mcv, this);

					if(o.Error != null)
						goto start;

					f += o.ExeUnits * ConsensusExeunitFee;
					r += o.Reward; 
				
					if(t.Fee < f || a.Balance - f < 0)
					{
						o.Error = Operation.NotEnoughUNT;
						goto start;
					}
				}

				Rewards += r + t.Fee;
				a.Balance -= t.Fee;
				a.LastTransactionNid++;
						
				if(Mcv.Roles.HasFlag(Role.Chain))
				{
					AffectAccount(t.Signer).Transactions.Add(Id);
				}
			}

			foreach(var a in AffectedAuthors)
			{
				a.Value.Affected = false;

				if(a.Value.Resources != null)
					foreach(var r in a.Value.Resources.Where(i => i.Affected))
					{
						r.Affected = false;

						if(r.Outbounds != null)
							foreach(var l in r.Outbounds.Where(i => i.Affected))
								l.Affected = false;
					}
			}
		}

		public void Confirm()
		{
			if(Confirmed)
				throw new IntegrityException();

			if(Id > 0 && Mcv.LastConfirmedRound != null && Mcv.LastConfirmedRound.Id + 1 != Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == Id");

			Execute(ConsensusTransactions);

			Last365BaseDeltas	= Id == 0 ? Enumerable.Range(0, Time.FromYears(1).Days).Select(i => (long)0).ToList() : Previous.Last365BaseDeltas.ToList();
			PreviousDayBaseSize	= Id == 0 ? 0 : Previous.PreviousDayBaseSize;
			Members				= Members.ToList();
			Funds				= Funds.ToList();
			Emissions			= Emissions.ToList();
			Migrations			= Migrations.ToList();

			foreach(var f in ConsensusViolators)
			{
				var fe = AffectAccount(f);
				Rewards += fe.Bail;
				fe.Bail = 0;
			}
			
			for(int ti = 0; ti < ConsensusTransactions.Length; ti++)
			{
				for(int oi = 0; oi < ConsensusTransactions[ti].Operations.Length; oi++)
				{
					var o = ConsensusTransactions[ti].Operations[oi];

					if(o is Emission e)
						Emissions.Add(e);

					if(o is AuthorMigration b)
						Migrations.Add(b);
				}
			}

			foreach(var i in ConsensusEmissions.Select(c => Emissions.Find(j => j.Id == c)))
			{
				i.ConfirmExecute(this);
				Emissions.Remove(i);
			}

			Emissions.RemoveAll(i => Id > i.Id.Ri + Mcv.Zone.ExternalVerificationDurationLimit);

			foreach(var i in ConsensusMigrations.Select(c => Migrations.Find(j => j.Id == c)))
			{
				i.ConsensusExecute(this);
				Migrations.Remove(i);
			}

			Migrations.RemoveAll(i => Id > i.Id.Ri + Mcv.Zone.ExternalVerificationDurationLimit);

	
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

			//foreach(var i in Members.Where(i => ConsensusViolators.Contains(i.Account)))
			//	Log?.Report(this, $"Member violator removed {Id} - {i.Account}");

			Members.RemoveAll(i => ConsensusViolators.Contains(i.Account));

			//foreach(var i in Members.Where(i => ConsensusMemberLeavers.Contains(i.Account)))
			//	Log?.Report(this, $"Member leaver removed {Id} - {i.Account}");

			Members.RemoveAll(i => ConsensusMemberLeavers.Contains(i.Account));

			var js = ConsensusTransactions	.SelectMany(i => i.Operations)
											.OfType<CandidacyDeclaration>()
											.DistinctBy(i => i.Transaction.Signer)
											.Where(i => !ConsensusViolators.Contains(i.Transaction.Signer) && !ConsensusMemberLeavers.Contains(i.Transaction.Signer))
											.OrderByDescending(i => i.Bail)
											.ThenBy(i => i.Signer)
											.Take(Mcv.Zone.MembersLimit - Members.Count);
 
			Members.AddRange(js.Select(i => new Member{	CastingSince = Id + Mcv.DeclareToGenerateDelay,
														Account = i.Signer, 
														BaseRdcIPs = i.BaseRdcIPs, 
														SeedHubRdcIPs = i.SeedHubRdcIPs}));


			Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
			Funds.AddRange(ConsensusFundJoiners);
			
			if(Mcv.IsCommitReady(this))
			{
				var tail = Mcv.Tail.AsEnumerable().Reverse().Take(Mcv.Zone.CommitLength);
				Distribute(tail.SumMoney(i => i.Rewards), Members.Where(i => i.CastingSince <= tail.First().Id).Select(i => i.Account), 9, Funds, 1);
			}

			if(Id > 0 && ConsensusTime != Previous.ConsensusTime)
			{
				var accounts = new Dictionary<AccountAddress, AccountEntry>();
				var authors	 = new Dictionary<string, AuthorEntry>();

				foreach(var r in Mcv.Tail.SkipWhile(i => i != this))
				{
					foreach(var i in r.AffectedAccounts)
						if(!accounts.ContainsKey(i.Key))
							accounts.Add(i.Key, i.Value);

					foreach(var i in r.AffectedAuthors)
						if(!authors.ContainsKey(i.Key))
							authors.Add(i.Key, i.Value);
				}

				var s = Mcv.Size + Mcv.Accounts.MeasureChanges(accounts.Values) + Mcv.Authors.MeasureChanges(authors.Values);
								
				Last365BaseDeltas.RemoveAt(0);
				Last365BaseDeltas.Add(s - PreviousDayBaseSize);

				if(Last365BaseDeltas.Sum() > Mcv.Zone.TargetBaseGrowth)
				{
					RentPerBytePerDay = Mcv.Zone.RentPerBytePerDayMinimum * Last365BaseDeltas.Sum() / Mcv.Zone.TargetBaseGrowth;
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

			Hash = Mcv.Zone.Cryptography.Hash(s.ToArray());
		}

		public void WriteBaseState(BinaryWriter writer)
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
			writer.Write(Emissions, i => i.WriteBaseState(writer));
			writer.Write(Migrations, i => i.WriteBaseState(writer));
		}

		public void ReadBaseState(BinaryReader reader)
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
			Emissions				= reader.Read<Emission>(m => m.ReadBaseState(reader)).ToList();
			Migrations				= reader.Read<AuthorMigration>(m => m.ReadBaseState(reader)).ToList();
		}

		public void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConsensusTime);
			writer.Write(ConsensusExeunitFee);
			writer.Write7BitEncodedInt(ConsensusTransactionsOverflowRound);
			writer.Write(ConsensusMemberLeavers);
			writer.Write(ConsensusFundJoiners);
			writer.Write(ConsensusFundLeavers);
			writer.Write(ConsensusViolators);
			writer.Write(ConsensusEmissions);
			writer.Write(ConsensusMigrations);
			writer.Write(ConsensusTransactions, i => i.WriteConfirmed(writer));
		}

		public void ReadConfirmed(BinaryReader reader)
		{
			ConsensusTime						= reader.Read<Time>();
			ConsensusExeunitFee					= reader.Read<Money>();
			ConsensusTransactionsOverflowRound	= reader.Read7BitEncodedInt();
			ConsensusMemberLeavers				= reader.ReadArray<AccountAddress>();
			ConsensusFundJoiners				= reader.ReadArray<AccountAddress>();
			ConsensusFundLeavers				= reader.ReadArray<AccountAddress>();
			ConsensusViolators					= reader.ReadArray<AccountAddress>();
			ConsensusEmissions					= reader.ReadArray<OperationId>();
			ConsensusMigrations					= reader.ReadArray<OperationId>();
			ConsensusTransactions				= reader.Read(() =>	new Transaction {Round = this}, t => t.ReadConfirmed(reader)).ToArray();
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
											var v = new Vote(Mcv);
											v.RoundId = Id;
											v.Round = this;
											v.ReadForRoundUnconfirmed(r);
												
											foreach(var i in v.Transactions)
											{
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
