using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Nethereum.Util;
using Nethereum.Web3;
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
		public static Money							TimeFactor(Time time) => new Money(time.Days * time.Days)/(Time.FromYears(1).Days);

		public abstract Guid						Guid { get; }
		public McvSettings							Settings;
		public Zone									Zone;
		public IClock								Clock;
		public object								Lock => Node.Lock;
		public Log									Log;
		public Node									Node;
		public Flow									Flow;
		bool										MinimalPeersReached;

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
		AutoResetEvent								TransactingWakeup = new AutoResetEvent(true);
		Thread										TransactingThread;

		public List<Round>							Tail = new();
		public Dictionary<int, Round>				LoadedRounds = new();
		Dictionary<AccountAddress, Transaction>		LastCandidacyDeclaration = new();
		public Round								LastConfirmedRound;
		public Round								LastCommittedRound;
		public Round								LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Votes.Any()) ?? LastConfirmedRound;
		public Round								LastPayloadRound	=> Tail.FirstOrDefault(i => i.VotesOfTry.Any(i => i.Transactions.Any())) ?? LastConfirmedRound;
		public Round								NextVoteRound => GetRound(LastConfirmedRound.Id + 1 + P);
		public List<Member>							NextVoteMembers => VotersOf(NextVoteRound);


		public Synchronization						Synchronization { set { _Synchronization = value; SynchronizationChanged?.Invoke(Node); } get { return _Synchronization; } }
		Synchronization								_Synchronization = Synchronization.None;
		SunDelegate									SynchronizationChanged;
		Thread										SynchronizingThread;
		public Dictionary<int, List<Vote>>			SyncTail = new();

		public List<Transaction>					IncomingTransactions = new();
		public List<Transaction>					OutgoingTransactions = new();

		public const string							ChainFamilyName = "Chain";
		public ColumnFamilyHandle					ChainFamily	=> Database.GetColumnFamily(ChainFamilyName);

		public bool									IsCommitReady(Round round) => (round.Id + 1) % Zone.CommitLength == 0; ///Tail.Count(i => i.Id <= round.Id) >= Zone.CommitLength; 
		public static int							GetValidityPeriod(int rid) => rid + P;

		protected abstract void						CreateTables(string databasepath);
		protected abstract void						GenesisCreate(Vote vote);
		protected abstract void						GenesisInitilize(Round vote);
		public abstract Round						CreateRound();
		public abstract Vote						CreateVote();
		public abstract void						FillVote(Vote vote);

		protected Mcv(Zone zone, McvSettings settings, string databasepath, bool skipinitload = false)
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

		public Mcv(Node sun, McvSettings settings, string databasepath, IClock clock, Flow flow) : this(sun.Zone, settings, databasepath)
		{
			Node = sun;
			Flow = flow;
			Clock = clock;

			VoteAdded += b => Node.MainWakeup.Set();

			ConsensusConcluded += (r, reached) =>	{
														if(reached)
														{
															/// Check OUR blocks that are not come back from other peer, means first peer went offline, if any - force broadcast them
															var notcomebacks = r.Parent.Votes.Where(i => i.Peers != null && !i.BroadcastConfirmed).ToArray();
					
															foreach(var v in notcomebacks)
																Node.Broadcast(this, v);
														}
														else
														{
															foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId == r.Id))
															{
																i.Vote = null;
																i.Status = TransactionStatus.Accepted;
															}
														}
													};

			Commited += r => {
								IncomingTransactions.RemoveAll(t => t.Vote?.Round != null && t.Vote.Round.Id <= r.Id || t.Expiration <= r.Id);
							};
		}

		public override string ToString()
		{
			//var gens = LastConfirmedRound != null ? Settings.Generators.Where(i => LastConfirmedRound.Members.Any(j => j.Account == i)) : [];
	
			return string.Join(", ", new string[]{	(Settings.Base != null ? "B" : null) +
													(Settings.Base?.Chain != null  ? "C" : null) +
													(Settings is RdnSettings x && x.Seed != null  ? "S" : null),
													Node.Connections(this).Count() < Settings.Peering.PermanentMin ? "Low Peers" : null,
													$"{Synchronization}/{LastConfirmedRound?.Id}/{LastConfirmedRound?.Hash.ToHexPrefix()}",
													$"T(i/o)={IncomingTransactions.Count}/{OutgoingTransactions.Count}"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
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
						r.ConsensusExeunitFee = Zone.ExeunitMinFee;
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

		public string CreateGenesis(AccountKey god, AccountKey f0)
		{
			/// 0 - emission request
			/// 1 - vote for emission 
			/// 1+P	 - emited
			/// 1+P + 1 - candidacy declaration
			/// 1+P + 1+P - decalared
			/// 1+P + 1+P + P - joined

			Clear();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
				r.ConsensusTransactions = r.OrderedTransactions.ToArray();
				r.Hashify();
				r.Write(w);
			}
	
			var v0 = CreateVote(); 
			{
				v0.RoundId = 0;
				v0.Time = Time.Zero;
				v0.ParentHash = Zone.Cryptography.ZeroHash;

				var t = new Transaction {Zone = Zone, Nid = 0, Expiration = 0};
				t.Generator = new([0, 0], -1);
				t.Fee = Zone.ExeunitMinFee;
				t.AddOperation(new Emission(Web3.Convert.ToWei(1_000_000, UnitConversion.EthUnit.Ether), 0));
				t.Sign(f0, Zone.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				v0.Sign(god);
				Add(v0);
				///v0.FundJoiners = v0.FundJoiners.Append(Zone.Father0).ToArray();
				write(0);
			}
			
			/// UO Autor

			var v1 = CreateVote(); 
			{
				v1.RoundId = 1; 
				v1.Time = Time.Zero; 
				v1.ParentHash = Zone.Cryptography.ZeroHash;

				GenesisCreate(v1);
	
				v1.Sign(god);
				Add(v1);
				write(1);
			}
	
			for(int i = 2; i <= 1+P + 1+P + P; i++)
			{
				var v = CreateVote(); 	
				v.RoundId		= i;
				v.Time			= Time.Zero;  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
				v.ParentHash	= i < P ? Zone.Cryptography.ZeroHash : GetRound(i - P).Summarize();
		 
				if(i == 1+P + 1)
				{
					var t = new Transaction {Zone = Zone, Nid = 1, Expiration = i};
					t.Generator = new([0, 0], -1);
					t.Fee = Zone.ExeunitMinFee;
					t.AddOperation(new CandidacyDeclaration{Bail = 1_000_000,
															BaseRdcIPs = [Zone.Father0IP],
															SeedHubRdcIPs = [Zone.Father0IP] });
					t.Sign(f0, Zone.Cryptography.ZeroHash);
					v.AddTransaction(t);
				}
	
				v.Sign(god);
				Add(v);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		public void Stop()
		{
			TransactingThread?.Join();
			SynchronizingThread?.Join();
			Database.Dispose();
		}

		public bool ProcessConnectivity()
		{
			var s = false;

			if(!MinimalPeersReached && 
				Node.Connections(this).Count(i => i.Permanent) >= Settings.Peering.PermanentMin && 
				(Settings.Base == null || Node.Bases(this).Count() >= Settings.Peering.PermanentBaseMin))
			{
				MinimalPeersReached = true;
				Flow.Log?.Report(this, $"Minimal peers reached");

				if(Settings.Base != null)
				{
					Synchronize();
				}

				s = true;
			}

			Generate();

			return s;
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

		public void Synchronize()
		{
			if(Node.Settings.IP != null && Node.Settings.IP.Equals(Zone.Father0IP) && Settings.Generators.Contains(Zone.Father0) && LastNonEmptyRound.Id == LastGenesisRound || NodeGlobals.SkipSynchronization)
			{
				Synchronization = Synchronization.Synchronized;
				return;
			}

			if(Synchronization != Synchronization.Downloading)
			{
				Flow.Log?.Report(this, $"Synchronization Started");

				SynchronizingThread = Node.CreateThread(Synchronizing);
				SynchronizingThread.Name = $"{Node.Settings.IP?.GetAddressBytes()[3]} Synchronizing";
				SynchronizingThread.Start();
		
				Synchronization = Synchronization.Downloading;
			}
		}

		void Synchronizing()
		{
			var used = new HashSet<Peer>();
	
			StampResponse stamp = null;
			Peer peer = null;

			lock(Node.Lock)
				SyncTail.Clear();

			while(Flow.Active)
			{
				try
				{
					WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 500);

					peer = Node.Connect(Guid, (long)(Settings.Base.Chain != null ? Role.Chain : Role.Base), used, Flow);

					if(Settings.Base?.Chain == null)
					{
						stamp = Call(peer, new StampRequest());
		
						void download(TableBase t)
						{
							var ts = Call(peer, new TableStampRequest{	Table = t.Id, 
																		SuperClusters = stamp.Tables[t.Id].SuperClusters.Where(i => !t.SuperClusters.TryGetValue(i.Id, out var c) || !c.SequenceEqual(i.Hash))
																														.Select(i => i.Id).ToArray()});
		
							foreach(var i in ts.Clusters)
							{
								var c = t.Clusters.FirstOrDefault(j => j.Id.SequenceEqual(i.Id));
		
								if(c == null || c.Hash == null || !c.Hash.SequenceEqual(i.Hash))
								{
									if(c == null)
									{
										c = t.AddCluster(i.Id);
									}
		
									var d = Call(peer, new DownloadTableRequest{Table = t.Id, 
																				ClusterId = i.Id, 
																				Offset = 0, 
																				Length = i.Length});
											
									c.Read(new BinaryReader(new MemoryStream(d.Data)));
										
									lock(Node.Lock)
									{
										using(var b = new WriteBatch())
										{
											c.Save(b);
			
											if(!c.Hash.SequenceEqual(i.Hash))
											{
												throw new SynchronizationException("Cluster hash mismatch");
											}
										
											Database.Write(b);
										}
									}
		
									Flow.Log?.Report(this, $"Cluster downloaded {t.GetType().Name}, {c.Id.ToHex()}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						foreach(var i in Tables)
						{
							download(i);
						}
		
						var r = CreateRound();
						r.Confirmed = true;
						r.ReadBaseState(new BinaryReader(new MemoryStream(stamp.BaseState)));
		
						var s = Call(peer, new StampRequest());

						lock(Node.Lock)
						{
							BaseState = stamp.BaseState;
							LastConfirmedRound = r;
							LastCommittedRound = r;
			
							Hashify();
			
							if(s.BaseHash.SequenceEqual(BaseHash))
	 							LoadedRounds[r.Id] = r;
							else
								throw new SynchronizationException("BaseHash mismatch");
						}
					}
		
					//int finalfrom = -1; 
					int from = -1;
					int to = -1;

					//lock(Lock)
					//	Tail.RemoveAll(i => i.Id > LastConfirmedRound.Id);

					while(Flow.Active)
					{
						lock(Node.Lock)
							if(Settings.Base?.Chain != null)
								from = LastConfirmedRound.Id + 1;
							else
								from = Math.Max(stamp.FirstTailRound, LastConfirmedRound == null ? -1 : (LastConfirmedRound.Id + 1));
		
						to = from + P;
		
						var rp = Call(peer, new DownloadRoundsRequest {From = from, To = to});

						lock(Node.Lock)
						{
							var rounds = rp.Read(this);
														
							for(int rid = from; rounds.Any() && rid <= rounds.Max(i => i.Id); rid++)
							{
								var r = rounds.FirstOrDefault(i => i.Id == rid);

								if(r == null)
									break;
								
								Flow.Log?.Report(this, $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.IP}");
									
								if(LastConfirmedRound.Id + 1 != rid)
								 	throw new IntegrityException();
	
								if(Enumerable.Range(rid, P + 1).All(SyncTail.ContainsKey) && (Settings.Base.Chain != null || FindRound(r.VotersRound) != null))
								{
									var p =	SyncTail[rid];
									var c =	SyncTail[rid + P];

									try
									{
										foreach(var v in p)
											ProcessIncoming(v, true);
		
										foreach(var v in c)
											ProcessIncoming(v, true);
									}
									catch(ConfirmationException)
									{
									}

									if(LastConfirmedRound.Id == rid && LastConfirmedRound.Hash.SequenceEqual(r.Hash))
									{
										//Commit(LastConfirmedRound);
										
										foreach(var i in SyncTail.OrderBy(i => i.Key).Where(i => i.Key > rid))
										{
											foreach(var v in i.Value)
												ProcessIncoming(v, true);
										}
										
										Synchronization = Synchronization.Synchronized;
										SyncTail.Clear();
										SynchronizingThread = null;
						
										Node.MainWakeup.Set();

										Flow.Log?.Report(this, $"Synchronization Finished");
										return;
									}
								}

								Tail.RemoveAll(i => i.Id >= rid);
								Tail.Insert(0, r);

// 								if(r.Id == 299)
// 									Debugger.Break();
			
								var h = r.Hash;

								r.Hashify();

								if(!r.Hash.SequenceEqual(h))
								{
									#if DEBUG
										Node.CompareBase(this, "a:\\UOTMP\\Simulation-Sun.Fast\\");
									#endif
									
									throw new SynchronizationException("!r.Hash.SequenceEqual(h)");
								}

								r.Confirmed = false;
								r.Confirm();

								if(r.Members.Count == 0)
									throw new SynchronizationException("Incorrect round (Members.Count == 0)");

								Commit(r);
								
								foreach(var i in SyncTail.Keys)
									if(i <= rid)
										SyncTail.Remove(i);
							}

							Thread.Sleep(1);
						}
					}
				}
				catch(ConfirmationException ex)
				{
					Flow.Log?.ReportError(this, ex.Message);

					lock(Node.Lock)
					{	
						//foreach(var i in SyncTail.Keys)
						//	if(i <= ex.Round.Id)
						//		SyncTail.Remove(i);

						Tail.RemoveAll(i => i.Id >= ex.Round.Id);
						//LastConfirmedRound = Tail.First();
					}
				}
				catch(SynchronizationException ex)
				{
					Flow.Log?.ReportError(this, ex.Message);

					used.Add(peer);

					lock(Node.Lock)
					{	
						//SyncTail.Clear();
						Clear();
						Initialize();
					}
				}
				catch(NodeException ex)
				{
					if(ex.Error != NodeError.TooEearly)
						used.Add(peer);
				}
				catch(EntityException)
				{
				}
			}
		}


		public bool ProcessIncoming(Vote v, bool assynchronized)
		{
			if(!v.Valid)
				return false;

			if(!assynchronized && (Synchronization == Synchronization.None || Synchronization == Synchronization.Downloading))
			{
 				//var min = SyncTail.Any() ? SyncTail.Max(i => i.Key) - Pitch * 100 : 0; /// keep latest Pitch * 3 rounds only
 
				if(SyncTail.TryGetValue(v.RoundId, out var r) && r.Any(j => j.Signature.SequenceEqual(v.Signature)))
					return false;

				if(!SyncTail.TryGetValue(v.RoundId, out r))
				{
					r = SyncTail[v.RoundId] = new();
				}

				v.Created = DateTime.UtcNow;
				r.Add(v);

				//foreach(var i in SyncTail.Keys)
				//{
				//	if(i < min)
				//	{
				//		SyncTail.Remove(i);
				//	}
				//}
			}
			else if(assynchronized || Synchronization == Synchronization.Synchronized)
			{
				if(v.RoundId <= LastConfirmedRound.Id || LastConfirmedRound.Id + P * 2 < v.RoundId)
					return false;

				//if(v.RoundId <= LastVotedRound.Id - Pitch / 2)
				//	return false;

				var r = GetRound(v.RoundId);

				if(!VotersOf(r).Any(i => i.Account == v.Generator))
					return false;

				if(r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
					return false;
								
				if(r.Parent != null && r.Parent.Members.Count > 0)
				{
					if(v.Transactions.Length > r.Parent.TransactionsPerVoteAllowableOverflow)
						return false;

					if(v.Transactions.Sum(i => i.Operations.Length) > r.Parent.OperationsPerVoteLimit)
						return false;

					if(v.Transactions.Any(t => VotersOf(r).NearestBy(m => m.Account, t.Signer).Account != v.Generator))
						return false;
				}

				if(v.Transactions.Any(i => !i.Valid(this))) /// do it only after adding to the chainbase
				{
					//r.Votes.Remove(v);
					return false;
				}

				Add(v);
			}

			return true;
		}
		
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
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var t in Tables)
				foreach(var i in t.SuperClusters.OrderBy(i => i.Key))
					BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}

		public void Generate()
		{
			Node.Statistics.Generating.Begin();

			if(Settings.Generators.Length ==0 || Synchronization != Synchronization.Synchronized)
				return;

			var votes = new List<Vote>();

			foreach(var g in Settings.Generators)
			{
				var m = NextVoteMembers.Find(i => i.Account == g);

				if(m == null)
				{
					m = LastConfirmedRound.Members.Find(i => i.Account == g);

					var a = Accounts.Find(g, LastConfirmedRound.Id);

					if(m == null && a != null && a.Balance > Settings.Bail && (!LastCandidacyDeclaration.TryGetValue(g, out var d) || d.Status > TransactionStatus.Placed))
					{
						var o = new CandidacyDeclaration{	Bail			= Settings.Bail,
															BaseRdcIPs		= [Node.Settings.IP],
															SeedHubRdcIPs	= [Node.Settings.IP]};

						var t = new Transaction();
						t.Flow = Flow;
						t.Zone = Zone;
						t.Signer = g;
 						t.__ExpectedStatus = TransactionStatus.Confirmed;
			
						t.AddOperation(o);

			 			Transact(t);

						LastCandidacyDeclaration[g] = t;
					}
				}
				else
				{
					if(LastCandidacyDeclaration.TryGetValue(g, out var lcd))
						OutgoingTransactions.Remove(lcd);

					var r = NextVoteRound;

					if(r.Id < m.CastingSince)
						continue;

					if(r.VotesOfTry.Any(i => i.Generator == g))
						continue;

					Vote createvote(Round r)
					{
						var prev = r.Previous?.VotesOfTry.FirstOrDefault(i => i.Generator == g);
						
						var v = CreateVote();

						v.RoundId		= r.Id;
						v.Try			= r.Try;
						v.ParentHash	= r.Parent.Hash ?? r.Parent.Summarize();
						v.Created		= Clock.Now;
						v.Time			= Time.Now(Clock);
						v.Violators		= ProposeViolators(r).ToArray();
						v.MemberLeavers	= ProposeMemberLeavers(r, g).ToArray();
						//v.FundJoiners	= Settings.ProposedFundJoiners.Where(i => !LastConfirmedRound.Funds.Contains(i)).ToArray();
						//v.FundLeavers	= Settings.ProposedFundLeavers.Where(i => LastConfirmedRound.Funds.Contains(i)).ToArray();

						FillVote(v);

						return v;
					}

					var txs = IncomingTransactions.Where(i => i.Status == TransactionStatus.Accepted).ToArray();
	
					if(txs.Any() || Tail.Any(i => LastConfirmedRound.Id < i.Id && i.Payloads.Any())) /// any pending foreign transactions or any our pending operations OR some unconfirmed payload 
					{
						var v = createvote(r);
						var deferred = new List<Transaction>();
						
						/// Compose txs list prioritizing higher fees but ensure continuous tx Nid sequence 

						bool add(Transaction t, bool isdeferred)
						{ 	
							if(v.Transactions.Sum(i => i.Operations.Length) + t.Operations.Length > r.Parent.OperationsPerVoteLimit)
								return false;
	
							if(v.Transactions.Length + 1 > r.Parent.TransactionsPerVoteAllowableOverflow)
								return false;
	
							if(r.Id > t.Expiration)
							{
								t.Status = TransactionStatus.FailedOrNotFound;
								IncomingTransactions.Remove(t);
								return true;
							}

							var nearest = VotersOf(r).NearestBy(m => m.Account, t.Signer).Account;

							if(nearest != g)
								return true;
	
							if(!Settings.Generators.Contains(nearest))
							{
								t.Status = TransactionStatus.FailedOrNotFound;
								IncomingTransactions.Remove(t);
								return true;
							}
	
							if(!isdeferred)
							{
								if(txs.Any(i => i.Signer == t.Signer && i.Nid < t.Nid)) /// any older tx left?
								{
									deferred.Add(t);
									return true;
								}
							}
							else
								deferred.Remove(t);

							t.Status = TransactionStatus.Placed;
							v.AddTransaction(t);
	
							var next = deferred.Find(i => i.Signer == t.Signer && i.Nid + 1 == t.Nid);

							if(next != null)
							{
								if(add(next, true) == false)
									return false;
							}
	
							Flow.Log?.Report(this, "Transaction Placed", t.ToString());

							return true;
						}

						foreach(var t in txs.OrderByDescending(i => i.Fee).ToArray())
						{
							if(add(t, false) == false)
								break;
						}

						if(v.Transactions.Any() || Tail.Any(i => LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						{
							v.Sign(Node.Vault.GetKey(g));
							votes.Add(v);
						}
					}

 					while(r.Previous != null && !r.Previous.Confirmed && VotersOf(r.Previous).Any(i => i.Account == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
 					{
 						r = r.Previous;
 
 						var b = createvote(r);
 								
 						b.Sign(Node.Vault.GetKey(g));
 						votes.Add(b);
 					}

					if(IncomingTransactions.Any(i => i.Status == TransactionStatus.Accepted) || Tail.Any(i => LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						Node.MainWakeup.Set();
				}
			}

			if(votes.Any())
			{
				try
				{
					foreach(var v in votes.GroupBy(i => i.RoundId).OrderBy(i => i.Key))
					{
						var r = FindRound(v.Key);

						foreach(var i in v)
						{
							Add(i);
						}
					}

					for(int i = LastConfirmedRound.Id + 1; i <= LastNonEmptyRound.Id; i++) /// better to start from votes.Min(i => i.Id) or last excuted
					{
						var r = GetRound(i);
						
						if(r.Hash == null)
						{
							r.ConsensusTime			= r.Previous.ConsensusTime;
							r.ConsensusExeunitFee	= r.Previous.ConsensusExeunitFee;
							r.RentPerBytePerDay		= r.Previous.RentPerBytePerDay;
							r.Members				= r.Previous.Members;
							r.Funds					= r.Previous.Funds;
						}

						if(!r.Confirmed)
						{
							r.Execute(r.OrderedTransactions.Where(i => Settings.Generators.Contains(i.Vote.Generator)));
						}
					}
				}
				catch(ConfirmationException ex)
				{
					ProcessConfirmationException(ex);
				}

				foreach(var i in votes)
				{
					Node.Broadcast(this, i);
				}
													
				 Flow.Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{i.Generator.Bytes.ToHexPrefix()}-{i.RoundId}")));
			}

			Node.Statistics.Generating.End();
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
			r.RentPerBytePerDay		= p.RentPerBytePerDay;
			r.Members				= p.Members;
			r.Funds					= p.Funds;
	
			r.Execute([transaction]);

			return r;
		}

		public abstract bool ProcessIncomingOperation(Operation o);

		public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			foreach(var t in txs.Where(i =>	!IncomingTransactions.Any(j => j.Signer == i.Signer && j.Nid == i.Nid) &&
											(i.EmissionOnly || i.Fee >= i.Operations.Length * LastConfirmedRound.ConsensusExeunitFee) &&
											i.Expiration > LastConfirmedRound.Id &&
											i.Valid(this)).OrderByDescending(i => i.Nid))
			{
				if(t.Operations.Any(o => ! ProcessIncomingOperation(o)))
					continue;

				TryExecute(t);
				
				if(t.Successful)
				{
					t.Status = TransactionStatus.Accepted;
					IncomingTransactions.Add(t);

					Flow.Log?.Report(this, "Transaction Accepted", t.ToString());

					yield return t;
				}
			}

			Node.MainWakeup.Set();
		}

		public void ProcessConfirmationException(ConfirmationException ex)
		{
			Flow.Log?.ReportError(this, ex.Message);
			Tail.RemoveAll(i => i.Id >= ex.Round.Id);

			//foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId >= ex.Round.Id && (i.Placing == PlacingStage.Placed || i.Placing == PlacingStage.Confirmed)).ToArray())
			//{
			//	i.Placing = PlacingStage.Accepted;
			//}

			Synchronize();
		}

		void Transacting()
		{
			//IEnumerable<Transaction>	accepted;

			Flow.Log?.Report(this, "Transacting started");

			while(Flow.Active)
			{
				if(!OutgoingTransactions.Any())
					WaitHandle.WaitAny([TransactingWakeup, Flow.Cancellation.WaitHandle]);

				var cr = Call(() => new MembersRequest(), Flow);

				if(!cr.Members.Any() || cr.Members.Any(i => !i.BaseRdcIPs.Any() || !i.SeedHubRdcIPs.Any()))
					continue;

				var members = cr.Members;

				IPeer getrdi(AccountAddress account)
				{
					var m = members.NearestBy(i => i.Account, account);

					if(m.BaseRdcIPs.Contains(Node.Settings.IP))
						return Node;

					var p = Node.GetPeer(m.BaseRdcIPs.Random());
					Node.Connect(p, Flow);

					return p;
				}

				Node.Statistics.Transacting.Begin();
				
				IEnumerable<IGrouping<AccountAddress, Transaction>> nones;

				lock(Node.Lock)
					nones = OutgoingTransactions.GroupBy(i => i.Signer).Where(g => !g.Any(i => i.Status >= TransactionStatus.Accepted) && g.Any(i => i.Status == TransactionStatus.None)).ToArray();

				foreach(var g in nones)
				{
					var m = members.NearestBy(i => i.Account, g.Key);

					//AllocateTransactionResponse at = null;
					IPeer rdi; 

					try
					{
						rdi = getrdi(g.Key);
					}
					catch(NodeException)
					{
						Thread.Sleep(1000);
						continue;
					}

					int nid = -1;
					var txs = new List<Transaction>();

					foreach(var t in g.Where(i => i.Status == TransactionStatus.None))
					{
						try
						{
							t.Rdi = rdi;
							t.Fee = 0;
							t.Nid = 0;
							t.Expiration = 0;
							t.Generator = new([0, 0], -1);

							t.Sign(Node.Vault.GetKey(t.Signer), Zone.Cryptography.ZeroHash);

							var at = Call(rdi, new AllocateTransactionRequest {Transaction = t});
								
							if(nid == -1)
								nid = at.NextNid;
							else
								nid++;

							t.Generator	 = at.Generetor;
							t.Fee		 = t.EmissionOnly ? 0 : at.MinFee;
							t.Nid		 = nid;
							t.Expiration = at.LastConfirmedRid + TransactionPlacingLifetime;

							t.Sign(Node.Vault.GetKey(t.Signer), at.PowHash);
							txs.Add(t);
						}
						catch(NodeException)
						{
							Thread.Sleep(1000);
							continue;
						}
						catch(EntityException ex)
						{
							//if(t.__ExpectedStatus == TransactionStatus.FailedOrNotFound)
							//{
								lock(Node.Lock)
								{
									t.Status = TransactionStatus.FailedOrNotFound;
									OutgoingTransactions.Remove(t);
								}

								t.Flow.Log?.Report(this, "Allocation failed", $"{t} -> {m}, {t.Rdi}");

							//} 
							//else
							//	Thread.Sleep(1000);

							continue;
						}
					}

					IEnumerable<byte[]> atxs = null;

					try
					{
						atxs = Call(rdi, new PlaceTransactionsRequest {Transactions = txs.ToArray()}).Accepted;
					}
					catch(NodeException)
					{
						Thread.Sleep(1000);
						continue;
					}

					lock(Node.Lock)
						foreach(var t in txs)
						{ 
							if(atxs.Any(s => s.SequenceEqual(t.Signature)))
							{
								t.Status = TransactionStatus.Accepted;
								t.Flow.Log?.Report(this, "Accepted", $"{t} -> {m}, {t.Rdi}");
							}
							else
							{
								if(t.__ExpectedStatus == TransactionStatus.FailedOrNotFound)
								{
									t.Status = TransactionStatus.FailedOrNotFound;
									OutgoingTransactions.Remove(t);
								} 
								else
								{
									t.Status = TransactionStatus.None;
								}

								t.Flow.Log?.Report(this, "Rejected", $"{t} -> {m}, {t.Rdi}");
							}
						}
				}

				Transaction[] accepted;
				
				lock(Node.Lock)
					accepted = OutgoingTransactions.Where(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed).ToArray();

				if(accepted.Any())
				{
					foreach(var g in accepted.GroupBy(i => i.Rdi))
					{
						TransactionStatusResponse ts;

						try
						{
							ts = Call(g.Key, new TransactionStatusRequest {Mcv = this, Transactions = g.Select(i => new TransactionsAddress {Account = i.Signer, Nid = i.Nid}).ToArray()});
						}
						catch(NodeException)
						{
							Thread.Sleep(1000);
							continue;
						}

						lock(Node.Lock)
							foreach(var i in ts.Transactions)
							{
								var t = accepted.First(d => d.Signer == i.Account && d.Nid == i.Nid);
																		
								if(t.Status != i.Status)
								{
									t.Flow.Log?.Report(this, i.Status.ToString(), $"{t} -> {t.Generator}, {t.Rdi}");

									t.Status = i.Status;

									if(t.Status == TransactionStatus.FailedOrNotFound)
									{
										if(t.__ExpectedStatus == TransactionStatus.Confirmed)
											t.Status = TransactionStatus.None;
										else
											OutgoingTransactions.Remove(t);
									}
									else if(t.Status == TransactionStatus.Confirmed)
									{
										if(t.__ExpectedStatus == TransactionStatus.FailedOrNotFound)
											Debugger.Break();
										else
											OutgoingTransactions.Remove(t);
									}
								}
						}
					}
				}
				
				Node.Statistics.Transacting.End();
			}
		}

		void Transact(Transaction t)
		{
			if(OutgoingTransactions.Count <= OperationsQueueLimit)
			{
				if(TransactingThread == null)
				{
					TransactingThread = Node.CreateThread(Transacting);

					TransactingThread.Name = $"{Node.Settings.IP?.GetAddressBytes()[3]} Transacting";
					TransactingThread.Start();
				}

				OutgoingTransactions.Add(t);
				TransactingWakeup.Set();
			} 
			else
			{
				Flow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
		public Transaction Transact(Operation operation, AccountAddress signer, TransactionStatus await, Flow workflow)
		{
			return Transact([operation], signer, await, workflow)[0];
		}

 		public Transaction[] Transact(IEnumerable<Operation> operations, AccountAddress signer, TransactionStatus await, Flow workflow)
 		{
			if(!Node.Vault.IsUnlocked(signer))
			{
				throw new NodeException(NodeError.NotUnlocked);
			}

			var p = new List<Transaction>();

			while(operations.Any())
			{
				var t = new Transaction();
				t.Zone = Zone;
				t.Signer = signer;
				t.Flow = workflow;
 				t.__ExpectedStatus = await;
			
				foreach(var i in operations.Take(Zone.OperationsPerTransactionLimit))
				{
					t.AddOperation(i);
				}

 				lock(Node.Lock)
				{	
			 		Transact(t);
				}
 
				Await(t, await, workflow);

				p.Add(t);

				operations = operations.Skip(Zone.OperationsPerTransactionLimit);
			}

			return p.ToArray();
 		}

		void Await(Transaction t, TransactionStatus s, Flow workflow)
		{
			while(workflow.Active)
			{ 
				switch(s)
				{
					case TransactionStatus.None :				return;
					case TransactionStatus.Accepted :			if(t.Status >= TransactionStatus.Accepted) goto end; else break;
					case TransactionStatus.Placed :				if(t.Status >= TransactionStatus.Placed) goto end; else break;
					case TransactionStatus.Confirmed :			if(t.Status == TransactionStatus.Confirmed) goto end; else break;
					case TransactionStatus.FailedOrNotFound :	if(t.Status == TransactionStatus.FailedOrNotFound) goto end; else break;
				}

			}
			
			Thread.Sleep(100);

			end:
				workflow.Log?.Report(this, $"Transaction is {t.Status}", t.ToString());
		}

		public Rp Call<Rp>(IPeer peer, PeerCall<Rp> rq) where Rp : PeerResponse
		{
			rq.Mcv		= this;
			rq.Sun		= Node;
			rq.McvId	= Guid;

			return peer.Send((PeerRequest)rq) as Rp;
		}

		public R Call<R>(Func<PeerCall<R>> call, Flow workflow, IEnumerable<Peer> exclusions = null)  where R : PeerResponse
		{
			return Call((Func<PeerRequest>)call, workflow, exclusions) as R;
		}

		public PeerResponse Call(Func<PeerRequest> call, Flow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Node.Lock)
				{
					if(Synchronization == Synchronization.Synchronized)
					{
						var c = call();
						c.Mcv	= this;
						c.Sun	= Node;
						c.McvId = Guid;

						return Node.Send(c);
					}

					p = Node.ChooseBestPeer(Guid, (long)Role.Base, tried);
	
					if(p == null)
					{
						tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();
						continue;
					}
				}

				tried.Add(p);

				try
				{
					Node.Connect(p, workflow);

					var c = call();
					c.Mcv	= this;
					c.Sun	= Node;
					c.McvId = Guid;

					return p.Send(c);
				}
 				catch(NodeException)
 				{
					p.LastFailure[Role.Base] = DateTime.UtcNow;
 				}
				catch(ContinueException)
				{
				}
			}

			throw new OperationCanceledException();
		}


		public R Call<R>(IPAddress ip, Func<PeerCall<R>> call, Flow workflow) where R : PeerResponse
		{
			var p = Node.GetPeer(ip);

			Node.Connect(p, workflow);

			var c = call();
			c.Mcv	= this;
			c.Sun	= Node;
			c.McvId = Guid;

			return p.Send(c);
		}

		public void Tell(IPAddress ip, PeerRequest requet, Flow workflow)
		{
			var p = Node.GetPeer(ip);

			Node.Connect(p, workflow);

			var c = requet;
			c.Mcv	= this;
			c.Sun	= Node;
			c.McvId = Guid;

			p.Post(c);

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

		public void RunPeer()
		{
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
