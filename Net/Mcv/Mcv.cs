using System.Data;
using System.Diagnostics;
using System.Text.Json;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void BlockDelegate(Vote b);
public delegate void RoundDelegate(Round b);

public enum McvTable : byte
{
	Meta, User, _Last = User
}

public enum VoteStatus
{
	None,	
	OK,
	Invalid,
	TooOld,
	AlreadyExists,
	Fork,
	Violator,
	NotMemeber,
	NotVoter,
	AccessDenied,
	PerVoteOperationsMaximumExceeded,
	InvalidTransaction
}

public abstract class Mcv /// Mutual chain voting
{
	public const int							P = 6; /// pitch
	public const int							RequiredVotersMaximum = 21; 
	public const int							JoinToVote = P + 1;
	public const int							LastGenesisRound = JoinToVote - 1;
	public const int							TransactionPlacingLifetime = P*2;
	public static readonly Unit					BalanceMin = new Unit(0.000_000_001);
	public const int							EntityRentYearsMin = 1;
	public const int							EntityRentYearsMax = 10;
	public const int							TransactionQueueLimit = 1000;
	public static readonly Time					Forever = Time.FromYears(30);

	public readonly static AccountKey			God = new AccountKey([1, ..new byte[31]]);
	public readonly static string				GodName = "";

	public object								Lock = new();
	public McvSettings							Settings;
	public McvNet								Net;
	public IClock								Clock;
	public Log									Log;
	public string								Databasepath;
	public string								Datapath;

	public RocksDb								Rocks;
	public byte[]								GraphState;
	public byte[]								GraphHash;
	static readonly byte[]						GraphStateKey = [0x01];
	static readonly byte[]						__GraphHashKey = [0x02];
	static readonly byte[]						ChainStateKey = [0x03];
	static readonly byte[]						GenesisKey = [0x04];
	public MetaTable							Metas;
	public UserTable							Users;
	public TableBase[] 							Tables;
	public int									Size => Tables.Sum(i => i.Size);
	public BlockDelegate						VoteAdded;
	public RoundDelegate						ConsensusReached;
	public RoundDelegate						ConsensusFailed;
	public RoundDelegate						Confirmed;

	List<Round>									_Tail = [];
	public List<Round>							Tail
												{
													set => _Tail = value;
													get 
													{
														if(!Monitor.IsEntered(Lock))
															Debugger.Break();
	
														return _Tail;
													}
												}

	public Round								LastConfirmedRound;
	public Round								LastCommitedRound;
	public Round								LastNonEmptyRound => Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
	public Round								LastPayloadRound => Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
	public Round								NextTargetRound => GetRound(LastConfirmedRound.Id + 1);
	public Round								NextVotingRound => GetRound(LastConfirmedRound.Id + 1 + P);

	public List<NnpBlock>						NnBlocks = [];

	public const string							ChainFamilyName = "Chain";
	public ColumnFamilyHandle					ChainFamily	=> Rocks.GetColumnFamily(ChainFamilyName);

	public static int							GetValidityPeriod(int rid) => rid + P;

	protected abstract void						CreateTables(string databasepath);
	protected abstract void						GenesisInitilize(Round vote);
	public abstract Round						CreateRound();
	public abstract Vote						CreateVote();
	public abstract Generator					CreateGenerator();
	public abstract CandidacyDeclaration		CreateCandidacyDeclaration();
	public abstract void						FillVote(Vote vote);

	Genesis Genesis;

	public Mcv()
	{
	}

	protected Mcv(McvNet net, McvSettings settings, string datapath, string databasepath, Genesis genesis, IClock clock)
	{
		Genesis = genesis;
		Clock = clock;

		lock(Lock)
		{
			Net = net;
			Settings = settings;
			Datapath = datapath;
			Databasepath = databasepath;
	
			CreateTables(databasepath);
	
			if(net.TablesCount != Tables.Count(i => !i.IsIndex))
				throw new IntegrityException();

			GraphHash = Net.Cryptography.ZeroHash;
	
			var g = Rocks.Get(GenesisKey);
	
			if(g == null)
			{
				Initialize();
			}
			else
			{
				if(g.SequenceEqual((new Genesis() as IBinarySerializable).Raw))
				{
					Load();
				}
				else
				{ 
					Clear();
					Initialize();
				}
			}
		}
	}

	public void Initialize()
	{
		if(Settings.Chain != null)
		{
			for(int i = 0; i <= LastGenesisRound; i++)
			{
				var v = CreateVote(); 

				v.RoundId	 = i;
				v.User		 = AutoId.God;
				v.Time		 = Time.Zero;
				v.TargetHash = i < P ? Net.Cryptography.ZeroHash : GetRound(i - P).Summarize();

				if(i == 0)
				{
 					var t = new Transaction {Net = Net, Nonce = 0, Expiration = 0};
 					t.Member = new(0, -1);
					t.User = GodName;
					t.AddOperation(Genesis);
 					v.AddTransaction(t);
 					t.Sign(God);

					GetRound(i).Payloads = [v];
				}
		
				v.Sign(God);
				Add(v, false);
				v.Round.VotesOfTry = v.Round.Selected = [v];
				
				GenesisInitilize(v.Round);
			}

			var r = GetRound(0);

			r.ConsensusEnergyCost = 1; ///1
			r.ConsensusFundJoiners = [Net.Father0Signer];

			r.Hashify();
			r.Confirm();
			Save(r);

			if(r.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null))))
				throw new IntegrityException("Genesis construction failed");
		}
	
		Rocks.Put(GenesisKey, (new Genesis() as IBinarySerializable).Raw);
	}

	public void Load()
	{
		GraphState = Rocks.Get(GraphStateKey);

		if(GraphState != null)
		{
			var r = new BinaryReader(new MemoryStream(GraphState));
	
			LastCommitedRound = CreateRound();
			LastCommitedRound.ReadGraphState(r);

			InsertRound(LastCommitedRound);

			Hashify();

			if(!GraphHash.SequenceEqual(Rocks.Get(__GraphHashKey)))
			{
				throw new IntegrityException();
			}
		}

		if(Settings.Chain != null)
		{
			var rd = new BinaryReader(new MemoryStream(Rocks.Get(ChainStateKey)));
		
			var lcr = FindRound(rd.Read7BitEncodedInt());
				
			if(GraphState == null) /// clear to avoid genesis loading issues, it must be created - not loaded
			{
				Clear();
				Initialize();
			} 
			else
			{
				LastConfirmedRound = LastCommitedRound;

				for(var i = LastCommitedRound.Id + 1; i <= lcr.Id; i++)
				{
					var r = FindRound(i);
				
					Tail.Insert(0, r);
				
					r.Confirmed = false;
					r.Confirm();
				}
			}
		}
	}

	public void Clear()
	{
		Tail.Clear();

		GraphState = null;
		GraphHash = Net.Cryptography.ZeroHash;

		LastCommitedRound = null;
		LastConfirmedRound = null;

		foreach(var i in Tables)
			i.Clear();

		Rocks.Remove(GraphStateKey);
		Rocks.Remove(__GraphHashKey);
		Rocks.Remove(ChainStateKey);
		Rocks.Remove(GenesisKey);

		Rocks.DropColumnFamily(ChainFamilyName);
		Rocks.CreateColumnFamily(new (), ChainFamilyName);
	}

	public void Stop()
	{
		Rocks.Dispose();
	}

	public void Add(Vote vote, bool process, bool check = true)
	{
		if(!Monitor.IsEntered(Lock))
			Debugger.Break();

		var r = GetRound(vote.RoundId);

		vote.Round = r;

		r.Votes.Add(vote);
		///r.Update();
	
		foreach(var t in vote.Transactions)
		{
			t.Round = r;
			t.Status = TransactionStatus.Placed;
		}

		if(process)
		{
			if(check)
				Check(vote);
	
			if(!check || vote.Status == VoteStatus.OK)
			{	
				if(vote.Try == r.Try)
	 			{	
					r.VotesOfTry.Add(vote);
					
					if(vote.Transactions.Any())
						r.Payloads.Add(vote);
	
					if(r.Id >= JoinToVote && r.Voters.Any(j => j.User == vote.User))
						r.Selected.Add(vote);
				}
			}
		}

		VoteAdded?.Invoke(vote);
	}

	public VoteStatus ProcessIncoming(Vote vote, Synchronization synchroniztion)
	{
		if(!vote.Valid)
			return VoteStatus.Invalid;

		if(LastConfirmedRound != null && vote.RoundId <= LastConfirmedRound.Id)
		{	
			//if(vote.RoundId <= LastConfirmedRound.Id - P)
				return VoteStatus.TooOld;
			//else
			//{
			//	var r = GetRound(vote.RoundId);
			//
			//	if(r.Votes.Any(i => Bytes.EqualityComparer.Equals(i.Signature, vote.Signature)))
			//		return VoteStatus.AlreadyExists;
			//
			//	Add(vote);
			//	
			//	return VoteStatus.OK;
			//
			//}
		}
		else if(synchroniztion != Synchronization.Synchronized || LastConfirmedRound != null && vote.RoundId > NextVotingRound.Id)
		{
			var r = GetRound(vote.RoundId);

			if(r.Votes.Any(i => Bytes.EqualityComparer.Equals(i.Signature, vote.Signature)))
				return VoteStatus.AlreadyExists;
	
			Add(vote, false);
				
			return VoteStatus.OK;
		}
		else
		{
			var r = GetRound(vote.RoundId);

			if(r.Votes.Any(i => Bytes.EqualityComparer.Equals(i.Signature, vote.Signature)))
				return VoteStatus.AlreadyExists;

			Add(vote, true);
			TryReachConsensus(r);
			
			return VoteStatus.OK;
		}
	}

	public void Check(Vote vote)
	{
		if(!Monitor.IsEntered(Lock))
			Debugger.Break();

		if(LastConfirmedRound != null && (vote.RoundId <= LastConfirmedRound.Id || vote.RoundId > NextVotingRound.Id))
			throw new IntegrityException();

		var r = GetRound(vote.RoundId);

		if(r.Forkers.Contains(vote.User))
		{	
			vote.Status = VoteStatus.Violator;
			return;
		}
	
		var e = r.VotesOfTry.FirstOrDefault(i => i.User == vote.User);
				
		if(e != null) /// FORK
		{
			r.VotesOfTry.Remove(e);
			r.Forkers.Add(e.User);
	
			vote.Status = VoteStatus.Fork; /// Let others know about incident
			return;
		}

		//if(r.Id >= JoinToVote)
		{
			if(!r.Senders.Any(i => i.User == vote.User))
			{	
				vote.Status = VoteStatus.NotMemeber;
				return;
			}
	
			var u = Users.Latest(vote.User);
							
			if(u.Owner != vote.Signer)
			{	
				vote.Status = VoteStatus.AccessDenied;
				return;
			}
	
			vote.Restore();
	
			//if(v.Transactions.Length > r.VotersRound.PerVoteTransactionsLimit)
			//{	
			//	//Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Length > r.Parent.PerVoteTransactionsLimit : {v}");
			//	return false;
			//}
		
			//if(vote.Transactions.Sum(i => i.Operations.Length) > Mcv.Net.OperationsPerRoundMaximum / r.Voters.Count()) //r.VotersRound.PerVoteOperationsMaximum)
			if(vote.Transactions.Sum(i => i.Operations.Length) > r.PerVoteOperationsMaximum)
			{	
				vote.Status = VoteStatus.PerVoteOperationsMaximumExceeded;
				return;
			}
		
			if(vote.Transactions.Any(t => r.Senders.NearestBy(i => i.User, t.User, t.Nonce).User != vote.User))
			{	
				vote.Status = VoteStatus.InvalidTransaction;
				return;
			}

		}

		vote.Status = VoteStatus.OK;
	}

	public bool TryReachConsensus(Round round)
	{
		if(LastConfirmedRound == null)
			return false;

		if(round.Target == null)
			return false;

		if(round.TargetId != LastConfirmedRound.Id + 1)
			return false;

		if(round.VotesOfTry.Count() < round.MinimalFilling)
			return false;

		var m = round.Selected.GroupBy(i => i.TargetHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

		if(m.Count() >= round.MinimumForConsensus)
		{
			var t = round.Target;
 		
			if(t.Hash == null || !m.Key.SequenceEqual(t.Hash))
			{
				t.ReUpdate();
				t.Summarize();
				
				Log?.Report(this, $"Summarize {t} - Payloads={string.Join(" ", t.Payloads.Select(i => i.User))} - VotesOfTry={string.Join(" ", t.VotesOfTry.Select(i => i.User))} - Votes={string.Join(" ", t.Votes.Select(i => i.User))}");
				
				if(t.Hash == null || !m.Key.SequenceEqual(t.Hash))
				{
					#if DEBUG
					///var x = r.Eligible.Select(i => i.ParentHash.ToHex());
					///var a = SunGlobals.Suns.Select(i => i.Mcv.FindRound(r.ParentId)?.Hash?.ToHex());
						
						
					//CompareBase([this, All.First(i => i.Node.Name == peer.Name)], "a:\\1111111111111");
					//lock(Mcv.Lock)
					//	Mcv.Dump();
					//			
					//lock(McvTcpPeering.All.First(i => i.Node.Name == peer.Name).Mcv.Lock)
					//	All.First(i => i.Node.Name == peer.Name).Mcv.Dump();
					#endif

					throw new ConfirmationException(t, m.Key);
				}
			}

			t.Confirm();
			Save(t);

			ConsensusReached.Invoke(round);

			if(round.Next != null)
			{
				round.Next.ReUpdate();
				TryReachConsensus(round.Next);
			}

			return true;
		}
		else
		{

			var s = round.Voters;
			var a = round.Selected;
		
			var missing = s.Count() - a.Count();

			if(a.Any() && m.Count() + missing < round.MinimumForConsensus)
			{
				//var h = r.Target.Hash;

				//r.Target.Update();
				//r.Target.Summarize();
				//
				//if(!r.Target.Hash.SequenceEqual(h))
				{
					round.Try++;

					//r.ReUpdate();

					round.Target.Hash = null;
				}

				ConsensusFailed(round); /// -> set MainWakeup -> Generate -> here to revote
			}
		}

		return false;
	}

	public Round GetRound(int rid)
	{
		var r = FindRound(rid);

		if(r == null)
		{	
			r = CreateRound();
			r.Id = rid;

			InsertRound(r);
		}

		return r;
	}

	public Round FindRound(int rid)
	{
		foreach(var i in Tail)
			if(i.Id == rid)
				return i;

		var d = Rocks.Get(BitConverter.GetBytes(rid), ChainFamily);

		if(d != null)
		{
			var r = CreateRound();
			r.Id			= rid; 
			r.Confirmed		= true;

			r.Load(new BinaryReader(new MemoryStream(d)));

			InsertRound(r);
			
			return r;
		}
		else
			return null;
	}

	public void InsertRound(Round round)
	{
		var i = Tail.FindIndex(i => i.Id <= round.Id);
			
		if(i != -1)
		{	
			if(Tail[i].Id == round.Id)
				Tail[i] = round;
			else
				Tail.Insert(i, round);
		}
		else
			Tail.Add(round);
	}

	public void Examine(Transaction transaction)
	{
		if(transaction.Expiration <= LastConfirmedRound.Id)
		{	
			transaction.Error = Operation.Expired;
			return;
		}

		if(!transaction.Valid(this))
		{	
			transaction.Error = "Invalid data";
			return;
		}

		var a = Users.Latest(transaction.User);

		var oldnonce = transaction.Nonce;

		transaction.Nonce = a == null ? 0 : a.LastNonce + 1;

		var r = CreateRound();
		r.Id					= LastConfirmedRound.Id + 1;
		r.ConsensusTime			= Time.Now(Clock);
		r.ConsensusEnergyCost	= LastConfirmedRound.ConsensusEnergyCost;

		r.Execute([transaction]);

		transaction.Nonce = oldnonce;
	}

	public void Dump()
	{
		var jo = new JsonSerializerOptions(NetJsonConfiguration.CreateOptions());
		jo.WriteIndented = true;

		foreach(var t in Tables)
		{
			var f = Path.Join(Databasepath, t.GetType().Name + ".table");
			File.Delete(f);

			foreach(var i in t.Clusters.OrderBy(i => i.Id))
			{	
				foreach(var b in i.Buckets.OrderBy(i => i.Id))
				{
					File.AppendAllText(f, b.Id + " - " + b.Hash.ToHex() + " - " + b.Export().ToHex() + Environment.NewLine);
					
					foreach(var e in b.Entries.OrderBy(i => i.Key))
						File.AppendAllText(f, JsonSerializer.Serialize(e, e.GetType(), jo) + Environment.NewLine);
				}
			}
		}
	}

	public void Hashify()
	{
		GraphHash = Cryptography.Hash(GraphState);

		foreach(var t in Tables.Where(i => !i.IsIndex))
			foreach(var i in t.Clusters.OrderBy(i => i.Id))
				GraphHash = Cryptography.Hash(GraphHash, i.Hash);
	}
	
	public void Save(Round round)
	{
		using var b = new WriteBatch();

		if(round.IsLastInCommit)
		{
			foreach(var t in Tables)
			{	
				var a = round.AffectedByTable(t);
				t.Commit(b, a.Values as IEnumerable<ITableEntry>, round.FindState<TableStateBase>(t), round);
				a.Clear();
			}

			LastCommitedRound = round;
					
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
	
			LastCommitedRound.WriteGraphState(w);
	
			GraphState = s.ToArray();

			Hashify();
				
			b.Put(GraphStateKey, GraphState);
			b.Put(__GraphHashKey, GraphHash);
		}

		if(Settings.Chain != null)
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

		Rocks.Write(b);
	}
//
//	public Transaction FindTailTransaction(Func<Transaction, bool> transaction_predicate)
//	{
//		foreach(var r in Tail)
//			foreach(var t in r.Transactions)
//				if(transaction_predicate(t))
//					return t;
//
//		//if(LastDissolvedRound != null)
//		//	for(int i = LastDissolvedRound.Id; i > LastDissolvedRound.Id - Net.CommitLength; i--)
//		//	{
//		//		var t = FindRound(i).ConsensusTransactions.FirstOrDefault(transaction_predicate);
//		//
//		//		if(t != null)
//		//			return t;
//		//	}
//
//		return null;
//	}
//
//	public IEnumerable<Transaction> FindLastTailTransactions(Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
//	{
//		foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
//			foreach(var t in transaction_predicate == null ? r.Transactions : r.Transactions.Where(transaction_predicate))
//				yield return t;
//	}
//
//	//public O FindLastTailOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
//	//{
//	//	var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
//	//	return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
//	//}
//	//
//	//IEnumerable<O> FindLastTailOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
//	//{
//	//	var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
//	//	return op == null ? ops : ops.Where(op);
//	//}
}
