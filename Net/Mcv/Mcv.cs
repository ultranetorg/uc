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

public abstract class Mcv /// Mutual chain voting
{
	public const int							P = 6; /// pitch
	public const int							RequiredVotersMaximum = 21; 
	//public int								VotesRequired => Net.MembersLimit; /// 1000/8
	public const int							JoinToVote = P + 1;
	public const int							LastGenesisRound = JoinToVote - 1;
	public const int							TransactionPlacingLifetime = P*2;
	public static readonly Unit					BalanceMin = new Unit(0.000_000_001);
	//public const int							EntityLength = 100;
	public const int							EntityRentYearsMin = 1;
	public const int							EntityRentYearsMax = 10;
	public const int							TransactionQueueLimit = 1000;
	public static readonly Time					Forever = Time.FromYears(30);
	//public static Money							TimeFactor(Time time) => new Money(time.Days * time.Days)/Time.FromYears(1).Days;
	//public static long							ApplyTimeFactor(Time time, long x) => x * time.Days/Time.FromYears(1).Days;

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
	public Dictionary<int, Round>				OldRounds = new();
	public Round								LastConfirmedRound;
	public Round								LastCommitedRound;
	public Round								LastNonEmptyRound => Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
	public Round								LastPayloadRound => Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
	public Round								NextVotingRound => GetRound(LastConfirmedRound.Id + 1 + P);
	//public List<Generator>						NextVoteMembers => FindRound(NextVoteRound.VotersId).Members;


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
			///Settings = new RdnSettings {Roles = Role.Chain};
			Net = net;
			Settings = settings;
			Datapath = datapath;
			Databasepath = databasepath;
	
			CreateTables(databasepath);
	
			if(net.TablesCount != Tables.Count(i => !i.IsIndex))
				throw new IntegrityException();

			GraphHash = Net.Cryptography.ZeroHash;
	
			//if(!skipinitload)
			{
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
				v.ParentHash = i < P ? Net.Cryptography.ZeroHash : GetRound(i - P).Summarize();

				if(i == 0)
				{
 					var t = new Transaction {Net = Net, Nonce = 0, Expiration = 0};
 					t.Member = new(0, -1);
					t.User = GodName;
					t.AddOperation(Genesis);
 					v.AddTransaction(t);
 					t.Sign(God);
				}
		
				v.Sign(God);
				Add(v);
				
				GenesisInitilize(v.Round);
			}

			var r = GetRound(0);

			r.ConsensusECEnergyCost = 1; ///1
			r.ConsensusFundJoiners = [Net.Father0Signer];
			r.ConsensusTransactions = r.OrderedTransactions.ToArray();
			r.ConsensusViolators = [];
			r.ConsensusMemberLeavers = [];
			r.Funds = [];

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

			OldRounds.Add(LastCommitedRound.Id, LastCommitedRound);

			Hashify();

			if(!GraphHash.SequenceEqual(Rocks.Get(__GraphHashKey)))
			{
				throw new IntegrityException();
			}
		}

		if(Settings.Chain != null)
		{
			var s = Rocks.Get(ChainStateKey);

			var rd = new BinaryReader(new MemoryStream(s));

			var lcr = FindRound(rd.Read7BitEncodedInt());
				
			if(lcr.Id < Net.CommitLength) /// clear to avoid genesis loading issues, it must be created - not loaded
			{
				Clear();
				Initialize();
			} 
			else
			{
				for(var i = lcr.Id - lcr.Id % Net.CommitLength; i <= lcr.Id; i++)
				{
					var r = FindRound(i);

					Tail.Insert(0, r);
		
					r.Confirmed = false;
					//Execute(r, r.ConfirmedTransactions);
					r.Confirm();
				}
			}
		}

// 		foreach(var i in Tables)
// 		{
// 			i.Load();
// 		}
	}

	public void Clear()
	{
		Tail.Clear();

		GraphState = null;
		GraphHash = Net.Cryptography.ZeroHash;

		LastCommitedRound = null;
		LastConfirmedRound = null;

		OldRounds.Clear();
		//Accounts.Clear();

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

	public bool Add(Vote vote)
	{
		var r = GetRound(vote.RoundId);

		vote.Round = r;

		r.Votes.Add(vote);
		r.Update();
	
		if(vote.Transactions.Any())
		{
			foreach(var t in vote.Transactions)
			{
				t.Round = r;
				t.Status = TransactionStatus.Placed;
			}
		}

		if(r.FirstArrivalTime == DateTime.MaxValue)
		{
			r.FirstArrivalTime = DateTime.UtcNow;
		} 

		VoteAdded?.Invoke(vote);

		var p = r.Parent;

		if(vote.RoundId > LastGenesisRound && p.Previous.Confirmed && !p.Confirmed)
		{
			if(r.ConsensusReached)
			{

				var mh = r.MajorityOfRequiredByParentHash.Key;
 		
				if(p.Hash == null || !mh.SequenceEqual(p.Hash))
				{
					p.Summarize();
					
					if(p.Hash == null || !mh.SequenceEqual(p.Hash))
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


						throw new ConfirmationException(p, mh);
					}
				}

				p.Confirm();
				Save(p);

				return true;

			}
			else if(r.ConsensusFailed)
			{
				ConsensusFailed(r);
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
			Tail.Add(r);
			Tail = Tail.OrderByDescending(i => i.Id).ToList();
		}

		return r;
	}

	public Round FindRound(int rid)
	{
		foreach(var i in Tail)
			if(i.Id == rid)
				return i;

		if(OldRounds.TryGetValue(rid, out var r))
			return r;

		var d = Rocks.Get(BitConverter.GetBytes(rid), ChainFamily);

		if(d != null)
		{
			r = CreateRound();
			r.Id			= rid; 
			r.Confirmed		= true;
			//r.LastAccessed	= DateTime.UtcNow;

			r.Load(new BinaryReader(new MemoryStream(d)));

			OldRounds[r.Id] = r;
			//Recycle();
			
			return r;
		}
		else
			return null;
	}

	void Recycle()
	{
		if(OldRounds.Count > Net.CommitLength)
		{
			foreach(var i in OldRounds.OrderByDescending(i => i.Value.Id).Skip(Net.CommitLength))
			{
				OldRounds.Remove(i.Key);
			}
		}
	}

	public Round Examine(Transaction transaction, bool preserve)
	{
		if(transaction.Expiration <= LastConfirmedRound.Id || !transaction.Valid(this))
			return null;

		var a = Users.Latest(transaction.User);

		var nid = transaction.Nonce;

		if(a == null)
		{	
			if(transaction.UserCreationRequest != null)
				transaction.Nonce = 0;
			else
				return null;
		}
		else
			transaction.Nonce = a.LastTransactionNid + 1;

		var round = TryExecute(transaction);

		if(preserve)
			transaction.Nonce = nid;

		return round;

	}


	///public Time CalculateTime(Round round, IEnumerable<Vote> votes)
	///{
 		///	if(round.Id == 0)
 		///	{
 		///		return round.ConfirmedTime;
 		///	}
	///	
 		///	if(!votes.Any())
 		///	{
	///		return round.Previous.ConfirmedTime + new Time(1);
	///	}
	///	
	///	if(votes.Count() < 3)
	///	{
	///		var a = votes.Sum(i => i.TimeDelta)/votes.Count();
	///		return round.Previous.ConfirmedTime + new Time(a);
	///	}
	///	else
	///	{
	///		var n = votes.Count();
	///		votes = votes.OrderBy(i => i.TimeDelta).Skip(n/3).Take(n/3);
	///		var a = votes.Sum(i => i.TimeDelta)/votes.Count();
	///	
	///		return round.Previous.ConfirmedTime + new Time(a);
	///	}
	///}

	public void Dump()
	{
		var jo = new JsonSerializerOptions(ApiClient.CreateOptions());
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


	public Round TryExecute(Transaction transaction)
	{
		//var m = NextVotingRound.VotersRound.Members.NearestBy(transaction.Nonce).Address;
		//
		//if(!Settings.Generators.Any(i => i.Signer == m))
		//	return null;

		var p = Tail.FirstOrDefault(r => !r.Confirmed && r.Votes.Any(v => Settings.Generators.Any(g => g.Signer == v.Generator))) ?? LastConfirmedRound;

		var r = GetRound(p.Id + 1);
		
		r.ConsensusTime			= Time.Now(Clock);
		r.ConsensusECEnergyCost	= LastConfirmedRound.ConsensusECEnergyCost;
		//r.Spacetimes			= LastConfirmedRound.Spacetimes;

		r.Execute([transaction], true);

		return r;
	}
	
	public void Save(Round round)
	{
		using(var b = new WriteBatch())
		{
			if(round.IsLastInCommit)
			{
				foreach(var t in Tables)
					t.Commit(b, Tail.TakeLast(Net.CommitLength).SelectMany(r => r.AffectedByTable(t).Values as IEnumerable<ITableEntry>).DistinctBy(i => i.Key), round.FindState<TableStateBase>(t), round);

				LastCommitedRound = round;
					
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				LastCommitedRound.WriteGraphState(w);
	
				GraphState = s.ToArray();

				Hashify();
				
				b.Put(GraphStateKey, GraphState);
				b.Put(__GraphHashKey, GraphHash);

				OldRounds.Clear();

				foreach(var i in Tail.SkipWhile(i => i.Id > round.Id).Take(JoinToVote + 1))
				{
					OldRounds[i.Id] = i;
				}

				Tail.RemoveAll(i => i.Id <= round.Id);

				Recycle();
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
	}

	public Transaction FindTailTransaction(Func<Transaction, bool> transaction_predicate)
	{
		foreach(var r in Tail)
			foreach(var t in r.Transactions)
				if(transaction_predicate(t))
					return t;

		//if(LastDissolvedRound != null)
		//	for(int i = LastDissolvedRound.Id; i > LastDissolvedRound.Id - Net.CommitLength; i--)
		//	{
		//		var t = FindRound(i).ConsensusTransactions.FirstOrDefault(transaction_predicate);
		//
		//		if(t != null)
		//			return t;
		//	}

		return null;
	}

	public IEnumerable<Transaction> FindLastTailTransactions(Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
	{
		foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
			foreach(var t in transaction_predicate == null ? r.Transactions : r.Transactions.Where(transaction_predicate))
				yield return t;
	}

	//public O FindLastTailOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
	//{
	//	var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
	//	return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
	//}
	//
	//IEnumerable<O> FindLastTailOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Round, bool> rp = null)
	//{
	//	var ops = FindLastTailTransactions(tp, rp).SelectMany(i => i.Operations.OfType<O>());
	//	return op == null ? ops : ops.Where(op);
	//}
}
