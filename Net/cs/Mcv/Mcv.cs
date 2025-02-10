using System.Data;
using System.Diagnostics;
using System.Text.Json;
using RocksDbSharp;

namespace Uccs.Net;

public delegate void BlockDelegate(Vote b);
public delegate void ConsensusDelegate(Round b, bool reached);
public delegate void RoundDelegate(Round b);

public abstract class Mcv /// Mutual chain voting
{
	public const int							P = 6; /// pitch
	public const int							RequiredVotersMaximum = 21; 
	//public int									VotesRequired => Net.MembersLimit; /// 1000/8
	public const int							JoinToVote = P*2;
	public const int							TransactionPlacingLifetime = P*2;
	public const int							LastGenesisRound = P*3 - 1;
	public static readonly Unit					BalanceMin = new Unit(0.000_000_001);
	public const int							EntityLength = 100;
	public const int							EntityRentYearsMin = 1;
	public const int							EntityRentYearsMax = 10;
	public const int							OperationsQueueLimit = 1000;
	public static readonly Time					Forever = Time.FromYears(30);
	//public static Money							TimeFactor(Time time) => new Money(time.Days * time.Days)/Time.FromYears(1).Days;
	//public static long							ApplyTimeFactor(Time time, long x) => x * time.Days/Time.FromYears(1).Days;

	public object								Lock = new();
	public McvSettings							Settings;
	public McvNet								Net;
	public IClock								Clock;
	public Log									Log;
	public string								Databasepath;

	public RocksDb								Database;
	public byte[]								BaseState;
	public byte[]								BaseHash;
	static readonly byte[]						BaseStateKey = [0x01];
	static readonly byte[]						__BaseHashKey = [0x02];
	static readonly byte[]						ChainStateKey = [0x03];
	static readonly byte[]						GenesisKey = [0x04];
	public AccountTable							Accounts;
	public TableBase[] 							Tables;
	public int									Size => Tables.Sum(i => i.Size);
	public BlockDelegate						VoteAdded;
	public ConsensusDelegate					ConsensusConcluded;
	public RoundDelegate						Commited;

	List<Round>									_Tail = [];
	public List<Round>							Tail
												{
													set => _Tail = value;
													get 
													{
														///if(!_init && !Monitor.IsEntered(Lock))
														///	Debugger.Break();

														return _Tail;
													}
												}
	public Dictionary<int, Round>				LoadedRounds = new();
	public Round								LastConfirmedRound;
	public Round								LastCommittedRound;
	public Round								LastNonEmptyRound => Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
	public Round								LastPayloadRound => Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
	public Round								NextVoteRound => GetRound(LastConfirmedRound.Id + 1 + P);
	//public List<Generator>						NextVoteMembers => FindRound(NextVoteRound.VotersId).Members;


	public List<NtnBlock>						NtnBlocks = [];

	public const string							ChainFamilyName = "Chain";
	public ColumnFamilyHandle					ChainFamily	=> Database.GetColumnFamily(ChainFamilyName);

	public static int							GetValidityPeriod(int rid) => rid + P;

	protected abstract void						CreateTables(string databasepath);
	protected abstract void						GenesisInitilize(Round vote);
	public abstract Round						CreateRound();
	public abstract Vote						CreateVote();
	public abstract Generator					CreateGenerator();
	public abstract CandidacyDeclaration		CreateCandidacyDeclaration();
	public abstract void						FillVote(Vote vote);

	public Mcv()
	{
	}

	protected Mcv(McvNet net, McvSettings settings, string databasepath, bool skipinitload = false)
	{
		///Settings = new RdnSettings {Roles = Role.Chain};
		Net = net;
		Settings = settings;
		Databasepath = databasepath;

		CreateTables(databasepath);

		BaseHash = Net.Cryptography.ZeroHash;

		if(!skipinitload)
		{
			var g = Database.Get(GenesisKey);

			if(g == null)
			{
				Initialize();
			}
			else
			{
				if(g.SequenceEqual(Net.Genesis.FromHex()))
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

	public Mcv(McvNet net, McvSettings settings, string databasepath, IClock clock) : this(net, settings, databasepath)
	{
		Clock = clock;
	}

	public virtual string CreateGenesis(AccountKey god, AccountKey f0, CandidacyDeclaration candidacydeclaration)
	{
		/// 0	- declare F0
		/// P	- confirmed F0 membership
		/// P+P	- F0 start voting for P+P-P-1 = P-1

		Clear();

		var s = new MemoryStream();
		var w = new BinaryWriter(s);

		void write(int rid)
		{
			var r = GetRound(rid);
			r.ConsensusTransactions = r.OrderedTransactions.ToArray();
			r.Hashify();
			r.Write(w);
		}

		var v0 = CreateVote(); 
		{
			v0.RoundId = 0;
			v0.Time = Time.Zero;
			v0.ParentHash = Net.Cryptography.ZeroHash;

			var t = new Transaction {Net = Net, Nid = 0, Expiration = 0};
			t.Member = new(0, -1);
			t.AddOperation(new UtilityTransfer(f0, Net.ECEmission, 0, Net.BDDayEmission));
			t.Sign(god, Net.Cryptography.ZeroHash);
			v0.AddTransaction(t);

			t = new Transaction {Net = Net, Nid = 0, Expiration = 0};
			t.Member = new(0, -1);
			t.AddOperation(candidacydeclaration);
			t.Sign(f0, Net.Cryptography.ZeroHash);
			v0.AddTransaction(t);
		
			v0.Sign(god);
			Add(v0);
			///v0.FundJoiners = v0.FundJoiners.Append(Net.Father0).ToArray();
			write(0);
		}

		for(int i = 1; i <= LastGenesisRound; i++)
		{
			var v = CreateVote();
			v.RoundId	 = i;
			v.Time		 = Time.Zero;  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
			v.ParentHash = i < P ? Net.Cryptography.ZeroHash : GetRound(i - P).Summarize();
	
			v.Sign(i < JoinToVote ? god : f0);
			Add(v);

			write(i);
		}
					
		return s.ToArray().ToHex();
	}

	public void Initialize()
	{
		if(Settings.Chain != null)
		{
			Tail.Clear();

 			var rd = new BinaryReader(new MemoryStream(Net.Genesis.FromHex()));
					
			for(int i = 0; i <= LastGenesisRound; i++)
			{
				var r = CreateRound();
				r.Read(rd);
	
				Tail.Insert(0, r);

				if(i < P*2)
				{
					if(i > 0)
						r.ConsensusECFee = 1;

					if(i == 0)
						r.ConsensusFundJoiners = [Net.Father0];
					
					r.ConsensusTransactions = r.OrderedTransactions.ToArray();

					GenesisInitilize(r);

					r.Hashify();
					r.Confirm();
					Commit(r);
				}
			}

			if(Tail.Any(i => i.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null)))))
				throw new IntegrityException("Genesis construction failed");
		}
	
		Database.Put(GenesisKey, Net.Genesis.FromHex());
	}
	
	public void Load()
	{
		BaseState = Database.Get(BaseStateKey);

		if(BaseState != null)
		{
			var r = new BinaryReader(new MemoryStream(BaseState));
	
			LastCommittedRound = CreateRound();
			LastCommittedRound.ReadBaseState(r);

			LoadedRounds.Add(LastCommittedRound.Id, LastCommittedRound);

			Hashify();

			if(!BaseHash.SequenceEqual(Database.Get(__BaseHashKey)))
			{
				throw new IntegrityException();
			}
		}

		if(Settings.Chain != null)
		{
			var s = Database.Get(ChainStateKey);

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
	}

	public void Clear()
	{
		Tail.Clear();

		BaseState = null;
		BaseHash = Net.Cryptography.ZeroHash;

		LastCommittedRound = null;
		LastConfirmedRound = null;

		LoadedRounds.Clear();
		//Accounts.Clear();

		foreach(var i in Tables)
			i.Clear();

		Database.Remove(BaseStateKey);
		Database.Remove(__BaseHashKey);
		Database.Remove(ChainStateKey);
		Database.Remove(GenesisKey);

		Database.DropColumnFamily(ChainFamilyName);
		Database.CreateColumnFamily(new (), ChainFamilyName);
	}

	public void Stop()
	{
		Database.Dispose();
	}

	public bool Add(Vote vote)
	{
		var r = GetRound(vote.RoundId);

		vote.Round = r;

		r.Votes.Add(vote);
	
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
						#endif

						throw new ConfirmationException(p, mh);
					}
				}

				p.Confirm();
				Commit(p);

				ConsensusConcluded(r, true);
				return true;

			}
			else if(r.ConsensusFailed)
			{
				r.Parent.Hash = null;
				r.FirstArrivalTime = DateTime.MaxValue;
				r.Try++;

				ConsensusConcluded(r, false);
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

		if(LoadedRounds.TryGetValue(rid, out var r))
			return r;

		var d = Database.Get(BitConverter.GetBytes(rid), ChainFamily);

		if(d != null)
		{
			r = CreateRound();
			r.Id			= rid; 
			r.Confirmed		= true;
			//r.LastAccessed	= DateTime.UtcNow;

			r.Load(new BinaryReader(new MemoryStream(d)));

			LoadedRounds[r.Id] = r;
			//Recycle();
			
			return r;
		}
		else
			return null;
	}

	void Recycle()
	{
		if(LoadedRounds.Count > Net.CommitLength)
		{
			foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.Id).Skip(Net.CommitLength))
			{
				LoadedRounds.Remove(i.Key);
			}
		}
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
			var f = Path.Join(Databasepath, t.GetType().Name);
			File.Delete(f);

			foreach(var i in t.Clusters.OrderBy(i => i.Id))
			{	
				foreach(var b in i.Buckets.OrderBy(i => i.Id))
				{
					File.AppendAllText(f, b.Id + " - " + b.Hash.ToHex() + " - " + b.Main.ToHex() + Environment.NewLine);
					
					foreach(var e in b.Entries.OrderBy(i => i.BaseId))
						File.AppendAllText(f, JsonSerializer.Serialize(e, e.GetType(), jo) + Environment.NewLine);
				}
			}
		}
	}

	public void Hashify()
	{
		BaseHash = Cryptography.Hash(BaseState);

		foreach(var t in Tables)
			foreach(var i in t.Clusters.OrderBy(i => i.Id))
				BaseHash = Cryptography.Hash(BaseHash, i.Hash);
	}


	public Round TryExecute(Transaction transaction)
	{
		var m = NextVoteRound.VotersRound.Members.NearestBy(m => m.Address, transaction.Signer).Address;

		if(!Settings.Generators.Contains(m))
			return null;

		var p = Tail.FirstOrDefault(r => !r.Confirmed && r.Votes.Any(v => v.Generator == m)) ?? LastConfirmedRound;

		var r = GetRound(p.Id + 1);
		
		r.ConsensusTime			= Time.Now(Clock);
		r.ConsensusECFee	= LastConfirmedRound.ConsensusECFee;
		///r.RentPerBytePerDay		= p.RentPerBytePerDay;
		//r.Candidates			= p.Candidates;
		//r.Members				= p.Members;
		//r.Funds				= p.Funds;

		r.Execute([transaction], true);

		return r;
	}
	
	public void Commit(Round round)
	{
		using(var b = new WriteBatch())
		{
			if(round.IsLastInCommit)
			{
				//if(LastCommittedRound != null && LastCommittedRound != round.Previous)
				//	throw new IntegrityException("Id % 100 == 0 && LastConfirmedRound != Previous");

				var tail = Tail.AsEnumerable().Reverse().Take(Net.CommitLength);

				foreach(var r in tail)
					foreach(var t in Tables)
						t.Save(b, r.AffectedByTable(t).Values, round);

				LastCommittedRound = tail.Last();
					
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				LastCommittedRound.WriteBaseState(w);
	
				BaseState = s.ToArray();

				Hashify();
				
				b.Put(BaseStateKey, BaseState);
				b.Put(__BaseHashKey, BaseHash);

				foreach(var i in tail)
				{
					if(!LoadedRounds.ContainsKey(i.Id))
					{
						LoadedRounds.Add(i.Id, i);
					}
						
					Tail.Remove(i);
				}

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

			Database.Write(b);
		}

		Commited?.Invoke(round);
	}

	public Transaction FindRecentTransaction(Func<Transaction, bool> transaction_predicate)
	{
		foreach(var r in Tail)
			foreach(var t in r.Transactions)
				if(transaction_predicate(t))
					return t;

		if(LastCommittedRound != null)
			for(int i = LastCommittedRound.Id; i > LastCommittedRound.Id - Net.CommitLength; i--)
			{
				var t = FindRound(i).ConsensusTransactions.FirstOrDefault(transaction_predicate);

				if(t != null)
					return t;
			}

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
