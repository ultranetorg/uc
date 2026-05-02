using System.Diagnostics;

namespace Uccs.Net;

public abstract class OutwardOperation : Operation
{	
	public abstract void ConfirmedExecute(Execution execution, OutwardTransaction task);
}

public class OutwardTransaction
{
	public int				Id;
	public Time				Expiration;
	public AutoId			Generator;
	public AutoId			User;
	public OutwardOperation	Operation;

	Net						Net;

	public OutwardTransaction(Net net)
	{
		Net = net;
	}

	public virtual void WriteBaseState(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(User);
		writer.Write(Generator);
		writer.Write(Expiration);
		writer.Write(Net.Constructor.TypeToCode(Operation.GetType())); 
		Operation.Write(writer); 
	}

	public virtual void ReadBaseState(Reader reader)
	{
		Id			= reader.Read7BitEncodedInt();
		User		= reader.Read<AutoId>();
		Generator	= reader.Read<AutoId>();
		Expiration	= reader.Read<Time>();
		Operation	= Net.Constructor.Construct(typeof(Operation), reader.ReadUInt32()) as OutwardOperation;
		Operation.Read(reader); 
	}
}


public abstract class Round : IBinarySerializable
{
	public int											Id;
	public int											Try;
	public int											TargetId	=> Id - Net.P;
	public int											VotingId	=> Id + Net.P;
	public Round										Previous	=> Mcv.FindRound(Id - 1);
	public Round										Next		=> Mcv.FindRound(Id + 1);
	public Round										Target		=> Mcv.FindRound(TargetId);
	public Round										Voting		=> Mcv.FindRound(Id + Net.P);
	public long											PerVoteOperationsMaximum		=> Mcv.Net.OperationsPerRoundMaximum / (Id < Mcv.JoinToVote ? 1 : Senders.Count());
	//public long										PerVoteTransactionsLimit		=> Mcv.Net.TransactionsPerRoundMaximum / Members.Count;
	//public long										PerVoteBandwidthAllocationLimit	=> Mcv.Net.BandwidthAllocationPerRoundMaximum / Members.Count;

	public bool											IsLastInCommit => AffectedCount >= Net.AffectedCountMaximum;
	public virtual int									AffectedCount => AffectedMetas.Count + AffectedUsers.Count + Mcv.Tables.Sum(i => AffectedByTable(i).Count);

	public List<OutwardTransaction>						OutwardTransactions;
	public List<IccpTransaction>							IccTransactions;

	public List<Generator>								Candidates;
	public List<Generator>								Members;
	public IEnumerable<Generator>						Senders => Mcv.LastConfirmedRound.Members?.Where(i => i.Since <= Id && Id <= i.Till) ?? [];
	public IEnumerable<Generator>						Voters => Id < Mcv.JoinToVote ? [new Generator {User = AutoId.God}] 
																						 : 
																						 Senders.OrderByHash(i => i.User.Raw, [(byte)(Try>>24), (byte)(Try>>16), (byte)(Try>>8), (byte)Try, ..Mcv.LastConfirmedRound.Hash]).Take(Mcv.RequiredVotersMaximum);

	public List<Vote>									Votes = [];
	public List<AutoId>									Forkers = [];
	public List<Vote>									VotesOfTry = [];
	public List<Vote>									Payloads = [];
	public List<Vote>									Selected = [];

	public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.User).SelectMany(i => i.Transactions);
	public IEnumerable<Transaction>						Transactions => Confirmed ? ConsensusTransactions : OrderedTransactions;

	public Time											ConsensusTime;
	public Transaction[]								ConsensusTransactions;
	public AutoId[]										ConsensusMemberLeavers;
	public AutoId[]										ConsensusViolators;
	public AccountAddress[]								ConsensusFundJoiners = [];
	public AccountAddress[]								ConsensusFundLeavers = [];
	public long											ConsensusEnergyCost;
	//public int											ConsensusOverloadRound;
	public byte[][]										ConsensusIncomingTransfers;
	public IccpTransferResult[]					ConsensusOutgoingTransfers;
	public OutwardResult[]								ConsensusOutwards = {};

	public bool											Confirmed = false;
	public byte[]										Hash;

	public List<AccountAddress>							Funds;
	public long[]										Spacetimes;
	public long[]										Bandwidths;

	public Dictionary<MetaId, MetaEntity>				AffectedMetas = new();
	public Dictionary<AutoId, User>						AffectedUsers = new();
	public TableState<AutoId, Friend>					Friends;
	public Dictionary<int, int>[]						NextEids;

	public Mcv											Mcv;
	public McvNet										Net => Mcv.Net;

	public abstract long								UserAllocationFee();
	public virtual void									CopyConfirmed(){}
	public virtual void									RegisterForeign(Operation operation, Time time){}

	public Round FindParent(int level)
	{
		var p = Target;

		for(int i = 0; i < level; i++)
		{
			p = p.Target;

			if(p == null)
				break;
		}

		return p;
	}

	public int MinimalFilling
	{
		get
		{
			var n = Senders.Count();

			if(n == 1)	return 1;
			if(n == 2)	return 2;
			if(n == 4)	return 3;
	
			return n * 2/3;
		}
	}

	public int MinimumForConsensus
	{
		get
		{
			var n = Voters.Count();

			if(n == 1)	return 1;
			if(n == 2)	return 2;
			if(n == 4)	return 3;
	
			return n * 2/3;
		}
	}

	public Round(Mcv c)
	{
		Mcv = c;
		NextEids = Mcv.Tables.Select(i => new Dictionary<int, int>()).ToArray();
		Friends		= new (c.Friends);
	}

	public override string ToString()
	{
		return $"Id={Id}, Try={Try}, V/VoT/P={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), {(Confirmed ? "Confirmed, " : "")}Members={Members?.Count}, ConfirmedTime={ConsensusTime}, Hash={Hash?.ToHex()}";
	}

//	public void Update()
//	{
//		foreach(var i in New)
//		{
//			Mcv.Check(i);
//
//			if(i.Status == VoteStatus.OK)
//			{	
//				if(i.Try == Try)
//	 			{	
//					VotesOfTry.Add(i);
//					
//					if(i.Transactions.Any())
//						Payloads.Add(i);
//
//					if(Id >= Mcv.JoinToVote && SelectedVoters.Any(j => j.User == i.User))
//						SelectedArrived.Add(i);
//				}
//			}
//		}
//
//		New.Clear();
//	}

	public void ReUpdate()
	{
		VotesOfTry.Clear();	
		Payloads.Clear();
		Selected.Clear();

		foreach(var i in Votes)
		{
			if(i.Try == Try)
	 		{	
				Mcv.Check(i);

				if(i.Status == VoteStatus.OK || i.User == AutoId.God)
				{	
					VotesOfTry.Add(i);
					
					if(i.Transactions.Any())
						Payloads.Add(i);

					if(Voters.Any(j => j.User == i.User))
						Selected.Add(i);
				}
			}
		}
	}

	public virtual System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Users)		return AffectedUsers;
		if(table == Mcv.Metas)		return AffectedMetas;
		if(table == Mcv.Friends)	return Friends.Affected;

		throw new IntegrityException();
	}

	public virtual S FindState<S>(TableBase table) where S : TableStateBase
	{
		if(table == Mcv.Friends)	return Friends as S;

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
		///if(Selected.Count < MinimumForConsensus)
		///	return null;

		var min = MinimumForConsensus;
					
		ConsensusEnergyCost	= Id == 0 ? 0 : Previous.ConsensusEnergyCost;
		//ConsensusOverloadRound	= Id == 0 ? 0 : Previous.ConsensusOverloadRound;

		//var tn = all.Sum(i => i.Transactions.Sum(i => i.Operations.Length));
		//var e = tn - Mcv.Net.OperationsPerRoundMaximum;
		//
		//if(e > 0)
		//{
		//	//ConsensusECEnergyCost *= Mcv.Net.OverloadFeeFactor;
		//	//ConsensusOverloadRound = Id;
		//
		//	var gi = all.AsEnumerable().GetEnumerator();
		//
		//	do
		//	{
		//		if(!gi.MoveNext())
		//			gi.Reset();
		//		
		//		if(gi.Current.Transactions.Sum(i => i.Operations.Length) > Net.ex PerVoteTransactionsLimit)
		//		{
		//			e--;
		//			gi.Current.TransactionCountExcess++;
		//		}
		//	}
		//	while(e > 0);
		//
		//	foreach(var i in all.Where(i => i.TransactionCountExcess > 0))
		//	{
		//		//var ts = new Transaction[i.Transactions.Length - i.TransactionCountExcess];
		//		//Array.Copy(i.Transactions, i.TransactionCountExcess, ts, 0, ts.Length);
		//		i.Transactions = i.Transactions[..^i.TransactionCountExcess];
		//	}
		//}
		//else 
		//{
		//	if(ConsensusECEnergyCost > 1 && Id - ConsensusOverloadRound > Mcv.P)
		//		ConsensusECEnergyCost /= Net.OverloadFeeFactor;
		//}
		
		if(Id > 0)
		{
			ConsensusTime = Previous.ConsensusTime;

			for(int n = 0; n < 8; n++) /// 8 means 256 seconds ~= 4 min maximal deviation
			{
				var g = Selected.GroupBy(i => i.Time.Seconds >> n).MaxBy(i => i.Count());

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

		var txs = Payloads.OrderBy(i => i.User).SelectMany(i => i.Transactions).ToArray();

		Execute(txs);

		ConsensusTransactions = txs.Where(i => i.OverallError == null).ToArray();

		if(Id < Net.P)
		{
			ConsensusMemberLeavers = [];
			ConsensusViolators = [];
			ConsensusIncomingTransfers = [];
			ConsensusOutgoingTransfers = [];
			Funds = [];
			//ConsensusFundJoiners = [];
			//ConsensusFundLeavers = [];
		}
		else
		{
			var svotes = Id < Mcv.JoinToVote ? [] : Selected.ToArray();

			ConsensusMemberLeavers = svotes	.SelectMany(i => i.Leavers).Distinct()
											.Where(x => Members.Any(j => j.User == x) && svotes.Count(b => b.Leavers.Contains(x)) >= min)
											.Order().ToArray();

			ConsensusViolators = svotes	.SelectMany(i => i.Violators).Distinct()
										.Where(x => svotes.Count(b => b.Violators.Contains(x)) >= min)
										.Order().ToArray();

			ConsensusIncomingTransfers = svotes.SelectMany(i => i.FriendTransferRequests).Distinct(Bytes.EqualityComparer)
													.Where(v => svotes.Count(i => i.FriendTransferRequests.Contains(v, Bytes.EqualityComparer)) >= min)
													.Order(Bytes.Comparer).ToArray();

			ConsensusOutgoingTransfers = svotes.SelectMany(i => i.FriendTransferConfirmations).Distinct()
														 .Where(v => svotes.Count(i => i.FriendTransferConfirmations.Contains(v)) >= min)
														 .Order().ToArray();

			ConsensusOutwards = svotes	.SelectMany(i => i.OutwardResults)
										.Distinct()
										.Where(x => OutwardTransactions.Any(o => o.User == x.User && o.Id == x.Id) && svotes.Count(b => b.OutwardResults.Contains(x)) >= min)
										.Order().ToArray();

			Elect(svotes, min);
		}

		Hashify(); /// depends on Mcv.GraphHash 

		Mcv.Log?.Report(this, $"Summarize {this} - Payloads={string.Join(" ", Payloads.Select(i => i.User))} - VotesOfTry={string.Join(" ", VotesOfTry.Select(i => i.User))} - Votes={string.Join(" ", Votes.Select(i => i.User))}");

		return Hash;
	}
	
	public IEnumerable<AutoId> ProposeViolators()
	{
		return Forkers;
	}

	public IEnumerable<AutoId> ProposeMemberLeavers(AutoId generator)
	{
		if(Id < Mcv.JoinToVote + Mcv.JoinToVote - 1)
			return [];

		var prevs = Enumerable.Range(TargetId - Net.P, Net.P).Select(Mcv.FindRound);

		if(prevs.Any(i => i == null)) /// if just synchronized
			return [];

		var l = Target.Senders.Where(i => !Target.VotesOfTry.Any(v => v.User == i.User) && /// did not sent a vote
										  !prevs.Any(r => r.VotesOfTry.Any(v => v.User == generator && v.Leavers.Contains(i.User)))) /// not yet proposed in prev [Pitch-1] rounds
							 .Select(i => i.User);
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

		Candidates			= Id == 0 ? []										 : Previous.Candidates; /// cloned in Execution.AffectCandidate
		Members				= Id == 0 ? []										 : Previous.Members;
		Funds				= Id == 0 ? []										 : Previous.Funds;
		Bandwidths			= Id == 0 ? new long[McvNet.BandwidthPeriodsMaximum] : Previous.Bandwidths;
		Spacetimes			= Id == 0 ? new long[1]								 : Previous.Spacetimes;
		OutwardTransactions	= Id == 0 ? []										 : Previous.OutwardTransactions;
		IccTransactions		= Id == 0 ? []										 : Previous.IccTransactions;

		AffectedMetas	= Id == 0 ? [] : new (Previous.AffectedMetas);
		AffectedUsers	= Id == 0 ? [] : new (Previous.AffectedUsers);
		
		NextEids = [..Mcv.Tables.Select(i => (Dictionary<int, int>)null)];
		for(int i = 0; i < NextEids.Length; i++)
		{
			NextEids[i] = Id == 0 ? [] : new(Previous.NextEids[i]); 
		}

		foreach(var i in Mcv.Tables)
			FindState<TableStateBase>(i)?.StartRoundExecution(this);

		foreach(var t in transactions)
		{
			var e = CreateExecution(t);
			
			t.EnergyConsumed	= 0;
			e.EnergySpenders	= [];
			e.SpacetimeSpenders	= [];
			e.EnergyCost		= ConsensusEnergyCost;

			var u = e.AffectSigner();

			if(u == null)
				continue;

			foreach(var o in t.Operations)
			{
				o.User = u;

				o.Execute(e);

				if(o.Error != null)
					break;

				u.Energy -= t.Boost;
				
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
			
			if(t.OverallError == null)
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
			AffectedUsers[i.Key] = i.Value;

		for(int t=0; t<Mcv.Tables.Length; t++)
			foreach(var i in execution.NextEids[t])
				NextEids[t][i.Key] = i.Value;

		Candidates			= execution.Candidates;
		Spacetimes			= execution.Spaces;
		Bandwidths			= execution.Bandwidths;
		OutwardTransactions	= execution.OutwardTransactions;
		IccTransactions		= execution.IccTransactions;

		Friends.Absorb(execution.Friends);
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

		OutwardTransactions	= [..OutwardTransactions];
		IccTransactions		= [..IccTransactions];

		CopyConfirmed();
		
		foreach(var (i, t) in ConsensusTransactions.Index())
		{
			t.Id = new (Id, i);
			
			foreach(var (j, o) in t.Operations.Index())
			{	
				o.Id = new (Id, i, (byte)j);
				RegisterForeign(o, ConsensusTime);
			}
		}

		var e = CreateExecution(null);

		ConfirmForeign(e);

		foreach(var t in OrderedTransactions)	t.Status = TransactionStatus.FailedOrNotFound;
		foreach(var t in ConsensusTransactions)	t.Status = TransactionStatus.Confirmed;

		Members = [..Members];

		foreach(var i in ConsensusViolators.Select(i => Members.Find(j => j.User == i)))
		{
			e.AffectUser(i.User).AverageUptime = 0;
			Members.Remove(i);
		}

		foreach(var i in ConsensusMemberLeavers.Select(i => Members.Find(j => j.User == i)))
		{
			var a = e.AffectUser(i.User);
			
			a.AverageUptime = (a.AverageUptime + Id - i.Since)/(a.AverageUptime == 0 ? 1 : 2);
			
			Members.Remove(i);
			var m = i .Clone();
			m.Till = Id + Net.P;
			Members.Add(m);

			Mcv.Log?.Report(this, $"Left - Round {Id} - {a}");

			///if(Mcv.Settings.Generators.Any(i => i.Id == a.Id))
			///	Debugger.Break();
		}

		Members.RemoveAll(i => i.Till < Id);

		foreach(var i in e.Candidates.TakeLast(Mcv.Net.MembersLimit - Members.Count).ToArray())
		{
			var c = e.AffectCandidate(i.User);
			
			c.Since = Id + Mcv.JoinToVote;
			c.Till = int.MaxValue - Mcv.JoinToVote;
			
			e.Candidates.Remove(i);
			Members.Add(c);
		}

		Funds = [..Funds];
		Funds.RemoveAll(i => ConsensusFundLeavers.Contains(i));
		Funds.AddRange(ConsensusFundJoiners);


		if(Id > 0)
		{
			var d = ConsensusTime.Days - Previous.ConsensusTime.Days;
			
			if(d > 0) /// day switched
			{
				e.AffectSpaces();

				foreach(var i in Members.Select(i => e.AffectUser(i.User)))
				{
					i.EnergyNext += d * Net.EnergyDailyEmission / Members.Count;
					i.Spacetime	 += d * (Net.SpacetimeDayEmission + e.Spaces.Take(d).Sum()) / Members.Count;
				}
				
				if(d <= e.Spaces.Length)
					e.Spaces = e.Spaces[d..];
			}
	
			var h = ConsensusTime.Hours - Previous.ConsensusTime.Hours;
	
			if(h > 0) /// hours switched
			{
				e.Bandwidths = h < e.Bandwidths.Length ? [..e.Bandwidths[h..], ..new long[h]] : new long[h];
			}
		}

		Absorb(e);
		
		Confirmed = true;
		Mcv.LastConfirmedRound = this;
		Mcv.Tail.RemoveAll(i => i.Id < Id);
		Mcv.Confirmed?.Invoke(this);
	}

	public virtual void	ConfirmForeign(Execution execution)
	{
		///	var ows = Outwards.Where(i => i.Operation is IccOperation o && execution.Friends.Find(o.ToNet).OutStatus != IccTransferStatus.FormedAndPending).ToArray();

		foreach(var txs in IccTransactions.GroupBy(i => i.ToNet))
		{
			foreach(var i in txs)
				i.OutgoingPrelock(execution);

			var s = execution.Friends.Affect(txs.Key);

			s.OutStatus = IccTransferStatus.FormedAndPending;
			s.LastOutgoingTransfer = new IccpTransfer
									 {
									 	Id = s.LastOutgoingTransfer == null ? 0 : (s.LastOutgoingTransfer.Id + 1),
									 	Transactions = [..txs]
									 };

			Mcv.FriendBlockFormed?.Invoke(execution, s);
		}


		foreach(var i in ConsensusIncomingTransfers)
		{
			var t = Mcv.FriendTransferRequests.Find(j => Bytes.Equal(j.Hash, i))
					??
					throw new ConfirmationException(this, []);

			var f = execution.Friends.Affect(t.From);
			f.LastIncomingTransfer = new IccpTransferResult {Hash = t.Hash, Results = new bool[t.Transactions.Length]};

			for(int j = 0; j < t.Transactions.Length; j++)
			{	
				f.LastIncomingTransfer.Results[j] = t.Transactions[j].IncomingExecute(execution);
			}

			Mcv.FriendTransferRequests.Remove(t);
		}

 		foreach(var i in ConsensusOutgoingTransfers)
 		{
 			if(!Mcv.FriendTransferResults.TryGetValue(i, out var to))
				throw new ConfirmationException(this, []);

			var f = execution.Friends.Affect(to);

			for(int j = 0; j < i.Results.Length; j++)
			{
				if(i.Results[j])
					f.LastOutgoingTransfer.Transactions[j].OutgoingConfirm(execution);
				else
					f.LastOutgoingTransfer.Transactions[j].OutgoingRollback(execution);
			}
 
			f.OutStatus = IccTransferStatus.Confirmed;
 
 			Mcv.FriendTransferResults.Remove(i);
 		}

//		foreach(var i in Mcv.Subnets.TailGraphEntities.Where(i => i.OutStatus == OutTransactionStatus.Confirmed && i.OutOperations.Any()))
//		{
//			//Call(new TransactionIcca
//			//		{ 
//			//		Net			= i.Name,
//			//		Nonce		= i.OutNonce + 1,
//			//		Peers		= r.Members.Select(i => i.GraphPpcIPs[0]).ToArray(),
//			//		Operations	= i.OutOperations
//			//		}, Flow);
//
//			i.OutOperations = [];
//			i.OutNonce++;
//			i.OutStatus = OutTransactionStatus.Sent;
//		}

		foreach(var i in ConsensusOutwards)
		{
			var e = OutwardTransactions.Find(j => j.User == i.User && j.Id == i.Id);

			if(i.Approved)
			{
				e.Operation.ConfirmedExecute(execution, e);
				OutwardTransactions.Remove(e);
			} 
			else
				execution.AffectUser(e.Generator).AverageUptime -= 10;
		}

		OutwardTransactions.RemoveAll(i => i.Expiration < execution.Time);
	}

	public void Hashify()
	{
		var s = new Blake2Stream();
		var w = new Writer(s);

		w.Write(Mcv.GraphHash);
		w.Write(Id > 0 ? Previous.Hash : Mcv.Net.Cryptography.ZeroHash);
		Write(w);

		Hash = s.Hash;
	}

	public virtual void WriteGraphState(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(Hash);
		writer.Write(Bandwidths, writer.Write7BitEncodedInt64);
		writer.Write(Funds);
		writer.Write(Spacetimes, writer.Write7BitEncodedInt64);

		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusEnergyCost);
		writer.Write(OutwardTransactions, i => i.WriteBaseState(writer));
		writer.Write(IccTransactions, i =>	{ 
												writer.Write(Net.Constructor.TypeToCode(i.GetType())); 
												i.Write(writer);
											});
	}

	public virtual void ReadGraphState(Reader reader)
	{
		Id						= reader.Read7BitEncodedInt();
		Hash					= reader.ReadHash();
		Bandwidths				= reader.ReadArray(reader.Read7BitEncodedInt64);
		Funds					= reader.ReadList<AccountAddress>();
		Spacetimes				= reader.ReadArray(reader.Read7BitEncodedInt64);

		ConsensusTime			= reader.Read<Time>();
		ConsensusEnergyCost		= reader.Read7BitEncodedInt64();
		OutwardTransactions		= reader.ReadList(() =>	{ 
															var o =	new OutwardTransaction(Net);
															o.ReadBaseState(reader);
															return o;
														});
		IccTransactions			= reader.ReadList(() => {
 										 					var o = Net.Constructor.Construct(typeof(IccpTransaction), reader.ReadUInt32()) as IccpTransaction;
 										 					o.Read(reader); 
 										 					return o; 
 														});
	}

	public virtual void Write(Writer writer)
	{
		writer.Write7BitEncodedInt(Id);
		writer.Write(ConsensusTime);
		writer.Write7BitEncodedInt64(ConsensusEnergyCost);
		writer.Write(ConsensusMemberLeavers);
		writer.Write(ConsensusViolators);
		writer.Write(ConsensusFundJoiners);
		writer.Write(ConsensusFundLeavers);
		writer.Write(ConsensusTransactions, i => i.WriteConfirmed(writer));
		writer.Write(ConsensusIncomingTransfers, writer.Write);
		writer.Write(ConsensusOutgoingTransfers);
		writer.Write(ConsensusOutwards);
	}

	public virtual void Read(Reader reader)
	{
		Id										= reader.Read7BitEncodedInt();
		ConsensusTime							= reader.Read<Time>();
		ConsensusEnergyCost						= reader.Read7BitEncodedInt64();
		ConsensusMemberLeavers					= reader.ReadArray<AutoId>();
		ConsensusViolators						= reader.ReadArray<AutoId>();
		ConsensusFundJoiners					= reader.ReadArray<AccountAddress>();
		ConsensusFundLeavers					= reader.ReadArray<AccountAddress>();
		ConsensusTransactions					= reader.Read(() =>	new Transaction {Net = Mcv.Net, Round = this}, t => t.ReadConfirmed(reader)).ToArray();
		ConsensusIncomingTransfers			= reader.ReadArray(reader.ReadHash);
		ConsensusOutgoingTransfers	= reader.ReadArray<IccpTransferResult>();
		ConsensusOutwards						= reader.ReadArray<OutwardResult>();
	}

	public void Save(Writer writer)
	{
		Write(writer);
		writer.Write(Hash);
	}

	public void Load(Reader reader)
	{
		Read(reader);
		Hash = reader.ReadHash();
	}
}
