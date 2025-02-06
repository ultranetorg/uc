namespace Uccs.Net;

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
	public long											PerVoteTransactionsLimit		=> Mcv.Net.TransactionsPerRoundAbsoluteLimit / Members.Count;
	public long											PerVoteOperationsLimit			=> Mcv.Net.ExecutionCyclesPerRoundMaximum / Members.Count;
	public long											PerVoteBandwidthAllocationLimit	=> Mcv.Net.BandwidthAllocationPerRoundMaximum / Members.Count;

	public bool											IsLastInCommit => (Id % Net.CommitLength) == Net.CommitLength - 1; ///Tail.Count(i => i.Id <= round.Id) >= Net.CommitLength; 

	public int											Try = 0;
	public DateTime										FirstArrivalTime = DateTime.MaxValue;

	public IEnumerable<Generator>						Voters => VotersRound.Members;
	public IEnumerable<Generator>						SelectedVoters => Id < Mcv.JoinToVote ? [] : Voters.OrderByHash(i => i.Address.Bytes, [(byte)(Try>>24), (byte)(Try>>16), (byte)(Try>>8), (byte)Try, ..VotersRound.Hash]).Take(Mcv.RequiredVotersMaximum);

	public List<Vote>									Votes = new();
	public List<AccountAddress>							Forkers = new();
	public IEnumerable<Vote>							VotesOfTry => Votes.Where(i => i.Try == Try);
	public IEnumerable<Vote>							Payloads => VotesOfTry.Where(i => i.Transactions.Any());
	public IEnumerable<Vote>							Required => VotesOfTry.Where(i => SelectedVoters.Any(j => j.Address == i.Generator));
	public IGrouping<byte[], Vote>						MajorityOfRequiredByParentHash => Required.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

	public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
	public IEnumerable<Transaction>						Transactions => Confirmed ? ConsensusTransactions : OrderedTransactions;

	public Time											ConsensusTime;
	public Transaction[]								ConsensusTransactions = {};
	public EntityId[]									ConsensusMemberLeavers = {};
	public EntityId[]									ConsensusViolators = {};
	public AccountAddress[]								ConsensusFundJoiners = {};
	public AccountAddress[]								ConsensusFundLeavers = {};
	public long											ConsensusExecutionFee;
	public int											ConsensusOverloadRound;
	public byte[][]										ConsensusNtnStates = [];

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
	public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
	public Dictionary<EntityId, Generator>				AffectedCandidates = new();
	public Dictionary<int, int>							NextAccountEids;
	public long[]										NextBandwidthAllocations = [];

	public Mcv											Mcv;
	public McvNet										Net => Mcv.Net;

	public abstract long								AccountAllocationFee(Account account);
	public virtual void									CopyConfirmed(){}
	public virtual void									RegisterForeign(Operation o){}
	public virtual void									ConfirmForeign(){}


	public byte[]												__SummaryBaseHash;
	public byte[]												__SummaryBaseState;
	public Vote[]												__SummaruVotesOfTry;

	public int MinimumForConsensus
	{
		get
		{
			var n = SelectedVoters.Count();

			if(n == 1)	return 1;
			if(n == 2)	return 2;
			if(n == 4)	return 3;
	
			///return Math.Min(n, Mcv.VotesRequired) * 2/3;
			return n * 2/3;

		}
	}

	public bool ConsensusReached
	{
		get
		{ 
			if(VotesOfTry.Count() < MinimumForConsensus)
				return false;

			return MajorityOfRequiredByParentHash.Count() >= MinimumForConsensus;
		}
	}

	public bool ConsensusFailed
	{
		get
		{ 
			var s = SelectedVoters;
			var r = Required;
		
			var missing = s.Count() - r.Count();

			return r.Any() && r.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).All(i => i.Count() + missing < MinimumForConsensus);
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

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Accounts)	return AffectedAccounts;

		throw new IntegrityException();
	}

	public virtual Dictionary<int, int> NextEidsByTable(TableBase table)
	{
		if(table == Mcv.Accounts)	return NextAccountEids;

		return null;
	}

	public int GetNextEid(TableBase table,  int b)
	{
		int e = -1;

		foreach(var r in Mcv.Tail.Where(i => i.Id <= Id))
		{	
			var eids = r.NextEidsByTable(table);

			if(eids != null && eids.TryGetValue(b, out e))
				break;
		}
			
		if(e == -1)
			e = table.FindBucket(b)?.NextEid ?? 0;

		NextEidsByTable(table)[b] = e + 1;

		return e;
	}

	public AccountEntry AffectAccount(AccountAddress address)
	{
		if(AffectedAccounts.TryGetValue(address, out var a))
			return a;
		
		a = Mcv.Accounts.Find(address, Id - 1);	

		if(a != null)
			return AffectedAccounts[address] = a.Clone();
		else
		{
			var b = Mcv.Accounts.KeyToBid(address);
			
			int e = GetNextEid(Mcv.Accounts, b);

			a = Mcv.Accounts.Create();

			a.Id		= new EntityId(b, e);
			a.Address	= address;
			a.ECBalance = [];
			a.New		= true;
			
			return  AffectedAccounts[address] = a;
		}
	}

	public AccountEntry AffectAccount(EntityId id)
	{
		if(AffectedAccounts.FirstOrDefault(i => i.Value.Id == id).Value is var a)
			return a;
		
		a = Mcv.Accounts.Find(id, Id - 1);	

		return AffectedAccounts[a.Address] = a.Clone();
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
			throw new IntegrityException();

		return c;
	}

	public virtual void Elect(Vote[] votes, int gq)
	{
	}

	public byte[] Summarize()
	{
		if(!VotesOfTry.Any())
			return null;

		var min = MinimumForConsensus;
		var all = VotesOfTry.ToArray();
		__SummaruVotesOfTry = all.ToArray();
		var svotes = Id < Mcv.JoinToVote ? [] : Required.ToArray();
					
		ConsensusExecutionFee	= Id == 0 ? 0 : Previous.ConsensusExecutionFee;
		ConsensusOverloadRound	= Id == 0 ? 0 : Previous.ConsensusOverloadRound;

		var tn = all.Sum(i => i.Transactions.Length);

		if(tn > Mcv.Net.TransactionsPerRoundExecutionLimit)
		{
			ConsensusExecutionFee *= Mcv.Net.OverloadFeeFactor;
			ConsensusOverloadRound = Id;

			var e = tn - Mcv.Net.TransactionsPerRoundExecutionLimit;

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
			if(ConsensusExecutionFee > 1 && Id - ConsensusOverloadRound > Mcv.P)
				ConsensusExecutionFee /= Net.OverloadFeeFactor;
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
			ConsensusMemberLeavers = svotes.SelectMany(i => i.MemberLeavers).Distinct()
											.Where(x => Members.Any(j => j.Id == x) && svotes.Count(b => b.MemberLeavers.Contains(x)) >= min)
											.Order().ToArray();

			ConsensusViolators = svotes.SelectMany(i => i.Violators).Distinct()
										.Where(x => svotes.Count(b => b.Violators.Contains(x)) >= min)
										.Order().ToArray();

			ConsensusNtnStates	= svotes.SelectMany(i => i.NntBlocks).Distinct(Bytes.EqualityComparer)
										.Where(v => svotes.Count(i => i.NntBlocks.Contains(v, Bytes.EqualityComparer)) >= min)
										.Order(Bytes.Comparer).ToArray();

			//ConsensusFundJoiners	= gu.SelectMany(i => i.FundJoiners).Distinct()
			//							.Where(x => !Funds.Contains(x) && gu.Count(b => b.FundJoiners.Contains(x)) >= Net.MembersLimit * 2/3)
			//							.Order().ToArray();
			//
			//ConsensusFundLeavers	= gu.SelectMany(i => i.FundLeavers).Distinct()
			//							.Where(x => Funds.Contains(x) && gu.Count(b => b.FundLeavers.Contains(x)) >= Net.MembersLimit * 2/3)
			//							.Order().ToArray();
			//
			Elect(svotes, min);
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

		var l = Parent.Voters.Where(i => 
										!Parent.VotesOfTry.Any(v => v.Generator == i.Address) && /// did not sent a vote
										!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Id)))) /// not yet proposed in prev [Pitch-1] rounds
							.Select(i => i.Id);

		return l;
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


	start: 
		#if IMMISSION
		Emissions			= Id == 0 ? new()								: Previous.Emissions;
		//Emission		= Id == 0 ? 0 : Previous.Emission;
		#endif

		Candidates					= Id == 0 ? new()																					: Previous.Candidates;
		Members						= Id == 0 ? new()																					: Previous.Members;
		Funds						= Id == 0 ? new()																					: Previous.Funds;
		NextBandwidthAllocations	= Id == 0 ? Enumerable.Range(0, Net.BandwidthAllocationDaysMaximum + 1).Select(i => 0L).ToArray()	: Previous.NextBandwidthAllocations.Clone() as long[];

		NextAccountEids	= new ();

		BYRewards.Clear();
		ECRewards.Clear();
		AffectedCandidates.Clear();
		AffectedAccounts.Clear();

		foreach(var i in Mcv.Tables)
		{
			AffectedByTable(i).Clear();
			NextEidsByTable(i)?.Clear();
		}

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

				o.Execute(Mcv, this);

				if(o.Error != null)
					goto start;

				#if IMMISION
				if(o is not Immission)
				{
					f += o.ExeUnits * ConsensusExeunitFee;
				}
				#endif
				
				t.ECSpent += ConsensusExecutionFee;
				
				if(t.ECFee == 0 && s.BandwidthExpiration >= ConsensusTime)
				{
					if(s.BandwidthTodayTime < ConsensusTime) /// switch to this day
					{	
						s.BandwidthTodayTime		= ConsensusTime;
						s.BandwidthTodayAvailable	= s.BandwidthNext;
					}

					s.BandwidthTodayAvailable -= ConsensusExecutionFee;

					if(s.BandwidthTodayAvailable < 0)
					{
						o.Error = Operation.NotEnoughEC;
						goto start;
					}
				}
				else if(EC.Integrate(s.ECBalance, ConsensusTime) < t.ECSpent || (!trying && (EC.Integrate(s.ECBalance, ConsensusTime) < t.ECFee || t.ECSpent > t.ECFee)))
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

			if(t.Member.E != -1)
			{
				var g = Mcv.Accounts.Find(t.Member, Id);

				if(BYRewards.TryGetValue(g, out var x))
					BYRewards[g] = x + t.BYReward;
				else
					BYRewards[g] = t.BYReward;
			}

			if(!trying)
				s.ECBalance = EC.Subtract(s.ECBalance, t.ECFee, ConsensusTime);
			
			s.LastTransactionNid++;
		}

		FinishExecution();
	}

	public void Confirm()
	{
// 			if(Mcv.Node != null)
// 				if(!Monitor.IsEntered(Mcv.Lock))
// 					Debugger.Break();
// 
		if(Confirmed)
			throw new IntegrityException();

		if(Id > 0 && Mcv.LastConfirmedRound != null && Mcv.LastConfirmedRound.Id + 1 != Id)
			throw new IntegrityException("LastConfirmedRound.Id + 1 == Id");

		Execute(ConsensusTransactions);

		Members		= Members.ToList();
		Funds		= Funds.ToList();
		#if IMMISION
		Emissions	= Emissions.ToList();
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

		foreach(var i in Candidates.OrderByHash(i => i.Address.Bytes, Hash).Take(Mcv.Net.MembersLimit - Members.Count).ToArray())
		{
			var c = AffectCandidate(i.Id);
			
			c.CastingSince = Id + Mcv.JoinToVote;
			
			Candidates.Remove(c);
			Members.Add(c);
		}

		Members = Members.OrderBy(i => i.Address).ToList();

		Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
		Funds.AddRange(ConsensusFundJoiners);

		if(Id > 0 && ConsensusTime != Previous.ConsensusTime)
		{
			NextBandwidthAllocations = NextBandwidthAllocations.Skip(1).Append(0).ToArray();

			foreach(var i in Members./*Where(i => i.CastingSince <= tail.First().Id).*/Select(i => AffectAccount(i.Address)))
			{
				i.ECBalance = EC.Add(i.ECBalance, new EC (ConsensusTime + Net.ECLifetime, Net.ECDayEmission / Members.Count));
				i.BYBalance += Net.BYDayEmission / Members.Count;
			}
		}
		
		Confirmed = true;
		Mcv.LastConfirmedRound = this;
	}

	public void Hashify()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(Mcv.BaseHash);
		w.Write(Id > 0 ? Previous.Hash : Mcv.Net.Cryptography.ZeroHash);
		WriteConfirmed(w);

		Hash = Cryptography.Hash(s.ToArray());
	}

	public virtual void WriteBaseState(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Hash);
		writer.Write(NextBandwidthAllocations, writer.Write7BitEncodedInt64);
		writer.Write(Funds);
		
		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusExecutionFee);
		writer.Write7BitEncodedInt(ConsensusOverloadRound);
				
		#if ETHEREUM
		writer.Write(Emission);
		writer.Write(Emissions, i => i.WriteBaseState(writer));
		#endif
	}

	public virtual void ReadBaseState(BinaryReader reader)
	{
		Id							= reader.Read7BitEncodedInt();
		Hash						= reader.ReadHash();
		NextBandwidthAllocations	= reader.ReadArray(reader.Read7BitEncodedInt64);
		Funds						= reader.ReadList<AccountAddress>();

		ConsensusTime				= reader.Read<Time>();
		ConsensusExecutionFee		= reader.Read7BitEncodedInt64();
		ConsensusOverloadRound		= reader.Read7BitEncodedInt();

		#if ETHEREUM
		//Emission					= reader.Read<Money>();
		Emissions					= reader.Read<Immission>(m => m.ReadBaseState(reader)).ToList();
		#endif
	}

	public virtual void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusExecutionFee);
		writer.Write7BitEncodedInt(ConsensusOverloadRound);
		writer.Write(ConsensusMemberLeavers);
		writer.Write(ConsensusViolators);
		writer.Write(ConsensusFundJoiners);
		writer.Write(ConsensusFundLeavers);
		writer.Write(ConsensusTransactions, i => i.WriteConfirmed(writer));
	}

	public virtual void ReadConfirmed(BinaryReader reader)
	{
		ConsensusTime			= reader.Read<Time>();
		ConsensusExecutionFee	= reader.Read7BitEncodedInt64();
		ConsensusOverloadRound	= reader.Read7BitEncodedInt();
		ConsensusMemberLeavers	= reader.ReadArray<EntityId>();
		ConsensusViolators		= reader.ReadArray<EntityId>();
		ConsensusFundJoiners	= reader.ReadArray<AccountAddress>();
		ConsensusFundLeavers	= reader.ReadArray<AccountAddress>();
		ConsensusTransactions	= reader.Read(() =>	new Transaction {Net = Mcv.Net, Round = this}, t => t.ReadConfirmed(reader)).ToArray();
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
			//											b.Read(r, Mcv.Net);
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
											i.Net = Mcv.Net;
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
