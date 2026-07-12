using System.Data;
using System.Diagnostics;
using System.Text.Json;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void BlockDelegate(Vote b);
public delegate void RoundDelegate(Round b);
public delegate void SubnetDelegate(Execution execution, Friend friend);

public enum McvTable : byte
{
	Meta, User, Subnet, _Last = Subnet
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
	public const int								RequiredVotersMaximum = 21; 
	public int										JoinToVote => Net.P + 1;
	public int										LastGenesisRound => JoinToVote - 1;
	public const int								EntityRentYearsMin = 1;
	public const int								EntityRentYearsMax = 10;
	public const int								TransactionQueueLimit = 1000;
	public static readonly Time						Forever = Time.FromYears(30);

	public readonly static AccountKey				God = new AccountKey([1, ..new byte[31]]);
	public readonly static string					GodName = "";

	public object									Lock = new();
	public McvSettings								Settings;
	public McvNet									Net;
	public IClock									Clock;
	public Log										Log;
	public string									Databasepath;
	public string									Datapath;

	public RocksDb									Rocks;
	public byte[]									GraphState;
	public byte[]									GraphHash;
	static readonly byte[]							GraphStateKey = [0x01];
	static readonly byte[]							__GraphHashKey = [0x02];
	static readonly byte[]							ChainStateKey = [0x03];
	static readonly byte[]							GenesisKey = [0x04];
	public MetaTable								Metas;
	public UserTable								Users;
	public FriendTable								Friends;
	public TableBase[] 								Tables;
	public int										Size => Tables.Sum(i => i.Size);
	public BlockDelegate							VoteAdded;
	public RoundDelegate							ConsensusReached;
	public RoundDelegate							ConsensusFailed;
	public RoundDelegate							Confirmed;
	public SubnetDelegate							FriendTransferFormed;

	public List<OutwardResult>						OutwardResults = new();

	List<Round>										_Tail = [];
	public List<Round>								Tail
													{
														set => _Tail = value;
														get 
														{
															if(Constructed && !Monitor.IsEntered(Lock))
																Debugger.Break();
	
															return _Tail;
														}
													}
	Dictionary<int, Round>							RawRounds = [];

	public Round									LastConfirmedRound;
	public Round									LastCommitedRound;
	public Round									LastNonEmptyRound => Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
	public Round									LastPayloadRound => Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
	public Round									NextTargetRound => GetRound(LastConfirmedRound.Id + 1);
	public Round									NextVotingRound => GetRound(LastConfirmedRound.Id + 1 + Net.P);

	public List<IccpTransfer>						FriendTransferRequests = [];
	public Dictionary<IccpTransferResult, string>	FriendTransferResults = [];

	public const string								ChainFamilyName = "Chain";
	public ColumnFamilyHandle						ChainFamily	=> Rocks.GetColumnFamily(ChainFamilyName);

	protected abstract void							CreateTables(string databasepath);
	protected abstract void							GenesisInitilize(Round vote);
	public abstract Round							CreateRound();
	public abstract Vote							CreateVote();
	public abstract Generator						CreateGenerator();
	public abstract CandidacyDeclaration			CreateCandidacyDeclaration();
	public virtual void								FillVote(Vote vote){}

	Genesis											Genesis;
	public readonly bool							Constructed = false;

	public Mcv()
	{
	}

	protected Mcv(McvNet net, McvSettings settings, string datapath, string databasepath, Genesis genesis, IClock clock)
	{
		Genesis = genesis;
		Clock = clock;

		Net = net;
		Settings = settings;
		Datapath = datapath;
		Databasepath = databasepath;
	
		CreateTables(databasepath);
	
		if(net.Tables.Count != Tables.Count(i => !i.IsIndex))
			throw new IntegrityException();

		GraphHash = Net.Cryptography.ZeroHash;
	
		var g = Rocks.Get(GenesisKey);
	
		if(g == null)
		{
			Initialize();
		}
		else
		{
			if(g.SequenceEqual((new Genesis() as IBinarySerializable).ToRaw()))
			{
				Load();
			}
			else
			{ 
				Clear();
				Initialize();
			}
		}

		Constructed = true;
	}

	public void Initialize()
	{
		if(Settings.Chain != null)
		{
			for(int i = 0; i <= LastGenesisRound; i++)
			{
				var v = CreateVote(); 

				v.RoundId	 = i;
				v.Member	 = AutoId.God;
				v.Time		 = Time.Zero;
				v.TargetHash = i < Net.P ? Net.Cryptography.ZeroHash : GetRound(i - Net.P).Summarize();

				if(i == 0)
				{
 					var t = new Transaction {Nonce = 0, Expiration = 0};
 					//t.Member = new(0, -1);
					t.User = GodName;
					t.AddOperation(Genesis);
 					v.AddTransaction(t);
 					t.Sign(Net, God);

					GetRound(i).Payloads = [v];
				}
		
				v.Sign(God);
				Add(v, false);
				v.Round.VotesOfTry = v.Round.Selected = [v];
				
				GenesisInitilize(v.Round);
			}

			var r = GetRound(0);

			r.ConsensusOperationCost = 1; ///1
			r.ConsensusFundJoiners = [Net.Father0Signer];

			r.Hashify();
			r.Confirm();
			Save(r);

			if(r.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null))))
				throw new IntegrityException("Genesis construction failed");
		}
	
		Rocks.Put(GenesisKey, (new Genesis() as IBinarySerializable).ToRaw());
	}

	public void Load()
	{
		GraphState = Rocks.Get(GraphStateKey);

		if(GraphState != null)
		{
			var r = new Reader(GraphState, Net.Constructor);
	
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
			var rd = new Reader(new MemoryStream(Rocks.Get(ChainStateKey)), Net.Constructor);
		
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
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

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
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

		var r = GetRound(vote.RoundId);

		vote.Round = r;

		r.Votes.Add(vote);
		///r.Update();

		if(process)
		{
			if(check)
				Check(vote);
	
			if(!check || vote.Status == VoteStatus.OK)
			{	
				if(vote.Try == r.Try)
	 			{	
					foreach(var t in vote.Transactions)
					{
						t.Round = r;
						t.Status = TransactionStatus.Placed;
					}

					r.VotesOfTry.Add(vote);
					
					if(vote.Transactions.Any())
						r.Payloads.Add(vote);
	
					if(r.Id >= JoinToVote && r.Voters.Any(j => j.User == vote.Member))
						r.Selected.Add(vote);
				}
			}
		}

		VoteAdded?.Invoke(vote);
	}

	public VoteStatus ProcessIncoming(Vote vote, Synchronization synchroniztion)
	{
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

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

			if(r.Votes.Any(i => Bytes.Equal(i.Signature, vote.Signature)))
				return VoteStatus.AlreadyExists;
	
			Add(vote, false);
				
			return VoteStatus.OK;
		}
		else
		{
			var r = GetRound(vote.RoundId);

			if(r.Votes.Any(i => Bytes.Equal(i.Signature, vote.Signature)))
				return VoteStatus.AlreadyExists;

			Add(vote, true);
			TryConfirm(r);
			
			return VoteStatus.OK;
		}
	}

	public void Check(Vote vote)
	{
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

		if(LastConfirmedRound != null && (vote.RoundId <= LastConfirmedRound.Id || vote.RoundId > NextVotingRound.Id))
			throw new IntegrityException();

		var r = GetRound(vote.RoundId);

		if(r.Forkers.Contains(vote.Member))
		{	
			vote.Status = VoteStatus.Violator;
			return;
		}
	
		var e = r.VotesOfTry.FirstOrDefault(i => i.Member == vote.Member);
				
		if(e != null) /// FORK
		{
			r.VotesOfTry.Remove(e);
			r.Forkers.Add(e.Member);
	
			vote.Status = VoteStatus.Fork; /// Let others know about incident
			return;
		}

		//if(r.Id >= JoinToVote)
		{
			if(!r.Senders.Any(i => i.User == vote.Member))
			{	
				vote.Status = VoteStatus.NotMemeber;
				return;
			}
	
			var u = Users.Latest(vote.Member);
							
			if(!Net.Cryptography.Verify(u.Owner, vote.Hash, vote.Signature))
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
		
			if(vote.Transactions.Any(t => r.Senders.NearestBy(t.Signature).User != vote.Member))
			{	
				vote.Status = VoteStatus.InvalidTransaction;
				return;
			}

		}

		vote.Status = VoteStatus.OK;
	}

	public bool TryConfirm(Round round)
	{
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

		if(LastConfirmedRound == null)
			return false;

		if(round.Target == null)
			return false;

		if(round.TargetId != LastConfirmedRound.Id + 1)
			return false;

		if(round.VotesOfTry.Count() < round.MinimumForConsensus)
			return false;

		var m = round.Selected.GroupBy(i => i.TargetHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

		if(m.Count() >= round.MinimumForConsensus)
		{
			var t = round.Target;
 		
			if(t.Hash == null || !m.Key.SequenceEqual(t.Hash))
			{
				t.ReUpdate();
				t.Summarize();

				if(t.Hash == null)
					return false;
				
				if(!m.Key.SequenceEqual(t.Hash))
				{
					if(NodeGlobals.DumpOnError)
					{
						Dump();

						foreach(var i in McvPeering.All.Where(i => i.Mcv != null && i.Mcv != this))	Monitor.Enter(i.Mcv.Lock);
						foreach(var i in McvPeering.All.Where(i => i.Mcv != null && i.Mcv != this))	i.Mcv.Dump();
						foreach(var i in McvPeering.All.Where(i => i.Mcv != null && i.Mcv != this))	Monitor.Exit(i.Mcv.Lock);
					}

					throw new ConfirmationException(t, m.Key);
				}
			}

			ConsensusReached.Invoke(round); /// our vote may confirm round

			if(!t.Confirmed)
			{
				t.Confirm();
				Save(t);

				if(round.Next != null)
				{
					round.Next.ReUpdate();
					TryConfirm(round.Next);
				}
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
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

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
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

		foreach(var i in Tail)
			if(i.Id == rid)
				return i;

		var d = Rocks.Get(BitConverter.GetBytes(rid), ChainFamily);

		if(d != null)
		{
			var r = CreateRound();
			r.Id			= rid; 
			r.Confirmed		= true;
			r.Raw			= d;

			RawRounds[rid] = r;
			
			return r;
		}
		else
			return null;
	}

	public void InsertRound(Round round)
	{
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

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
		r.Id						= LastConfirmedRound.Id + 1;
		r.ConsensusTime				= Time.Now(Clock);
		r.ConsensusOperationCost	= LastConfirmedRound.ConsensusOperationCost;

		r.Execute([transaction], true);

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
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

		GraphHash = Cryptography.Hash(GraphState);

		foreach(var t in Tables.Where(i => !i.IsIndex))
			foreach(var i in t.Clusters.OrderBy(i => i.Id))
				GraphHash = Cryptography.Hash(GraphHash, i.Hash);
	}
	
	public void Save(Round round)
	{
		if(Constructed && !Monitor.IsEntered(Lock))
			Debugger.Break();

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
			var w = new Writer(s, Net.Constructor);
	
			LastCommitedRound.WriteGraphState(w);
	
			GraphState = s.ToArray();

			Hashify();
				
			b.Put(GraphStateKey, GraphState);
			b.Put(__GraphHashKey, GraphHash);
		}

		if(Settings.Chain != null)
		{
			var s = new MemoryStream();
			var w = new Writer(s, Net.Constructor);

			w.Write7BitEncodedInt(round.Id);

			b.Put(ChainStateKey, s.ToArray());
	
			if(round.Raw == null)
				round.Archive();
	
			b.Put(BitConverter.GetBytes(round.Id), round.Raw, ChainFamily);
		}

		Rocks.Write(b);
	}
}
