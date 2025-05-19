using System.Diagnostics;

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
	public AutoId[]										ConsensusMemberLeavers = {};
	public AutoId[]										ConsensusViolators = {};
	public AccountAddress[]								ConsensusFundJoiners = {};
	public AccountAddress[]								ConsensusFundLeavers = {};
	public long											ConsensusECEnergyCost;
	public int											ConsensusOverloadRound;
	public byte[][]										ConsensusNtnStates = [];

	public bool											Confirmed = false;
	public byte[]										Hash;

	public List<Generator>								Candidates = new();
	public List<Generator>								Members = new();
	public List<AccountAddress>							Funds;
	public long[]										Spacetimes = [];
	public long[]										Bandwidths = [];

	public Dictionary<MetaId, MetaEntity>				AffectedMetas = new();
	public Dictionary<AutoId, Account>					AffectedAccounts = new();
	public Dictionary<int, int>[]						NextEids;

	public Mcv											Mcv;
	public McvNet										Net => Mcv.Net;

	public abstract long								AccountAllocationFee(Account account);
	//public abstract int									GetSpaceUsers();
	public virtual void									CopyConfirmed(){}
	public virtual void									RegisterForeign(Operation o){}
	public virtual void									ConfirmForeign(Execution execution){}


	public int MinimumForConsensus
	{
		get
		{
			var n = SelectedVoters.Count();

			if(n == 1)	return 1;
			if(n == 2)	return 2;
			if(n == 4)	return 3;
	
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
		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();
	}

	public override string ToString()
	{
		return $"Id={Id}, VoT/P={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), Members={Members?.Count}, ConfirmedTime={ConsensusTime}, {(Confirmed ? "Confirmed, " : "")}Hash={Hash?.ToHex()}";
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Accounts)	return AffectedAccounts;
		if(table == Mcv.Metas)	return AffectedMetas;

		throw new IntegrityException();
	}

	public virtual S FindState<S>(TableBase table) where S : TableStateBase
	{
		return null;
	}

	public Dictionary<K, E> AffectedByTable<K, E>(TableBase table)
	{
		return AffectedByTable(table) as Dictionary<K, E>;
	}
	/*
	public int GetNextEid(TableBase table,  int b)
	{
		int e = 0;

		foreach(var r in Mcv.Tail.Where(i => i.Id <= Id))
		{	
			var eids = r.NextEids[table.Id];

			if(eids != null && eids.TryGetValue(b, out e))
				break;
		}
			
		if(e == 0)
			e = table.FindBucket(b)?.NextEid ?? 0;

		NextEids[table.Id][b] = e + 1;

		return e;
	}
	*/
	/*
	public virtual ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Accounts.Id == table)	
			return Mcv.Accounts.Find(id, Id) != null ? AffectAccount(id) : null;

		return null;
	}

	public virtual AccountEntry AffectSigner(Transaction transaction)
	{
 		if(transaction.Signer == Net.God)
 			return new AccountEntry {Address = Net.God};

		var s = Mcv.Accounts.Find(transaction.Signer, Id);

		if(s == null)
		{
			foreach(var o in transaction.Operations)
				o.Error = Operation.NotFound;
					
			return null;
		}
	
		if(transaction.Nid != s.LastTransactionNid + 1)
		{
			foreach(var o in transaction.Operations)
				o.Error = Operation.NotSequential;
					
			return null;
		}

		return AffectAccount(s.Id);
	}

	public virtual AccountEntry CreateAccount(AccountAddress address)
	{
		var b = Mcv.Accounts.KeyToBid(address);
			
		int e = GetNextEid(Mcv.Accounts, b);

		var a = Mcv.Accounts.Create();

		a.Id		= LastCreatedId = new EntityId(b, e);
		a.Address	= address;
		a.New		= true;
		
		AffectedAccounts[a.Id] = a;

		return a;
	}
	*/

// 	protected AccountEntry AffectAccount(EntityId id)
// 	{
// 		if(AffectedAccounts.TryGetValue(id, out var a))
// 			return a;
// 
// 		a = Mcv.Accounts.Find(id, Id - 1)?.Clone();	
// 
// 		AffectedAccounts[a.Id] = a;
// 
// 		TransferEnergyIfNeeded(a);
// 
// 		return a;
// 	}
// 
// 	Generator AffectCandidate(EntityId id)
// 	{
// 		if(AffectedCandidates.TryGetValue(id, out Generator a))
// 			return a;
// 
// 		if(Id > 0 && Candidates == Previous.Candidates)
// 		{
// 			Candidates = Previous.Candidates.ToList();
// 		}
// 
// 		var c = Candidates.Find(i => i.Id == id);
// 
// 		if(c == null)
// 		{
// 			c = AffectedCandidates[id] = Mcv.CreateGenerator();
// 
// 			Candidates.Add(c);
// 		
// 			if(Candidates.Count > Mcv.Net.CandidatesMaximum)
// 				Candidates.RemoveAt(0);
// 		}
// 		else
// 			throw new IntegrityException();
// 
// 		return c;
// 	}
	
	public virtual void Elect(Vote[] votes, int gq)
	{
	}

	public byte[] Summarize()
	{
		if(!VotesOfTry.Any())
			return null;

		var min = MinimumForConsensus;
		var all = VotesOfTry.ToArray();
		var svotes = Id < Mcv.JoinToVote ? [] : Required.ToArray();
					
		ConsensusECEnergyCost	= Id == 0 ? 0 : Previous.ConsensusECEnergyCost;
		ConsensusOverloadRound	= Id == 0 ? 0 : Previous.ConsensusOverloadRound;

		var tn = all.Sum(i => i.Transactions.Length);

		if(tn > Mcv.Net.TransactionsPerRoundExecutionLimit)
		{
			ConsensusECEnergyCost *= Mcv.Net.OverloadFeeFactor;
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
			if(ConsensusECEnergyCost > 1 && Id - ConsensusOverloadRound > Mcv.P)
				ConsensusECEnergyCost /= Net.OverloadFeeFactor;
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
			ConsensusMemberLeavers = svotes	.SelectMany(i => i.MemberLeavers).Distinct()
											.Where(x => Members.Any(j => j.Id == x) && svotes.Count(b => b.MemberLeavers.Contains(x)) >= min)
											.Order().ToArray();

			ConsensusViolators = svotes	.SelectMany(i => i.Violators).Distinct()
										.Where(x => svotes.Count(b => b.Violators.Contains(x)) >= min)
										.Order().ToArray();

			ConsensusNtnStates	= svotes.SelectMany(i => i.NntBlocks).Distinct(Bytes.EqualityComparer)
										.Where(v => svotes.Count(i => i.NntBlocks.Contains(v, Bytes.EqualityComparer)) >= min)
										.Order(Bytes.Comparer).ToArray();

			Elect(svotes, min);
		}

		Hashify(); /// depends on Mcv.BaseHash 

		return Hash;
	}
	
	public IEnumerable<AutoId> ProposeViolators()
	{
		return Forkers.Select(i => Mcv.Accounts.Find(i, Previous.Id).Id);
	}

	public IEnumerable<AutoId> ProposeMemberLeavers(AccountAddress generator)
	{
		var prevs = Enumerable.Range(ParentId - Mcv.P, Mcv.P).Select(Mcv.FindRound);

		var l = Parent.Voters.Where(i => 
										!Parent.VotesOfTry.Any(v => v.Generator == i.Address) && /// did not sent a vote
										!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Id)))) /// not yet proposed in prev [Pitch-1] rounds
							.Select(i => i.Id);

		return l;
	}

	public virtual Execution CreateExecution(Transaction transaction)
	{
		return new Execution(Mcv, this, transaction);
	}

	public virtual void FinishExecution()
	{
	}

	public virtual void Execute(IEnumerable<Transaction> transactions, bool trying = false)
	{
		if(Confirmed)
			throw new IntegrityException();

		if(Id != 0 && Previous == null)
			return;

		foreach(var t in transactions)
			foreach(var o in t.Operations)
				o.Error = null;

		Candidates	= Id == 0 ? new()								: Previous.Candidates.ToList();
		Members		= Id == 0 ? new()								: Previous.Members;
		Funds		= Id == 0 ? new()								: Previous.Funds;
		Bandwidths	= Id == 0 ? new long[Net.BandwidthDaysMaximum]	: Previous.Bandwidths.Clone() as long[];
		Spacetimes	= Id == 0 ? new long[1]							: Previous.Spacetimes.Clone() as long[];

		AffectedMetas.Clear();
		AffectedAccounts.Clear();
		
		foreach(var i in NextEids)
			i.Clear();

		foreach(var i in Mcv.Tables)
			FindState<TableStateBase>(i)?.StartRoundExecution(this);

		foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
		{
			var e = CreateExecution(t);

			var s = e.AffectSigner();

			if(s == null)
				continue;

			foreach(var o in t.Operations)
			{
				o.Signer			= s;
				o.EnergyFeePayer	= null;
				o.EnergySpenders	= [];
				o.SpacetimeSpenders	= [];
				o.EnergyConsumed	= ConsensusECEnergyCost;

				o.Execute(e);

				if(o.Error != null)
					break;
			
				if(o.EnergyFeePayer == null)
				{
					if(o.EnergySpenders.Count == 0)
					{	
						o.EnergyFeePayer = s;
						o.EnergySpenders.Add(s);
					}
					else if(o.EnergySpenders.Count == 1)
					{
						o.EnergyFeePayer = o.EnergySpenders.First();
					}
					else
						throw new IntegrityException();
				}

				if(o.EnergyFeePayer.BandwidthExpiration >= ConsensusTime.Days)
				{
					if(o.EnergyFeePayer.BandwidthTodayTime < ConsensusTime.Days) /// switch to this day
					{	
						o.EnergyFeePayer.BandwidthTodayTime		 = (short)ConsensusTime.Days;
						o.EnergyFeePayer.BandwidthTodayAvailable = o.EnergyFeePayer.Bandwidth;
					}

					o.EnergyFeePayer.BandwidthTodayAvailable -= o.EnergyConsumed;

					if(o.EnergyFeePayer.BandwidthTodayAvailable < 0)
					{
						o.Error = Operation.NotEnoughBandwidth;
						break;
					}
				}
				else
				{
					o.EnergyFeePayer.Energy -= o.EnergyConsumed;
				}
				
				o.EnergyFeePayer.Energy -= t.Bonus;
				
				foreach(var i in o.EnergySpenders)
				{
					if(i.Energy < 0)
					{
						o.Error = Operation.NotEnoughEnergy;
						break;
					}
				
					if(i.EnergyNext < 0)
					{
						o.Error = Operation.NotEnoughEnergyNext;
						break;
					}
				}

				foreach(var i in o.SpacetimeSpenders)
				{
					if(i.Spacetime < 0)
					{
						o.Error = Operation.NotEnoughSpacetime;
						break;
					}
				}
			}
			
			if(t.Successful)
			{
				s.LastTransactionNid++;
	
				Absorb(e);
			}
		}

		FinishExecution();
	}

	public virtual void Absorb(Execution execution)
	{
		foreach(var i in execution.AffectedMetas)
			AffectedMetas[i.Key] = i.Value;

		foreach(var i in execution.AffectedAccounts)
			AffectedAccounts[i.Key] = i.Value;

		for(int t=0; t<Mcv.Tables.Length; t++)
			foreach(var i in execution.NextEids[t])
				NextEids[t][i.Key] = i.Value;

		if(execution.Candidates != null)	Candidates	= execution.Candidates;
		if(execution.Spacetimes != null)	Spacetimes	= execution.Spacetimes;
		if(execution.Bandwidths != null)	Bandwidths	= execution.Bandwidths;
	}

	public void Confirm()
	{
 		if(!Monitor.IsEntered(Mcv.Lock))
 			Debugger.Break();
 
		if(Confirmed)
			throw new IntegrityException();

		if(Id > 0 && Mcv.LastConfirmedRound != null && Mcv.LastConfirmedRound.Id + 1 != Id)
			throw new IntegrityException("LastConfirmedRound.Id + 1 == Id");

		Execute(ConsensusTransactions);

		Members	= Members.ToList();
		Funds	= Funds.ToList();

		CopyConfirmed();
		
		foreach(var t in ConsensusTransactions)
		{
			foreach(var o in t.Operations)
			{
				RegisterForeign(o);
			}
		}

		var e = CreateExecution(null);

		ConfirmForeign(e);

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
			e.AffectAccount(i.Id).AverageUptime = 0;
			Members.Remove(i);
		}

		foreach(var i in ConsensusMemberLeavers.Select(i => Members.Find(j => j.Id == i)))
		{
			var a = e.AffectAccount(i.Id);
			
			a.AverageUptime = (a.AverageUptime + Id - i.CastingSince)/(a.AverageUptime == 0 ? 1 : 2);
			Members.Remove(i);
		}

		foreach(var i in e.Candidates/*.OrderByHash(i => i.Address.Bytes, Hash)*/.TakeLast(Mcv.Net.MembersLimit - Members.Count).ToArray())
		{
			var c = e.AffectCandidate(i.Id);
			
			c.CastingSince = Id + Mcv.JoinToVote;
			
			e.Candidates.Remove(i);
			Members.Add(c);
		}

		Members = Members.OrderBy(i => i.Address).ToList();

		Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
		Funds.AddRange(ConsensusFundJoiners);

		if(Id > 0 && ConsensusTime.Days != Previous.ConsensusTime.Days) /// day switched
		{
			var d = ConsensusTime.Days - Previous.ConsensusTime.Days;

			//d %= Time.FromYears(1).Days;

			e.Bandwidths = d < e.Bandwidths.Length ? [..e.Bandwidths[d..], ..new long[d]] : new long[d];

			foreach(var i in Members.Select(i => e.AffectAccount(i.Id)))
			{
				i.EnergyNext	+= d * Net.ECDayEmission / Members.Count;
				i.Spacetime		+= d * (Net.BDDayEmission + e.Spacetimes[0]) / Members.Count;
			}
		}

		Absorb(e);
		
		Confirmed = true;
		Mcv.LastConfirmedRound = this;
	}

	public void Hashify()
	{
		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		w.Write(Mcv.GraphHash);
		w.Write(Id > 0 ? Previous.Hash : Mcv.Net.Cryptography.ZeroHash);
		WriteConfirmed(w);

		Hash = Cryptography.Hash(s.ToArray());
	}

	public virtual void WriteGraphState(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Hash);
		writer.Write(Bandwidths, writer.Write7BitEncodedInt64);
		writer.Write(Funds);
		writer.Write(Spacetimes, writer.Write7BitEncodedInt64);

		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusECEnergyCost);
		writer.Write7BitEncodedInt(ConsensusOverloadRound);
	}

	public virtual void ReadGraphState(BinaryReader reader)
	{
		Id						= reader.Read7BitEncodedInt();
		Hash					= reader.ReadHash();
		Bandwidths				= reader.ReadArray(reader.Read7BitEncodedInt64);
		Funds					= reader.ReadList<AccountAddress>();
		Spacetimes				= reader.ReadArray(reader.Read7BitEncodedInt64);

		ConsensusTime			= reader.Read<Time>();
		ConsensusECEnergyCost	= reader.Read7BitEncodedInt64();
		ConsensusOverloadRound	= reader.Read7BitEncodedInt();
	}

	public virtual void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusECEnergyCost);
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
		ConsensusECEnergyCost	= reader.Read7BitEncodedInt64();
		ConsensusOverloadRound	= reader.Read7BitEncodedInt();
		ConsensusMemberLeavers	= reader.ReadArray<AutoId>();
		ConsensusViolators		= reader.ReadArray<AutoId>();
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
			WriteConfirmed(w);
			w.Write(Hash);
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
			ReadConfirmed(r);
			Hash = r.ReadHash();
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
