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

	public int											Try;
	public DateTime										FirstArrivalTime = DateTime.MaxValue;

	public IEnumerable<Generator>						Voters => VotersRound.Members;
	public IEnumerable<Generator>						SelectedVoters => Id < Mcv.JoinToVote ? [new Generator {Id = AutoId.God, Address = Mcv.God.Address}] : (Voters ?? []).OrderByHash(i => i.Address.Bytes, [(byte)(Try>>24), (byte)(Try>>16), (byte)(Try>>8), (byte)Try, ..VotersRound.Hash]).Take(Mcv.RequiredVotersMaximum);

	public List<Vote>									Votes = [];
	public List<AutoId>									Forkers = [];
	public List<Vote>									VotesOfTry = [];
	public List<Vote>									Payloads = [];
	public List<Vote>									SelectedArrived = [];
	public IGrouping<byte[], Vote>						MajorityOfRequiredByParentHash;

	public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
	public IEnumerable<Transaction>						Transactions => Confirmed ? ConsensusTransactions : OrderedTransactions;

	public Time											ConsensusTime;
	public Transaction[]								ConsensusTransactions;
	public AutoId[]										ConsensusMemberLeavers;
	public AutoId[]										ConsensusViolators;
	public AccountAddress[]								ConsensusFundJoiners = [];
	public AccountAddress[]								ConsensusFundLeavers = [];
	public long											ConsensusECEnergyCost;
	public int											ConsensusOverloadRound;
	public byte[][]										ConsensusNnStates;

	public bool											Confirmed = false;
	public byte[]										Hash;

	public List<Generator>								Candidates;
	public List<Generator>								Members;
	public List<AccountAddress>							Funds;
	public long[]										Spacetimes;
	public long[]										Bandwidths;

	public Dictionary<MetaId, MetaEntity>				AffectedMetas = new();
	public Dictionary<AutoId, User>						AffectedAccounts = new();
	public Dictionary<int, int>[]						NextEids;

	public Mcv											Mcv;
	public McvNet										Net => Mcv.Net;

	public abstract long								AccountAllocationFee();
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
			var r = SelectedArrived;
		
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

	public void Update()
	{
		VotesOfTry.Clear();	
		Payloads.Clear();
		SelectedArrived.Clear();
	
 		VotesOfTry.AddRange(Votes.Where(i => i.Try == Try));
		Payloads.AddRange(VotesOfTry.Where(i => i.Transactions.Any()));
		SelectedArrived.AddRange(VotesOfTry.Where(i => SelectedVoters.Any(j => j.Address == i.Generator)));
		MajorityOfRequiredByParentHash	= SelectedArrived.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).MaxBy(i => i.Count());
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Users)	return AffectedAccounts;
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
	
	public virtual void Elect(Vote[] votes, int gq)
	{
	}

	public byte[] Summarize()
	{
		if(!VotesOfTry.Any())
			return null;

		var min = MinimumForConsensus;
		var all = VotesOfTry.ToArray();
		var svotes = Id < Mcv.JoinToVote ? [] : SelectedArrived.ToArray();
					
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
			ConsensusTime = Previous.ConsensusTime;

			for(int n = 0; n < 8; n++) /// 8 means 256 seconds ~= 4 min maximal deviation
			{
				var g = SelectedArrived.GroupBy(i => i.Time.Seconds >> n).MaxBy(i => i.Count());

				if(g.Count() >= min)
				{	
					var t = new Time((int)(g.Sum(i => (long)i.Time.Seconds)/g.Count()));

					if(t > Previous.ConsensusTime)
					{
						ConsensusTime = t;
						break;
					}
				}
			}
		}

		var txs = all.OrderBy(i => i.Generator).SelectMany(i => i.Transactions).ToArray();

		Execute(txs);

		ConsensusTransactions = txs.Where(i => i.Successful).ToArray();

		if(Id < Mcv.P)
		{
			ConsensusMemberLeavers = [];
			ConsensusViolators = [];
			ConsensusNnStates = [];
			//ConsensusFundJoiners = [];
			//ConsensusFundLeavers = [];
		}
		else
		{
			ConsensusMemberLeavers = svotes	.SelectMany(i => i.MemberLeavers).Distinct()
											.Where(x => Members.Any(j => j.Id == x) && svotes.Count(b => b.MemberLeavers.Contains(x)) >= min)
											.Order().ToArray();

			ConsensusViolators = svotes	.SelectMany(i => i.Violators).Distinct()
										.Where(x => svotes.Count(b => b.Violators.Contains(x)) >= min)
										.Order().ToArray();

			ConsensusNnStates	= svotes.SelectMany(i => i.NntBlocks).Distinct(Bytes.EqualityComparer)
										.Where(v => svotes.Count(i => i.NntBlocks.Contains(v, Bytes.EqualityComparer)) >= min)
										.Order(Bytes.Comparer).ToArray();

			Elect(svotes, min);
		}

		Hashify(); /// depends on Mcv.BaseHash 

		return Hash;
	}
	
	public IEnumerable<AutoId> ProposeViolators()
	{
		return Forkers;
	}

	public IEnumerable<AutoId> ProposeMemberLeavers(AccountAddress generator)
	{
		if(Id < Mcv.JoinToVote + Mcv.JoinToVote - 1)
			return [];

		var prevs = Enumerable.Range(ParentId - Mcv.P, Mcv.P).Select(Mcv.FindRound);

		var l = Parent.Voters.Where(i => !Parent.VotesOfTry.Any(v => v.Generator == i.Address) && /// did not sent a vote
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

	public virtual void Execute(IEnumerable<Transaction> transactions)
	{
		if(Confirmed)
			throw new IntegrityException();

		if(Id != 0 && Previous == null)
			return;

		foreach(var t in transactions)
		{	
			t.Error = null;

			foreach(var o in t.Operations)
				o.Error = null;
		}

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

		foreach(var t in transactions.Reverse())
		{
			var e = CreateExecution(t);
			
			t.EnergyConsumed	= 0;
			e.EnergySpenders	= [];
			e.SpacetimeSpenders	= [];
			e.ECEnergyCost		= ConsensusECEnergyCost;

			var u = e.AffectSigner();

			if(u == null)
				continue;

			foreach(var o in t.Operations)
			{
				o.User = u;

				o.Execute(e);

				if(o.Error != null)
					break;

				u.Energy -= t.Bonus;

				if(u.Energy < 0)
				{
					o.Error = Operation.NotEnoughEnergy;
					break;
				}
				
				foreach(var i in e.EnergySpenders)
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

					if(i.BandwidthTodayAvailable < 0)
					{
						o.Error = Operation.NotEnoughBandwidth;
						break;
					}
				}

				foreach(var i in e.SpacetimeSpenders)
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
				u.LastNonce++;
	
				Absorb(e);
			}
		}

		FinishExecution();
	}

	public virtual void Absorb(Execution execution)
	{
		foreach(var i in execution.AffectedMetas)
			AffectedMetas[i.Key] = i.Value;

		foreach(var i in execution.AffectedUsers)
			AffectedAccounts[i.Key] = i.Value;

		for(int t=0; t<Mcv.Tables.Length; t++)
			foreach(var i in execution.NextEids[t])
				NextEids[t][i.Key] = i.Value;

		if(execution.Candidates != null)	Candidates	= execution.Candidates;
		if(execution.Spaces != null)		Spacetimes	= execution.Spaces;
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

		///if(Members == null)
			Execute(ConsensusTransactions);

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

		foreach(var t in OrderedTransactions)	t.Status = TransactionStatus.FailedOrNotFound;
		foreach(var t in ConsensusTransactions)	t.Status = TransactionStatus.Confirmed;

		Members = [..Members];

		foreach(var i in ConsensusViolators.Select(i => Members.Find(j => j.Id == i)))
		{
			e.AffectUser(i.Id).AverageUptime = 0;
			Members.Remove(i);
		}

		foreach(var i in ConsensusMemberLeavers.Select(i => Members.Find(j => j.Id == i)))
		{
			var a = e.AffectUser(i.Id);
			
			a.AverageUptime = (a.AverageUptime + Id - i.CastingSince)/(a.AverageUptime == 0 ? 1 : 2);
			Members.Remove(i);
		}

		foreach(var i in e.Candidates.TakeLast(Mcv.Net.MembersLimit - Members.Count).ToArray())
		{
			var c = e.AffectCandidate(i.Id);
			
			c.CastingSince = Id + Mcv.JoinToVote;
			
			e.Candidates.Remove(i);
			Members.Add(c);
		}

		Funds = [..Funds];
		Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
		Funds.AddRange(ConsensusFundJoiners);

		if(Id > 0 && ConsensusTime.Days != Previous.ConsensusTime.Days) /// day switched
		{
			var d = ConsensusTime.Days - Previous.ConsensusTime.Days;

			e.Bandwidths = d < e.Bandwidths.Length ? [..e.Bandwidths[d..], ..new long[d]] : new long[d];

			foreach(var i in Members.Select(i => e.AffectUser(i.Id)))
			{
				i.EnergyNext += d * Net.ECDayEmission / Members.Count;
				i.Spacetime	 += d * (Net.SpacetimeDayEmission + e.Spaces.Take(d).Sum()) / Members.Count;
			}
			
			if(d <= e.Spaces.Length)
				e.Spaces = e.Spaces[d..];
		}

		Absorb(e);
		
		Confirmed = true;
		Mcv.LastConfirmedRound = this;
		Mcv.Confirmed?.Invoke(this);
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
