using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using RocksDbSharp;

namespace Uccs.Net;

public enum McvPpcClass : byte
{
	None = 0, 
	SharePeers = PpcClass._Last + 1, 
	Info,
	Vote, Time, Members, Funds, AllocateTransaction, PlaceTransactions, TransactionStatus, Account, 
	Stamp, TableStamp, DownloadTable, DownloadRounds,
	Cost,
	_Last = 199
}

public enum Synchronization
{
	None, Downloading, Synchronized
}

[Flags]
public enum Role : long
{
	None,
	Graph		= 0b00000001,
	Chain		= 0b00000010,
}

public abstract class McvTcpPeering : HomoTcpPeering
{
	new public McvNode						Node => base.Node as McvNode;
	public McvNet							Net => Node.Net;
	public Mcv								Mcv => Node.Mcv; 
	public UosApiClient						UosApi; 

	public IEnumerable<Peer>				Graphs => Connections.Where(i => i.Permanent && i.Roles.IsSet(Role.Graph));

	public bool								MinimalPeersReached;
	AutoResetEvent							TransactingWakeup = new AutoResetEvent(true);
	Thread									TransactingThread;
	public List<Transaction>				OutgoingTransactions = new();
	public List<Transaction>				IncomingTransactions = new();
	public List<Transaction>				ArchivedTransactions = new();

	public Synchronization					Synchronization { get; protected set; } = Synchronization.None;
	Thread									SynchronizingThread;
	public Dictionary<int, List<Vote>>		SyncTail = new();

	List<AccountAddress>					CandidacyDeclarations = [];
	
	public static List<McvTcpPeering>		All = new();

	public McvTcpPeering(McvNode node, PeeringSettings settings, long roles, UosApiClient uosapi, Flow flow) : base(node, node.Net, settings, roles, flow)
	{
		UosApi = uosapi;

		Register(typeof(McvPpcClass), node);

		All.Add(this);
	}

	public override object Constract(Type t, byte b)
	{
		if(t == typeof(Transaction))	return new Transaction {Net = Net}; 
 		if(t == typeof(Vote))			return new Vote(Mcv);

		return base.Constract(t, b);
	}
	
	public override byte TypeToCode(Type i)
	{
		return base.TypeToCode(i);
	}

	public override void Run()
	{
		base.Run();

		if(Mcv != null)
		{
			Mcv.VoteAdded += b => MainWakeup.Set();

			Mcv.ConsensusConcluded += (r, reached) =>	{
															if(reached)
															{
																///// Check OUR blocks that are not come back from other peer, means first peer went offline, if any - force broadcast them
																//var notcomebacks = r.Parent.Votes.Where(i => i.Peers != null && !i.BroadcastConfirmed).ToArray();
																//
																//foreach(var v in notcomebacks)
																//	Broadcast(v);
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

			Mcv.Commited += r => {
									bool old(Transaction t) => t.Vote?.Round != null && t.Vote.Round.Id <= r.Id || t.Expiration <= r.Id;

									ArchivedTransactions.AddRange(IncomingTransactions.Where(old));
									IncomingTransactions.RemoveAll(old);
									ArchivedTransactions.RemoveAll(t => t.Expiration < Mcv.LastConfirmedRound.Id - Net.CommitLength);
								 };
		}
	}

	public override void Stop()
	{
		TransactingThread?.Join();
		SynchronizingThread?.Join();

		base.Stop();

		All.Remove(this);
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);

		if(ex.Error == NodeError.NotGraph)	peer.Roles  &= ~(long)Role.Graph;
		if(ex.Error == NodeError.NotChain)	peer.Roles  &= ~(long)Role.Chain;
	}

	protected override void ProcessConnectivity()
	{
		base.ProcessConnectivity();

		if(Mcv != null)
		{
			var needed = Settings.PermanentGraphsMin - Graphs.Count();

			foreach(var p in Peers	.Where(p =>	p.Status == ConnectionStatus.Disconnected && DateTime.UtcNow - p.LastTry > TimeSpan.FromSeconds(5))
									.OrderBy(i => i.Retries)
									.ThenByDescending(i => i.Roles.IsSet(Role.Graph))
									.ThenBy(i => Settings.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
									.Take(needed))
			{
				OutboundConnect(p, true);
			}
		}

		if(!MinimalPeersReached && 
			Connections.Count(i => i.Permanent) >= Settings.PermanentMin && 
			(Mcv == null || Graphs.Count() >= Settings.PermanentGraphsMin))
		{
			MinimalPeersReached = true;
			Flow.Log?.Report(this, $"Minimal peers reached");

			if(IsListener)
			{
				foreach(var c in Connections)
					c.Post(new SharePeersRequest {Broadcast = true, 
												  Peers = [new Peer(IP, Settings.Port) {Roles = Roles}]});
			}

			if(Mcv != null)
			{
				lock(Mcv.Lock)
					Synchronize();
			}
		}

		if(Mcv != null && MinimalPeersReached)
		{
			lock(Mcv.Lock)
				Generate();
		}
	}

	public void Synchronize()
	{
		if(Settings.IP != null && Settings.IP.Equals(Net.Father0IP) && Mcv.Settings.Generators.Contains(Net.Father0) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound || NodeGlobals.SkipSynchronization)
		{
			Synchronization = Synchronization.Synchronized;
			return;
		}

		if(Synchronization != Synchronization.Downloading)
		{
			Flow.Log?.Report(this, $"Synchronization Started");

			SynchronizingThread = Node.CreateThread(Synchronizing);
			SynchronizingThread.Name = $"{Node.Name} Synchronizing";
			SynchronizingThread.Start();
	
			Synchronization = Synchronization.Downloading;
		}
	}

	void Synchronizing()
	{
		var used = new HashSet<Peer>();

		StampResponse stamp = null;
		Peer peer = null;

		lock(Lock)
			SyncTail.Clear();

		while(Flow.Active)
		{
			try
			{
				WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 500);

				peer = Connect(Mcv.Settings.Roles, used, Flow);

				if(Mcv.Settings.Chain == null)
				{
					stamp = Call(peer, new StampRequest());
	
					void download(TableBase t)	{
													var ts = Call(peer, new TableStampRequest {Table = t.Id, 
																							   Clusters = stamp.Tables[t.Id].Clusters.Where(i => !t.FindCluster(i.Id)?.Hash?.SequenceEqual(i.Hash) ?? true) 
																																	 .Select(i => i.Id)
																																	 .ToArray()});
													using(var w = new WriteBatch())
													{
														foreach(var i in ts.Clusters)
														{
															///lock(Mcv.Lock)	
															{
																var c = t.GetCluster(i.Id);

																foreach(var j in i.Buckets)
																{
																	var b = c.GetBucket(j.Id);
			
																	if(b.Hash == null || !b.Hash.SequenceEqual(j.Hash))
																	{
																		var d = Call(peer, new DownloadTableRequest {Table		= t.Id,
																													 BucketId	= j.Id, 
																													 Hash		= j.Hash});
																		lock(Mcv.Lock)	
																			b.Import(w, d.Main);
				
																		if(!b.Hash.SequenceEqual(j.Hash))
																			throw new SynchronizationException("Cluster hash mismatch");
			
																		Flow.Log?.Report(this, $"Bucket downloaded {t.GetType().Name}, {b.Id}");
																	}
																}

																c.Commit(w);
															}
														}
							
														Mcv.Rocks.Write(w);
													}
												}
	
					while(Flow.Active)
					{
						foreach(var i in Mcv.Tables.Where(i => !i.IsIndex))
						{
							download(i);
						}
		
						var r = Mcv.CreateRound();
						r.Confirmed = true;
						r.ReadGraphState(new BinaryReader(new MemoryStream(stamp.GraphState)));
		
						var s = Call(peer, new StampRequest());
	
						lock(Mcv.Lock)
						{
							Mcv.GraphState = stamp.GraphState;
							Mcv.LastConfirmedRound = r;
							Mcv.LastDissolvedRound = r;
			
							Mcv.Hashify();
			
							if(s.GraphHash.SequenceEqual(Mcv.GraphHash))
	 						{	
								Mcv.LoadedRounds[r.Id] = r;

								using(var w = new WriteBatch())
								{
									foreach(var i in Mcv.Tables.Where(i => !i.IsIndex))
										i.Index(w, r);

									//foreach(var i in Mcv.Tables)
									//	i.Commit();
						
									Mcv.Rocks.Write(w);
								}

								break;
							}
						}
					}
				}
	
				//int finalfrom = -1; 
				int from = -1;
				int to = -1;

				//lock(Lock)
				//	Tail.RemoveAll(i => i.Id > LastConfirmedRound.Id);

				while(Flow.Active)
				{
					lock(Mcv.Lock)
						if(Mcv.Settings.Chain != null)
							from = Mcv.LastConfirmedRound.Id + 1;
						else
							from = Math.Max(stamp.FirstTailRound, Mcv.LastConfirmedRound == null ? -1 : (Mcv.LastConfirmedRound.Id + 1));
	
					to = from + Mcv.P;
	
					var rp = Call(peer, new DownloadRoundsRequest {From = from, To = to});

					lock(Mcv.Lock)
					{
						var rounds = rp.Read(Mcv);
													
						foreach(var r in rounds)
						{
							//var r = rounds.FirstOrDefault(i => i.Id == rid);
							//
							//if(r == null)
							//	break;
							
							Flow.Log?.Report(this, $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.IP}");
								
							if(Mcv.LastConfirmedRound.Id + 1 != r.Id)
							 	throw new SynchronizationException();

							if(Enumerable.Range(r.Id, Mcv.P + 1).All(SyncTail.ContainsKey) && (Mcv.Settings.Chain != null || Mcv.FindRound(r.VotersId) != null))
							{
								var p =	SyncTail[r.Id];
								var c =	SyncTail[r.Id + Mcv.P];

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

								if(Mcv.LastConfirmedRound.Id == r.Id && Mcv.LastConfirmedRound.Hash.SequenceEqual(r.Hash))
								{
									//Commit(LastConfirmedRound);
									
									foreach(var i in SyncTail.OrderBy(i => i.Key).Where(i => i.Key > r.Id))
									{
										foreach(var v in i.Value)
											ProcessIncoming(v, true);
									}
									
									Synchronization = Synchronization.Synchronized;
									SyncTail.Clear();
									SynchronizingThread = null;
					
									MainWakeup.Set();

									Flow.Log?.Report(this, $"Synchronization Finished");
									return;
								}
							}

							Mcv.Tail.RemoveAll(i => i.Id >= r.Id);
							Mcv.Tail.Insert(0, r);

// 								if(r.Id == 299)
// 									Debugger.Break();
		
							var h = r.Hash;

							r.Hashify();

							if(!r.Hash.SequenceEqual(h))
							{
								#if DEBUG
									//CompareBase([this, All.First(i => i.Node.Name == peer.Name)], "a:\\1111111111111");
									lock(Mcv.Lock)
										Mcv.Dump();
									
									lock(All.First(i => i.Node.Name == peer.Name).Mcv.Lock)
										All.First(i => i.Node.Name == peer.Name).Mcv.Dump();
								
								#endif
																
								throw new SynchronizationException("!r.Hash.SequenceEqual(h)");
							}

							r.Confirmed = false;
							r.Confirm();

							if(r.Members.Count == 0)
								throw new SynchronizationException("Incorrect round (Members.Count == 0)");

							Mcv.Commit(r);
							
							foreach(var i in SyncTail.Keys)
								if(i <= r.Id)
									SyncTail.Remove(i);
						}

						Thread.Sleep(1);
					}
				}
			}
			catch(ConfirmationException ex)
			{
				Flow.Log?.ReportError(this, ex.Message);

				lock(Mcv.Lock)
				{	
					//foreach(var i in SyncTail.Keys)
					//	if(i <= ex.Round.Id)
					//		SyncTail.Remove(i);

					Mcv.Tail.RemoveAll(i => i.Id >= ex.Round.Id);
					//LastConfirmedRound = Tail.First();
				}
			}
			catch(SynchronizationException ex)
			{
				Flow.Log?.ReportError(this, ex.Message);

				used.Add(peer);

				lock(Mcv.Lock)
				{	
					//SyncTail.Clear();
					Mcv.Clear();
					Mcv.Initialize();
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

	public void ProcessConfirmationException(ConfirmationException ex)
	{
		Flow.Log?.ReportError(this, ex.Message);
		Mcv.Tail.RemoveAll(i => i.Id >= ex.Round.Id);

		//foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId >= ex.Round.Id && (i.Placing == PlacingStage.Placed || i.Placing == PlacingStage.Confirmed)).ToArray())
		//{
		//	i.Placing = PlacingStage.Accepted;
		//}

		Synchronize();
	}

	public bool ProcessIncoming(Vote v, bool fromsynchronization)
	{
		if(!v.Valid)
			return false;

		if(!fromsynchronization && Synchronization == Synchronization.Downloading)
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
		else if(fromsynchronization || Synchronization == Synchronization.Synchronized)
		{
			if(v.RoundId < Mcv.LastConfirmedRound.Id + 1 || Mcv.LastConfirmedRound.Id + Mcv.P + 1 < v.RoundId)
				return false;
			
			var r = Mcv.GetRound(v.RoundId);

			if(r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
				return false;
								
			if(!r.VotersRound.Members.Any(i => i.Address == v.Generator))
				return false;

			if(r.Forkers.Contains(v.Generator))
				return false;

			var e = r.VotesOfTry.FirstOrDefault(i => i.Generator == v.Generator);
			
			if(e != null) /// FORK
			{
				r.Votes.Remove(e);
				r.Forkers.Add(e.Generator);

				return false;
			}

			v.Restore();
							
			if(r.Parent != null && r.Parent.Members.Count > 0)
			{
				if(v.Transactions.Length > r.Parent.PerVoteTransactionsLimit)
					return false;

				if(v.Transactions.Sum(i => i.Operations.Length) > r.Parent.PerVoteOperationsLimit)
					return false;

				if(v.Transactions.Any(t => r.VotersRound.Members.NearestBy(m => m.Address, t.Signer).Address != v.Generator))
					return false;
			}

			if(v.Transactions.Any(i => !i.Valid(Mcv))) /// do it only after adding to the chainbase
			{
				//r.Votes.Remove(v);
				return false;
			}

// 				for(int i = r.Id - 1; i > Mcv.LastConfirmedRound.Id; i--)
// 				{
// 					Mcv.GetRound(i);
// 				}

			Mcv.Add(v);
		}

		return true;
	}

	/// <summary>
	/// 0	declaration
	/// 1
	/// 2
	/// 3
	/// 4
	/// 5
	/// 6
	/// 7	last confirmed
	/// 8	declaration voting, first vote for
	/// 9
	/// 10
	/// 11
	/// 12
	/// 13
	/// 14
	/// 15	
	/// 16	first vote
	/// </summary>

	public void Generate()
	{
		try
		{
			Statistics.Generating.Begin();
	
			if(Mcv.Settings.Generators.Length == 0 || Synchronization != Synchronization.Synchronized)
				return;
	
			var votes = new List<Vote>();
	
			foreach(var g in Mcv.Settings.Generators)
			{
				//if(g == Net.Father0	&& Mcv.NextVoteRound.Previous.Votes.Any(i => i.Generator == g)
				//						&& Mcv.NextVoteRound.Previous.Previous.Votes.Any(i => i.Generator == g)
				//						&& Mcv.NextVoteRound.Previous.Previous.Previous.Votes.Any(i => i.Generator == g)
				//					)
				//	votes = votes;
	
				var m = Mcv.NextVoteRound.VotersRound.Members.Find(i => i.Address == g);
	
				if(m == null)
				{
					if(Mcv.LastConfirmedRound.Candidates.Any(i => i.Address == g))
						continue;
	
					if(!CandidacyDeclarations.Contains(g) && GetSession(g) != null)
					{
						var t = new Transaction();
						t.Flow	 = Flow;
						t.Net	 = Net;
						t.Signer = g;
	 					t.ActionOnResult = ActionOnResult.RetryUntilConfirmed;
				
						t.AddOperation(Mcv.CreateCandidacyDeclaration());
	
						CandidacyDeclarations.Add(g);
	
				 		Transact(t);
					} 
					else
					{
						if(!OutgoingTransactions.Any(i => i.Signer == g && i.Operations.Any(o => o is CandidacyDeclaration)))
							CandidacyDeclarations.Remove(g);
					}
				}
				else
				{
					CandidacyDeclarations.Remove(g);
					OutgoingTransactions.RemoveAll(i => i.Signer == g && i.Operations.Any(o => o is CandidacyDeclaration));
	
					var r = Mcv.NextVoteRound;
	
					//if(r.Id < m.CastingSince)
					//	continue;
	
					if(r.VotesOfTry.Any(i => i.Generator == g))
						continue;
	
					Vote createvote(Round r)
					{
						//var prev = r.Previous?.VotesOfTry.FirstOrDefault(i => i.Generator == g);
						
						var v = Mcv.CreateVote();
	
						v.RoundId		= r.Id;
						v.Try			= r.Try;
						v.ParentHash	= r.Parent.Hash ?? r.Parent.Summarize();
						v.Created		= Mcv.Clock.Now;
						v.Time			= Time.Now(Mcv.Clock);
						v.Violators		= r.ProposeViolators().ToArray();
						v.MemberLeavers	= r.ProposeMemberLeavers(g).ToArray();
						v.NntBlocks		= Mcv.NtnBlocks.Select(i => i.State.Hash).ToArray();
	
						//v.FundJoiners	= Settings.ProposedFundJoiners.Where(i => !LastConfirmedRound.Funds.Contains(i)).ToArray();
						//v.FundLeavers	= Settings.ProposedFundLeavers.Where(i => LastConfirmedRound.Funds.Contains(i)).ToArray();
	
						Mcv.FillVote(v);
	
						return v;
					}
	
					var txs = IncomingTransactions.Where(i => i.Status == TransactionStatus.Accepted).ToArray();
	
					var must = r.Voters.Any(i => i.Address == g) && Mcv.Tail.Any(i => i.Id > Mcv.LastConfirmedRound.Id && i.Payloads.Any());
	
					if(txs.Any() || must)
					{
						var v = createvote(r);
						var deferred = new List<Transaction>();
						var pp = r.Parent.Previous;
						
						/// Compose txs list prioritizing higher fees but ensure continuous tx Nid sequence 
	
						bool tryplace(Transaction t, bool ba, bool isdeferred)	{ 	
																					if(v.Transactions.Length + 1 > pp.PerVoteTransactionsLimit)
																						return false;
	
																					if(v.Transactions.Sum(i => i.Operations.Length) + t.Operations.Length > (ba ? pp.PerVoteBandwidthAllocationLimit : pp.PerVoteOperationsLimit))
																						return false;
	
																					if(r.Id > t.Expiration)
																					{
																						t.Status = TransactionStatus.FailedOrNotFound;
																						IncomingTransactions.Remove(t);
																						return true;
																					}
	
																					var nearest = r.VotersRound.Members.NearestBy(m => m.Address, t.Signer).Address;
	
																					if(nearest != g)
																					{
																						if(!Mcv.Settings.Generators.Contains(nearest))
																						{
																							t.Status = TransactionStatus.FailedOrNotFound;
																							IncomingTransactions.Remove(t);
																						}
	
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
																						if(tryplace(next, ba, true) == false)
																							return false;
																					}
	
																					Flow.Log?.Report(this, "Transaction Placed", t.ToString());
	
																					return true;
																				}
	
						var stxs = txs.Select(i => new {t = i, a = Mcv.Accounts.Find(i.Signer, pp.Id)});
	
						foreach(var t in stxs.Where(i => i.a != null && i.a.BandwidthExpiration >= pp.ConsensusTime.Days && (i.a.BandwidthTodayTime == pp.ConsensusTime.Days && i.a.BandwidthTodayAvailable >= i.t.EnergyConsumed || /// Allocated bandwidth first
																															 i.a.Bandwidth >= i.t.EnergyConsumed)))
							if(false == tryplace(t.t, true, false))
								break;
	
						foreach(var t in stxs.Where(i => i.a != null && i.a.BandwidthExpiration < pp.ConsensusTime.Days).OrderByDescending(i => i.t.Bonus))		/// ... then fee-transactions
							if(false == tryplace(t.t, false, false))
								break;
	
						foreach(var t in stxs.Where(i => i.a == null))
							if(false == tryplace(t.t, false, false))
								break;
	
						if(v.Transactions.Any() || must)
						{
							///v.Sign(Vault.Find(g).Key);
							v.Generator = g;
							v.Signature	= UosApi.Request<byte[]>(new AuthorizeApc  {Net		= Net.Name,
																					Account	= g,
																					Session = GetSession(g),
																					Hash	= v.Hashify(),
																					Trust	= Trust.None}, Flow);						
	
							votes.Add(v);
						}
					}
	
	 				while(r.Previous != null && !r.Previous.Confirmed && r.Previous.Voters.Any(i => i.Address == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
	 				{
	 					r = r.Previous;
	 
	 					var v = createvote(r);
	 						
						v.Generator = g;
						v.Signature	= UosApi.Request<byte[]>(new AuthorizeApc  {Net		= Net.Name,
																				Account	= g,
																				Session = GetSession(g),
																				Hash	= v.Hashify(),
																				Trust	= Trust.None}, Flow);						
	 					votes.Add(v);
	 				}
	
					if(IncomingTransactions.Any(i => i.Status == TransactionStatus.Accepted) || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						MainWakeup.Set();
				}
			}
	
			if(votes.Any())
			{
				try
				{
					foreach(var v in votes.GroupBy(i => i.RoundId).OrderBy(i => i.Key))
					{
						var r = Mcv.FindRound(v.Key);
	
						foreach(var i in v)
						{
							Mcv.Add(i);
						}
					}
	
					for(int i = Mcv.LastConfirmedRound.Id + 1; i <= Mcv.LastNonEmptyRound.Id; i++) /// better to start from votes.Min(i => i.Id) or last excuted
					{
						var r = Mcv.GetRound(i);
						
						if(r.Hash == null)
						{
							r.ConsensusTime			= r.Previous.ConsensusTime;
							r.ConsensusECEnergyCost	= r.Previous.ConsensusECEnergyCost;
							///r.RentPerBytePerDay		= r.Previous.RentPerBytePerDay;
							r.Members				= r.Previous.Members;
							r.Funds					= r.Previous.Funds;
						}
	
						if(!r.Confirmed)
						{
							r.Execute(r.OrderedTransactions.Where(i => Mcv.Settings.Generators.Contains(i.Vote.Generator)));
						}
					}
				}
				catch(ConfirmationException ex)
				{
					ProcessConfirmationException(ex);
				}
	
				foreach(var i in votes)
				{
					Broadcast(i);
				}
													
				Flow.Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{i.Generator.Bytes.ToHexPrefix()}-{i.RoundId}")));
			}
	
			Statistics.Generating.End();
		}
		catch(ApiCallException ex)
		{
			Flow?.Log.ReportError(this, "Uos API call failure", ex);
			Thread.Sleep(1000);
		}
	}

	public abstract bool ValidateIncoming(Operation o);

	public bool ValidateIncoming(Transaction transaction, out Round round)
	{
		if( IncomingTransactions.Any(j => j.Signer == transaction.Signer && j.Nid == transaction.Nid) ||
			transaction.Expiration <= Mcv.LastConfirmedRound.Id ||
			!transaction.Valid(Mcv) ||
			transaction.Operations.Any(o => !ValidateIncoming(o)))
		{
			round = null;
			return false;
		}

		var a = Mcv.Accounts.Find(transaction.Signer, Mcv.LastConfirmedRound.Id);

		if(a == null)
		{	
			if(transaction.Sponsored)
				transaction.Nid = 0;
			else
			{	
				round = null;
				return false;
			}
		}
		else
			transaction.Nid	= a.LastTransactionNid + 1;

		round = Mcv.TryExecute(transaction);

		return transaction.Successful;

	}

	public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
	{
		foreach(var t in txs.Where(i => ValidateIncoming(i, out _)).OrderByDescending(i => i.Nid))
		{
			t.Status = TransactionStatus.Accepted;
			IncomingTransactions.Add(t);

			yield return t;
		}

		MainWakeup.Set();
	}

	public byte[] GetSession(AccountAddress signer)
	{
		var s = Node.Settings.Sessions.FirstOrDefault(i => i.Account == signer);

		if(s != null)
			return s.Session;

		var a = UosApi.Request<AccountSession>(new AuthenticateApc {Net = Net.Name, Account = signer}, Flow); 

		if(a == null)
			return null;

		Node.Settings.Sessions = [..Node.Settings.Sessions, new AccountSessionSettings {Account = a.Account, Session = a.Session}];

		Node.Settings.Save();

		return a.Session;
	}

	void Transacting()
	{
		Flow.Log?.Report(this, "Transacting started");

		while(Flow.Active)
		{
			bool nothing;

			lock(Lock)
				nothing = OutgoingTransactions.All(i => i.Status == TransactionStatus.Confirmed || i.Status == TransactionStatus.FailedOrNotFound);
			
			if(nothing)		
				WaitHandle.WaitAny([TransactingWakeup, Flow.Cancellation.WaitHandle]);

			var cr = Call(() => new MembersRequest(), Flow);

			if(!cr.Members.Any() || cr.Members.Any(i => !i.GraphPpcIPs.Any()))
				continue;

			var members = cr.Members;

			IPeer getrdi(AccountAddress account)
			{
				var m = members.NearestBy(i => i.Address, account);

				if(m.GraphPpcIPs.Contains(Settings.IP))
					return this;

				var p = GetPeer(m.GraphPpcIPs.Random());
				Connect(p, Flow);

				return p;
			}

			Statistics.Transacting.Begin();
			
			IEnumerable<IGrouping<AccountAddress, Transaction>> nones;

			lock(Lock)
				nones = OutgoingTransactions.Where(i => GetSession(i.Signer) != null).GroupBy(i => i.Signer).Where(g => !g.Any(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed) && g.Any(i => i.Status == TransactionStatus.None)).ToArray();

			foreach(var g in nones)
			{
				var m = members.NearestBy(i => i.Address, g.Key);

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
						t.Rdi		 = rdi;
						t.Bonus		 = 0;
						t.Nid		 = 0;
						t.Expiration = 0;
						t.Member	 = new(0, -1);
						t.Signature	 = UosApi.Request<byte[]>(new AuthorizeApc {Net		= Net.Name,
																				Account	= t.Signer,
																				Session = GetSession(t.Signer),
																				Hash	= t.Hashify(Net.Cryptography.ZeroHash),
																				Trust	= Trust.None}, t.Flow);

						var at = Call(rdi, new AllocateTransactionRequest {Transaction = t});
							
						if(nid == -1)
							nid = at.NextNid;
						else
							nid++;

						t.Member	 = at.Generator;
						t.Bonus		 = 0;
						t.Nid		 = nid;
						t.Expiration = at.LastConfirmedRid + Mcv.TransactionPlacingLifetime;
						t.Signature	 = UosApi.Request<byte[]>(new AuthorizeApc {Net		= Net.Name,
																				Account	= t.Signer,
																				Session = GetSession(t.Signer),
																				Hash	= t.Hashify(at.PoWBase),
																				Trust	= Trust.None}, t.Flow);

						txs.Add(t);

						t.Flow?.Log.Report(this, $"Created:  Nid={t.Nid}, Expiration={t.Expiration}, Operations={{{t.Operations.Length}}}, Signer={t.Signer}, Signature={t.Signature.ToHex()}");
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "Transaction allocation", ex);
						
						Thread.Sleep(1000);
						continue;
					}
					catch(EntityException ex)
					{
						Flow.Log?.ReportError(this, "Transaction allocation", ex);

						if(t.ActionOnResult != ActionOnResult.RetryUntilConfirmed)
						{
							lock(Lock)
								t.Status = TransactionStatus.FailedOrNotFound;
						} 

						continue;
					}
					catch(ApiCallException ex)
					{
						Flow.Log?.ReportError(this, "Transaction allocation", ex);

						//lock(Lock)
						//	t.Status = TransactionStatus.FailedOrNotFound;

						continue;
					}
				}

				if(txs.Any())
				{
					IEnumerable<byte[]> atxs = null;

					try
					{
						atxs = Call(rdi, new PlaceTransactionsRequest {Transactions = txs.ToArray()}).Accepted;
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "PlaceTransactionsRequest", ex);
						Thread.Sleep(1000);
						continue;
					}

					lock(Lock)
						foreach(var t in txs)
						{ 
							if(atxs.Any(s => s.SequenceEqual(t.Signature)))
							{
								t.Status = TransactionStatus.Accepted;
								t.Flow.Log?.Report(this, $"{TransactionStatus.Accepted}: Member={{{m}}}");
							}
							else
							{
								t.Flow.Log?.Report(this, $"Rejected: Member={{{m}}}");

								if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
									t.Status = TransactionStatus.None;
								else
									t.Status = TransactionStatus.FailedOrNotFound;
							}
						}
				}
			}

			Transaction[] accepted;
			
			lock(Lock)
				accepted = OutgoingTransactions.Where(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed).ToArray();

			Flow.Log?.Report(this, $"accepted : {accepted.Count()}" );

			if(accepted.Any())
			{
				foreach(var g in accepted.GroupBy(i => i.Rdi))
				{
					TransactionStatusResponse ts;

					try
					{
						ts = Call(g.Key, new TransactionStatusRequest {Transactions = g.Select(i => new TransactionsAddress {Signer = i.Signer, Nid = i.Nid}).ToArray()});
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "TransactionStatusRequest", ex);
						Thread.Sleep(1000);
						continue;
					}

					lock(Lock)
						foreach(var i in ts.Transactions)
						{
							var t = accepted.First(d => d.Signer == i.Account && d.Nid == i.Nid);
																	
							if(t.Status != i.Status)
							{
								t.Flow.Log?.Report(this, $"{i.Status}");

								t.Status = i.Status;

								if(t.Status == TransactionStatus.FailedOrNotFound)
								{
									if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
										t.Status = TransactionStatus.None;
								}
								else if(t.Status == TransactionStatus.Confirmed)
								{
									if(t.ActionOnResult == ActionOnResult.ExpectFailure)
										Debugger.Break();
									else
										t.Id = i.Id; 
								}
							}
						}
				}
			}
			
			Statistics.Transacting.End();
		}
	}

	void Transact(Transaction t)
	{
		if(OutgoingTransactions.Count > Mcv.TransactionQueueLimit)
		{
			OutgoingTransactions.RemoveAll(i => DateTime.UtcNow - i.Inquired > TimeSpan.FromSeconds(Node.Settings.TransactionNoInquireKeepPeriod));
		}
		
		if(OutgoingTransactions.Count <= Mcv.TransactionQueueLimit)
		{
			if(GetSession(t.Signer) == null)
				throw new NodeException(NodeError.Locked);

			if(TransactingThread == null)
			{
				TransactingThread = Node.CreateThread(Transacting);
				TransactingThread.Name = $"{Node.Name} Transacting";
				TransactingThread.Start();
			}

			OutgoingTransactions.Add(t);
			TransactingWakeup.Set();
		} 
		else
		{
			Flow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			throw new NodeException(NodeError.LimitExceeded);
		}
	}

 	public Transaction Transact(IEnumerable<Operation> operations, AccountAddress signer, byte[] tag, bool sponsored, ActionOnResult aor, Flow flow)
 	{
		if(operations.Count() > Net.ExecutionCyclesPerTransactionLimit)
			throw new NodeException(NodeError.LimitExceeded);

		if(!operations.Any() || operations.Any(i => !i.IsValid(Net)))
			throw new NodeException(NodeError.Invalid);

		var t = new Transaction();
		t.Tag				= tag ?? Guid.NewGuid().ToByteArray();
		t.Net				= Net;
		t.Signer			= signer;
		t.Sponsored			= sponsored;
		t.Flow				= flow;
		t.Inquired			= DateTime.UtcNow;
 		t.ActionOnResult	= aor;
		
		foreach(var i in operations)
		{
			t.AddOperation(i);
		}

 		lock(Lock)
		{	
		 	Transact(t);
		}

		return t;
 	}

	public R Call<R>(Func<Ppc<R>> call, Flow workflow, IEnumerable<Peer> exclusions = null)  where R : PeerResponse
	{
		return Call((Func<FuncPeerRequest>)call, workflow, exclusions) as R;
	}

	public PeerResponse Call(Func<FuncPeerRequest> call, Flow workflow, IEnumerable<Peer> exclusions = null)
	{
		HashSet<Peer> tried;
		
		void init()
		{
			tried = exclusions != null ? [..exclusions] : [];
		}

		init();

		Peer p;

		while(workflow.Active)
		{
			Thread.Sleep(1);

			lock(Lock)
			{
				if(Synchronization == Synchronization.Synchronized)
				{
					var c = call();
					c.Peering = this;

					return Send(c);
				}

				p = ChooseBestPeer((long)Role.Graph, tried);

				if(p == null)
				{
					init();
					continue;
				}
			}

			tried.Add(p);

			try
			{
				Connect(p, workflow);

				var c = call();
				c.Peering = this;

				return p.Send(c);
			}
			catch(NodeException)
			{
			}
			catch(ContinueException)
			{
			}
		}

		throw new OperationCanceledException();
	}

	public void Broadcast(Vote vote, Peer skip = null)
	{
		foreach(var i in Graphs.Where(i => i != skip))
		{
			try
			{
				var v = new VoteRequest {Vote = vote};
				v.Peering = this;
				i.Post(v);
			}
			catch(NodeException)
			{
			}
		}
	}

	public static void CompareGraphs(string destination)
	{
		var mcvs = All.OfType<McvTcpPeering>().GroupBy(i => i.Net.Address);

		foreach(var i in mcvs)
		{
			var d = Path.Join(destination, "!CompareGraphs- " + i.Key);

			CompareBase(i.Where(i => i.Mcv != null).ToArray(), d);
		}
	}

	public static void CompareBase(McvTcpPeering[] all, string destibation)
	{
		//Suns.GroupBy(s => s.Mcv.Accounts.SuperClusters.SelectMany(i => i.Value), Bytes.EqualityComparer);
		Directory.CreateDirectory(destibation);

		var jo = new JsonSerializerOptions(ApiClient.CreateOptions());
		jo.WriteIndented = true;

		foreach(var i in all)
			Monitor.Enter(i.Mcv.Lock);

		void compare(int table)
		{
// 				foreach(var m in  All.OfType<McvTcpPeering>().Where(i => i.Net.Address == mcv.Net.Address && i.Mcv != null))
// 				{
// 					var w = new StringWriter();
// 
// 					foreach(var c in m.Mcv.Tables[table].Clusters)
// 					{
// 						w.WriteLine(c.ToString());
// 
// 						foreach(var b in c.Buckets)
// 						{
// 							w.WriteLine("	" + b.ToString());
// 
// 							foreach(var e in b.Entries)
// 							{
// 								w.WriteLine("		" + e.ToString());
// 							}
// 						}
// 
// 					}
// 
// 					File.WriteAllText(Path.Join(destibation, $"{mcv.Tables[table].GetType().Name}-{m.Node.Name}"), w.ToString());
// 				}

			

			var cs = all.Select(i => new {m = i, c = i.Mcv.Tables[table].Clusters.OrderBy(i => i.Id).ToArray().AsEnumerable().GetEnumerator()})
						.ToArray();

			while(true)
			{
				var x = new bool[cs.Length];

				for(int i=0; i<cs.Length; i++)
					x[i] = cs[i].c.MoveNext();

				if(x.All(i => !i))
					break;
				else if(!x.All(i => i))
					Debugger.Break();

				var es = cs.Select(i => new {i.m, e = i.c.Current.Buckets.OrderBy(i => i.Id).SelectMany(i => i.Entries.OrderBy(i => i.Key)).ToArray().GetEnumerator()}).ToArray();

				while(true)
				{
					var y = new bool[es.Length];

					for(int i=0; i<es.Length; i++)
						y[i] = es[i].e.MoveNext();

					if(y.All(i => !i))
						break;
					else if(!y.All(i => i))
						Debugger.Break();

					var jes = es.Select(i => new {i.m, j = JsonSerializer.Serialize(i.e.Current, jo)}).GroupBy(i => i.j);

					if(jes.Count() > 1)
					{
						foreach(var i in jes)
						{
							File.WriteAllText(Path.Join(destibation, string.Join(',', i.Select(i => i.m.Node.Name))), i.Key);
						}
						
						Debugger.Break();
					}
				}
			}
		}

		foreach(var t in all[0].Mcv.Tables.Where(i => !i.IsIndex))
			compare(t.Id);

		foreach(var i in all)
			Monitor.Exit(i.Mcv.Lock);
	}

}