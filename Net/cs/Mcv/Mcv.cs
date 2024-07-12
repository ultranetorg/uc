using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void BlockDelegate(Vote b);
	public delegate void ConsensusDelegate(Round b, bool reached);
	public delegate void RoundDelegate(Round b);

	public abstract class Mcv /// Mutual chain voting
	{
		public const int							P = 8; /// pitch
		public const int							DeclareToGenerateDelay = P*2;
		public const int							TransactionPlacingLifetime = P*2;
		public const int							LastGenesisRound = 1+P + 1+P + P;
		public static readonly Money				BalanceMin = new Money(0.000_000_001);
		public const int							EntityLength = 100;
		public const int							EntityRentYearsMin = 1;
		public const int							EntityRentYearsMax = 10;
		public const int							OperationsQueueLimit = 1000;
		public static readonly Time					Forever = Time.FromYears(30);
		//public static Money							TimeFactor(Time time) => new Money(time.Days * time.Days)/Time.FromYears(1).Days;
		public static Money							TimeFactor(Time time) => new Money(time.Days)/Time.FromYears(1).Days;

		public McvSettings							Settings;
		public McvZone								Zone;
		public McvNode								Node;
		public IClock								Clock;
		public object								Lock => Node.Lock;
		public Log									Log;
		public Flow									Flow;

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

		public List<Round>							Tail = new();
		public Dictionary<int, Round>				LoadedRounds = new();
		public Round								LastConfirmedRound;
		public Round								LastCommittedRound;
		public Round								LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
		public Round								LastPayloadRound	=> Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
		public Round								NextVoteRound => GetRound(LastConfirmedRound.Id + 1 + P);
		public List<Member>							NextVoteMembers => VotersOf(NextVoteRound);


		public const string							ChainFamilyName = "Chain";
		public ColumnFamilyHandle					ChainFamily	=> Database.GetColumnFamily(ChainFamilyName);

		public bool									IsCommitReady(Round round) => (round.Id + 1) % Zone.CommitLength == 0; ///Tail.Count(i => i.Id <= round.Id) >= Zone.CommitLength; 
		public static int							GetValidityPeriod(int rid) => rid + P;

		public abstract string						CreateGenesis(AccountKey god, AccountKey f0);
		protected abstract void						CreateTables(string databasepath);
		protected abstract void						GenesisCreate(Vote vote);
		protected abstract void						GenesisInitilize(Round vote);
		public abstract Operation					CreateOperation(int type);
		public abstract Round						CreateRound();
		public abstract Vote						CreateVote();
		public abstract void						FillVote(Vote vote);

		static Mcv()
		{
			if(!ITypeCode.Contructors.ContainsKey(typeof(Operation)))
				ITypeCode.Contructors[typeof(Operation)] = [];

			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Operation)) && !i.IsAbstract))
			{
				ITypeCode.Codes[i] = (byte)Enum.Parse<OperationClass>(i.Name);
				ITypeCode.Contructors[typeof(Operation)][(byte)Enum.Parse<OperationClass>(i.Name)]  = i.GetConstructor([]);
			}
  		}

		public Mcv()
		{
  		}

		protected Mcv(McvZone zone, McvSettings settings, string databasepath, bool skipinitload = false)
		{
			///Settings = new RdnSettings {Roles = Role.Chain};
			Zone = zone;
			Settings = settings;

			CreateTables(databasepath);

			BaseHash = Zone.Cryptography.ZeroHash;

			if(!skipinitload)
			{
				var g = Database.Get(GenesisKey);
	
				if(g == null)
				{
					Initialize();
				}
				else
				{
					if(g.SequenceEqual(Zone.Genesis.FromHex()))
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

		public Mcv(McvNode node, McvSettings settings, string databasepath, IClock clock, Flow flow) : this(node.Zone as McvZone, settings, databasepath)
		{
			Node = node;
			Flow = flow;
			Clock = clock;
		}

		public void Initialize()
		{
			if(Settings.Base?.Chain != null)
			{
				Tail.Clear();
	
 				var rd = new BinaryReader(new MemoryStream(Zone.Genesis.FromHex()));
						
				for(int i = 0; i <=1+P + 1+P + P; i++)
				{
					var r = CreateRound();
					r.Read(rd);
		
					Tail.Insert(0, r);
	
					if(r.Id > 0)
					{
						r.ConsensusTime = r.Confirmed ? r.ConsensusTime : r.Votes.First().Time;
						r.ConsensusExeunitFee = 1;
					}
	
					if(i <= 1+P + 1+P)
					{
						if(i == 0)
							r.ConsensusFundJoiners = [Zone.Father0];

						GenesisInitilize(r);

						r.ConsensusTransactions = r.OrderedTransactions.ToArray();

						r.Hashify();
						r.Confirm();
						Commit(r);
					}
				}
	
				if(Tail.Any(i => i.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null)))))
					throw new IntegrityException("Genesis construction failed");
			}
		
			Database.Put(GenesisKey, Zone.Genesis.FromHex());
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
					throw new IntegrityException("");
				}
			}

			if(Settings.Base?.Chain != null)
			{
				var s = Database.Get(ChainStateKey);

				var rd = new BinaryReader(new MemoryStream(s));

				var lcr = FindRound(rd.Read7BitEncodedInt());
					
				for(int i = lcr.Id - lcr.Id % Zone.CommitLength; i <= lcr.Id; i++)
				{
					var r = FindRound(i);

					Tail.Insert(0, r);
		
					r.Confirmed = false;
					//Execute(r, r.ConfirmedTransactions);
					r.Confirm();
				}
			}
		}

		public virtual void ClearTables()
		{
		}

		public void Clear()
		{
			Tail.Clear();

			BaseState = null;
			BaseHash = Zone.Cryptography.ZeroHash;

			LastCommittedRound = null;
			LastConfirmedRound = null;

			LoadedRounds.Clear();
			Accounts.Clear();

			ClearTables();

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
					ConsensusConcluded(r, true);

					var mh = r.Majority.Key;
	 		
					if(p.Hash == null || !mh.SequenceEqual(p.Hash))
					{
						p.Summarize();
						
						if(!mh.SequenceEqual((p.Hash)))
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

					return true;
	
				}
				else if(ConsensusFailed(r))
				{
					r.FirstArrivalTime = DateTime.MaxValue;
					r.Try++;

					r.Parent.Hash = null;

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
				//r.LastAccessed = DateTime.UtcNow;
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
			if(LoadedRounds.Count > Zone.CommitLength)
			{
				foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.Id).Skip(Zone.CommitLength))
				{
					LoadedRounds.Remove(i.Key);
				}
			}
		}

		public List<Member> VotersOf(Round round)
		{
			return FindRound(round.VotersRound).Members/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool ConsensusFailed(Round r)
		{
			var m = VotersOf(r);

			var e = r.Eligible;
			
			var d = m.Count - e.Count();

			var q = r.RequiredVotes;

			return e.Any() && e.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).All(i => i.Count() + d < q);
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

		
		public IEnumerable<AccountAddress> ProposeViolators(Round round)
		{
			var g = round.Id > P ? VotersOf(round) : new();
			var gv = round.VotesOfTry.Where(i => g.Any(j => i.Generator == j.Account)).ToArray();
			return gv.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key).ToArray();
		}

		public IEnumerable<AccountAddress> ProposeMemberLeavers(Round round, AccountAddress generator)
		{
			var prevs = Enumerable.Range(round.ParentId - P, P).Select(i => FindRound(i));

			var ls = VotersOf(round).Where(i =>	i.CastingSince <= round.ParentId &&/// in previous Pitch number of rounds
												!round.Parent.VotesOfTry.Any(v => v.Generator == i.Account) &&	/// ??? sent less than MinVotesPerPitch of required blocks
												!prevs.Any(r => r.VotesOfTry.Any(v => v.Generator == generator && v.MemberLeavers.Contains(i.Account)))) /// not yet proposed in prev [Pitch-1] rounds
									.Select(i => i.Account);
			return ls;
		}

		public void Hashify()
		{
			BaseHash = Cryptography.Hash(BaseState);
	
			foreach(var t in Tables)
				foreach(var i in t.SuperClusters.OrderBy(i => i.Key))
					BaseHash = Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}


		public Round TryExecute(Transaction transaction)
		{
			var m = NextVoteMembers.NearestBy(m => m.Account, transaction.Signer).Account;

			if(!Settings.Generators.Contains(m))
				return null;

			var p = Tail.FirstOrDefault(r => !r.Confirmed && r.Votes.Any(v => v.Generator == m)) ?? LastConfirmedRound;

			var r = GetRound(p.Id + 1);
			
			r.ConsensusTime			= Time.Now(Clock);
			r.ConsensusExeunitFee	= p.ConsensusExeunitFee;
			///r.RentPerBytePerDay		= p.RentPerBytePerDay;
			r.Members				= p.Members;
			r.Funds					= p.Funds;
	
			r.Execute([transaction]);

			return r;
		}
		
		public void Commit(Round round)
		{
			using(var b = new WriteBatch())
			{
				if(IsCommitReady(round))
				{
					//if(LastCommittedRound != null && LastCommittedRound != round.Previous)
					//	throw new IntegrityException("Id % 100 == 0 && LastConfirmedRound != Previous");

					var tail = Tail.AsEnumerable().Reverse().Take(Zone.CommitLength);

					foreach(var r in tail)
						foreach(var t in Tables)
							t.Save(b, r.AffectedByTable(t));
	
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

				if(Settings.Base?.Chain != null)
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

		public Transaction FindLastTailTransaction(Func<Transaction, bool> transaction_predicate, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var t in r.Transactions)
					if(transaction_predicate == null || transaction_predicate(t))
						return t;

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
}
