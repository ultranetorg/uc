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
	public EntityId[]									ConsensusMemberLeavers = {};
	public EntityId[]									ConsensusViolators = {};
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

	public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
	public Dictionary<EntityId, Generator>				AffectedCandidates = new();
	public Dictionary<int, int>							NextAccountEids;
	public EntityId										LastCreatedId;
	public long[]										Spacetimes = [];
	public long[]										BandwidthAllocations = [];

	public Mcv											Mcv;
	public McvNet										Net => Mcv.Net;

	public abstract long								AccountAllocationFee(Account account);
	//public abstract int									GetSpaceUsers();
	public virtual void									CopyConfirmed(){}
	public virtual void									RegisterForeign(Operation o){}
	public virtual void									ConfirmForeign(){}

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
		int e = 0;

		foreach(var r in Mcv.Tail.Where(i => i.Id <= Id))
		{	
			var eids = r.NextEidsByTable(table);

			if(eids != null && eids.TryGetValue(b, out e))
				break;
		}
			
		if(e == 0)
			e = table.FindBucket(b)?.NextEid ?? 0;

		NextEidsByTable(table)[b] = e + 1;

		return e;
	}

	public void TransferECIfNeeded(IEnergyHolder a)
	{
		if(a.EnergyThisPeriod != ConsensusTime.Days/Net.ECLifetime.Days)
		{
			if(a.EnergyThisPeriod + 1 == ConsensusTime.Days/Net.ECLifetime.Days)
				a.Energy = a.EnergyNext;
	
			a.EnergyNext = 0;
			a.EnergyThisPeriod	= (byte)(ConsensusTime.Days/Net.ECLifetime.Days);
		}
	}

	public virtual ITableEntry Affect(byte table, EntityId id)
	{
		if(Mcv.Accounts.Id == table)
			return AffectAccount(id);

		throw new IntegrityException();
	}

	public AccountEntry CreateAccount(AccountAddress address)
	{
// 		if(AffectedAccounts.TryGetValue(address, out var a))
// 			return a;
// 		
// 		a = Mcv.Accounts.Find(address, Id - 1);	
// 
// 		if(a != null)
// 		{	
// 			a = AffectedAccounts[address] = a.Clone();
// 
// 			TransferECIfNeeded(a);
// 		}
// 		else
// 		{
			var b = Mcv.Accounts.KeyToBid(address);
			
			int e = GetNextEid(Mcv.Accounts, b);

			var a = Mcv.Accounts.Create();

			a.Id		= LastCreatedId = new EntityId(b, e);
			a.Address	= address;
			a.New		= true;
			

			AffectedAccounts[address] = a;
//		}

		return a;
	}

	public AccountEntry AffectAccount(EntityId id)
	{
		if(AffectedAccounts.FirstOrDefault(i => i.Value.Id == id).Value is AccountEntry a)
			return a;

		a = Mcv.Accounts.Find(id, Id - 1)?.Clone();	

		AffectedAccounts[a.Address] = a;

		TransferECIfNeeded(a);

		return a;
	}

	public AccountEntry AffectAccount(AccountAddress address)
	{
		if(address == Net.God)
			return new AccountEntry {Address = address};

		if(AffectedAccounts.FirstOrDefault(i => i.Value.Address == address).Value is AccountEntry a)
			return a;
		
		a = Mcv.Accounts.Find(address, Id - 1).Clone();	

		AffectedAccounts[a.Address] = a;

		TransferECIfNeeded(a);

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
			ConsensusMemberLeavers = svotes.SelectMany(i => i.MemberLeavers).Distinct()
											.Where(x => Members.Any(j => j.Id == x) && svotes.Count(b => b.MemberLeavers.Contains(x)) >= min)
											.Order().ToArray();

			ConsensusViolators = svotes.SelectMany(i => i.Violators).Distinct()
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
	
	public IEnumerable<EntityId> ProposeViolators()
	{
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
		Candidates				= Id == 0 ? new()										 : Previous.Candidates;
		Members					= Id == 0 ? new()										 : Previous.Members;
		Funds					= Id == 0 ? new()										 : Previous.Funds;
		BandwidthAllocations	= Id == 0 ? new long[Net.BandwidthAllocationDaysMaximum] : Previous.BandwidthAllocations.Clone() as long[];
		Spacetimes				= Id == 0 ? new long[1]									 : Previous.Spacetimes.Clone() as long[];
		
		NextAccountEids	= new ();

		AffectedCandidates.Clear();
		AffectedAccounts.Clear();

		foreach(var i in Mcv.Tables)
		{
			AffectedByTable(i).Clear();
			NextEidsByTable(i).Clear();
		}

		RestartExecution();

		foreach(var t in transactions.Where(t => t.Operations.All(i => i.Error == null)).Reverse())
		{
			var s = Mcv.Accounts.Find(t.Signer, Id);

			if(t.Signer != Net.God)
			{
				if(s == null)
				{
					foreach(var o in t.Operations)
						o.Error = Operation.NotFound;
					
					continue;
				}
	
				if(t.Nid != s.LastTransactionNid + 1)
				{
					foreach(var o in t.Operations)
						o.Error = Operation.NotSequential;
					
					continue;
				}
			}

			s = AffectAccount(t.Signer);

			foreach(var o in t.Operations)
			{
				o.Signer			= s;
				o.EnergyFeePayer	= null;
				o.EnergySpenders	= [];
				o.SpacetimeSpenders	= [];
				o.EnergyConsumed	= ConsensusECEnergyCost;

				o.Execute(Mcv, this);

				if(o.Error != null)
					goto start;
			
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
						goto start;
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
						goto start;
					}
				
					if(i.EnergyNext < 0)
					{
						o.Error = Operation.NotEnoughEnergyNext;
						goto start;
					}
				}

				foreach(var i in o.SpacetimeSpenders)
				{
					if(i.Spacetime < 0)
					{
						o.Error = Operation.NotEnoughSpacetime;
						goto start;
					}
				}
			}
						
			s.LastTransactionNid++;
		}

		FinishExecution();
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

		Members		= Members.ToList();
		Funds		= Funds.ToList();

		CopyConfirmed();
		
		foreach(var t in ConsensusTransactions)
		{
			foreach(var o in t.Operations)
			{
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
			var a = AffectAccount(i.Id);
			
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

		if(Id > 0 && ConsensusTime.Days != Previous.ConsensusTime.Days) /// day switched
		{
			var d = ConsensusTime.Days - Previous.ConsensusTime.Days;

			//d %= Time.FromYears(1).Days;

			BandwidthAllocations = d < BandwidthAllocations.Length ? [..BandwidthAllocations[d..], ..new long[d]] : new long[d];

			foreach(var i in Members.Select(i => AffectAccount(i.Id)))
			{
				i.EnergyNext	+= d * Net.ECDayEmission / Members.Count;
				i.Spacetime += d * (Net.BDDayEmission + Spacetimes[0]) / Members.Count;
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
		writer.Write(BandwidthAllocations, writer.Write7BitEncodedInt64);
		writer.Write(Funds);
		writer.Write(Spacetimes, writer.Write7BitEncodedInt64);

		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusECEnergyCost);
		writer.Write7BitEncodedInt(ConsensusOverloadRound);
	}

	public virtual void ReadBaseState(BinaryReader reader)
	{
		Id						= reader.Read7BitEncodedInt();
		Hash					= reader.ReadHash();
		BandwidthAllocations	= reader.ReadArray(reader.Read7BitEncodedInt64);
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
