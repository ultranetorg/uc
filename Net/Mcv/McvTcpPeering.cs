using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using RocksDbSharp;

namespace Uccs.Net;

public enum McvPpcClass : uint
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
	public McvNode							Node;
	public McvNet							Net => Node.Net;
	public Mcv								Mcv => Node.Mcv; 
	public VaultApiClient					VaultApi; 

	public IEnumerable<HomoPeer>			Graphs => Connections.Where(i => i.Permanent && i.Roles.IsSet(Role.Graph));

	public bool								MinimalPeersReached;
	AutoResetEvent							TransactingWakeup = new AutoResetEvent(true);
	Thread									TransactingThread;
	public List<Transaction>				OutgoingTransactions = new();
	public List<Transaction>				IncomingTransactions = new();
	public List<Transaction>				ConfirmedTransactions = new();

	public Synchronization					Synchronization { get; protected set; } = Synchronization.None;
	Thread									SynchronizingThread;
	public Dictionary<int, List<Vote>>		SyncTail = new();

	List<AccountAddress>					CandidacyDeclarations = [];
	
	public static List<McvTcpPeering>		All = new();

	public McvTcpPeering(McvNode node, PeeringSettings settings, long roles, VaultApiClient vaultapi, Flow flow) : base(node, node.Name, node.Net, node.Database, settings, roles, flow)
	{
		Node = node;
		VaultApi = vaultapi;

		Constructor.Register<PeerRequest> (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Result> (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppr".Length));

		Constructor.Register(() => new Transaction {Net = Net});
		Constructor.Register(() => new Vote(Mcv));

		All.Add(this);
	}

	//public override object Constract(Type t, byte b)
	//{
	//	if(t == typeof(Transaction))	return new Transaction {Net = Net}; 
 	//	if(t == typeof(Vote))			return new Vote(Mcv);
	//
	//	return base.Constract(t, b);
	//}
	//
	//public override byte TypeToCode(Type i)
	//{
	//	return base.TypeToCode(i);
	//}

	public override void Run()
	{
		base.Run();

		if(Mcv != null)
		{
			Mcv.VoteAdded += b => MainWakeup.Set();

			Mcv.ConsensusFailed += r =>	{
											foreach(var i in r.OrderedTransactions.Where(j => Mcv.Settings.Generators.Contains(j.Vote.Generator)))
											{
												i.Vote = null;
												i.Status = TransactionStatus.Accepted;
											}
										};

			Mcv.Confirmed += r =>	{
										if(Synchronization == Synchronization.Synchronized)
										{
											bool old(Transaction t) => t.Vote?.Round?.Id == r.Id || t.Expiration <= r.Id;
	
											ConfirmedTransactions.AddRange(r.ConsensusTransactions.Where(j => Mcv.Settings.Generators.Contains(j.Vote.Generator)));
											IncomingTransactions.RemoveAll(old);
											ConfirmedTransactions.RemoveAll(t => t.Expiration < r.Id - Net.CommitLength);
										}
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
					c.Send(new SharePeersPpc{Broadcast = true, 
											 Peers = [new HomoPeer(EP) {Roles = Roles}]});
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
		if(Settings.EP != null && Settings.EP.Equals(Net.Father0IP) && Mcv.Settings.Generators.Contains(Net.Father0) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound)
		{
			Synchronization = Synchronization.Synchronized;
			return;
		}

		if(Synchronization != Synchronization.Downloading)
		{
			SyncTail.Clear();
			IncomingTransactions.Clear();

			Flow.Log?.Report(this, $"Synchronization Started");

			SynchronizingThread = Node.CreateThread(Synchronizing);
			SynchronizingThread.Name = $"{Node.Name} Synchronizing";
			SynchronizingThread.Start();
	
			Synchronization = Synchronization.Downloading;
		}
	}

	void Synchronizing()
	{
		var used = new HashSet<HomoPeer>();

		StampPpr stamp = null;
		HomoPeer peer = null;

		while(Flow.Active)
		{
			try
			{
				WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 500);

				peer = Connect(Mcv.Settings.Roles, used, Flow);

				if(Mcv.Settings.Chain == null)
				{
					stamp = Call(peer, new StampPpc());
	
					void download(TableBase t)	{
													var ts = Call(peer, new TableStampPpc {Table = t.Id, 
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
																		var d = Call(peer, new DownloadTablePpc {Table		= t.Id,
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
		
						var s = Call(peer, new StampPpc());
	
						lock(Mcv.Lock)
						{
							Mcv.GraphState = stamp.GraphState;
							Mcv.LastConfirmedRound = r;
							Mcv.LastCommitedRound = r;
			
							Mcv.Hashify();
			
							if(s.GraphHash.SequenceEqual(Mcv.GraphHash))
	 						{	
								Mcv.OldRounds[r.Id] = r;

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
	
					var rp = Call(peer, new DownloadRoundsPpc {From = from, To = to});

					lock(Mcv.Lock)
					{
						var rounds = rp.Read(Mcv);
													
						foreach(var r in rounds)
						{
							//var r = rounds.FirstOrDefault(i => i.Id == rid);
							//
							//if(r == null)
							//	break;
							
							Flow.Log?.Report(this, $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.EP}");
								
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

							Mcv.Save(r);
							
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
		{	
			Flow.Log.ReportWarning(this, $"Vote rejected !Valid : {v}");
			return false;
		}

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
			if(v.RoundId <= Mcv.LastConfirmedRound.Id || Mcv.LastConfirmedRound.Id + Mcv.P*2 < v.RoundId)
			{	
				//Flow.Log.ReportWarning(this, $"Vote rejected v.RoundId={v.RoundId} LastConfirmedRound={Mcv.LastConfirmedRound.Id}");
				return false;
			}
			
			var r = Mcv.GetRound(v.RoundId);

			if(r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
				return false;
								
			if(r.VotersRound.Members.Any() && !r.VotersRound.Members.Any(i => i.Address == v.Generator))
				return false;

			if(r.Forkers.Contains(v.Generator))
				return false;

			var e = r.VotesOfTry.FirstOrDefault(i => i.Generator == v.Generator);
			
			if(e != null) /// FORK
			{
				//Flow.Log.ReportWarning(this, $"Vote rejected FORK : {v}");

				r.Votes.Remove(e);
				r.Forkers.Add(e.Generator);

				return false;
			}

			v.Restore();
							
			if(r.VotersRound.Confirmed)
			{
				if(v.Transactions.Length > r.VotersRound.PerVoteTransactionsLimit)
				{	
					//Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Length > r.Parent.PerVoteTransactionsLimit : {v}");
					
					return false;
				}

				if(v.Transactions.Sum(i => i.Operations.Length) > r.VotersRound.PerVoteOperationsLimit)
				{	
					//Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Sum(i => i.Operations.Length) > r.Parent.PerVoteOperationsLimit : {v}");
					
					return false;
				}

				if(v.Transactions.Any(t => r.VotersRound.Members.NearestBy(m => m.Address, t.Signer).Address != v.Generator))
				{	
					//Flow.Log.ReportWarning(this, $"{nameof(Vote)} rejected NOT NEAREST : {v}");

					return false;
				}
			}

			//if(v.Transactions.Any(i => !i.Valid(Mcv))) /// do it only after adding to the chainbase
			//{
			//	Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Any(i => !i.Valid(Mcv)): {v}");
			//	return false;
			//}

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
	
				var m = Mcv.NextVotingRound.VotersRound.Members.Find(i => i.Address == g);
	
				if(m == null)
				{
					if(Mcv.LastConfirmedRound.Candidates.Any(i => i.Address == g))
						continue;

					try
					{
						var fcb = !CandidacyDeclarations.Contains(g) && GetSession(g) == null;
					}
					catch(VaultException ex)
					{
						
					}
	
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
	
					var r = Mcv.NextVotingRound;
	
					//if(r.Id < m.CastingSince)
					//	continue;
	
					if(!r.ConsensusFailed && r.VotesOfTry.Any(i => i.Generator == g))
						continue;

					if(r.ConsensusFailed)
					{
						var s = r.Parent.Summarize();

						if(r.Parent.Hash.SequenceEqual(s))
							continue;
					}
	
					Vote createvote(Round r)
					{
						//var prev = r.Previous?.VotesOfTry.FirstOrDefault(i => i.Generator == g);
						
						var v = Mcv.CreateVote();
	
						if(!r.ConsensusFailed)
						{
							v.Try			= r.Try;
							v.ParentHash	= r.Parent.Hash ?? r.Parent.Summarize();
						} 
						else
						{
							r.Try++;
							r.Update();
							r.FirstArrivalTime	= DateTime.MaxValue;

							v.Try			= r.Try;
							v.ParentHash	= r.Parent.Summarize();
						}

						v.RoundId		= r.Id;
						v.Created		= Mcv.Clock.Now;
						v.Time			= Time.Now(Mcv.Clock);
						v.Violators		= r.ProposeViolators().ToArray();
						v.MemberLeavers	= r.ProposeMemberLeavers(g).ToArray();
						v.NntBlocks		= Mcv.NnBlocks.Select(i => i.State.Hash).ToArray();
	
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
																						IncomingTransactions.Remove(t);
																						return true;
																					}
	
																					var nearest = r.VotersRound.Members.NearestBy(m => m.Address, t.Signer).Address;
	
																					if(nearest != g)
																					{
																						if(!Mcv.Settings.Generators.Contains(nearest))
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
	
						foreach(var t in stxs.Where(i => i.a != null && i.a.BandwidthExpiration < pp.ConsensusTime.Days).OrderByDescending(i => i.t.Bonus))		/// ... then paid transactions
							if(false == tryplace(t.t, false, false))
								break;
	
						foreach(var t in stxs.Where(i => i.a == null))
							if(false == tryplace(t.t, false, false))
								break;
	
						if(v.Transactions.Any() || must)
						{
							///v.Sign(Vault.Find(g).Key);
							v.Generator = g;
							v.Signature	= VaultApi.Call<byte[]>(new AuthorizeApc
																{
																	Cryptography= Net.Cryptography.Type,
																	Application	= Name,
																	Net			= Net.Name,
																	Account		= g,
																	Session		= GetSession(g),
																	Hash		= v.Hashify()
																}, Flow);						
							votes.Add(v);
						}
					}
	
	 				while(r.Previous != null && !r.Previous.Confirmed && r.Previous.VotersRound != null && r.Previous.Voters.Any(i => i.Address == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
	 				{
	 					r = r.Previous;
	 
	 					var v = createvote(r);
	 						
						v.Generator = g;
						v.Signature	= VaultApi.Call<byte[]>(new AuthorizeApc
															{
																Cryptography= Net.Cryptography.Type,
																Application	= Name,
																Net			= Net.Name,
																Account		= g,
																Session		= GetSession(g),
																Hash		= v.Hashify()
															}, Flow);						
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
							///r.RentPerBytePerDay	= r.Previous.RentPerBytePerDay;
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
	
				foreach(var v in votes)
				{
					Flow.Log?.Report(this, $"{nameof(Vote)} generated : {v}");
					
					Broadcast(v);
				}
													
			}
	
			Statistics.Generating.End();
		}
		catch(ApiCallException ex)
		{
			Flow?.Log.ReportError(this, "Uos API call failure", ex);
			Thread.Sleep(NodeGlobals.TimeoutOnError);
		}
	}

	public abstract bool ValidateIncoming(Operation o);

	public bool ValidateIncoming(Transaction transaction, bool preserve, out Round round)
	{
		if( IncomingTransactions.Any(j => j.Signer == transaction.Signer && j.Nid == transaction.Nid) ||
			transaction.Operations.Any(o => !ValidateIncoming(o)))
		{
			round = null;
			return false;
		}

		round = Mcv.Examine(transaction, preserve);

		return round != null && transaction.Successful;

	}

	public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
	{
		foreach(var t in txs.Where(i => ValidateIncoming(i, true, out _)).OrderByDescending(i => i.Nid))
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

		var a = VaultApi.Call<AuthenticationResult>(new AuthenticateApc {Application = Node.Name, Net = Net.Name, Account = signer}, Flow); 

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
				nothing = OutgoingTransactions.All(i => i.Status == TransactionStatus.Confirmed);
			
			if(nothing)		
				WaitHandle.WaitAny([TransactingWakeup, Flow.Cancellation.WaitHandle]);

			var cr = Call(new MembersPpc(), Flow);

			if(!cr.Members.Any() || cr.Members.Any(i => !i.GraphPpcIPs.Any()))
				continue;

			var members = cr.Members;

			IHomoPeer getrdi(AccountAddress account)
			{
				var m = members.NearestBy(i => i.Address, account);

				if(m.GraphPpcIPs.Contains(Settings.EP))
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

				IHomoPeer ppi; 

				try
				{
					ppi = getrdi(g.Key);
				}
				catch(NodeException)
				{
					Thread.Sleep(NodeGlobals.TimeoutOnError);
					continue;
				}

				int nid = -1;
				var txs = new List<Transaction>();

				foreach(var t in g.Where(i => i.Status == TransactionStatus.None))
				{
					try
					{
						t.Ppi		 = ppi;
						t.Bonus		 = 0;
						t.Nid		 = 0;
						t.Expiration = 0;
						t.Member	 = new(0, -1);
						t.Signature	 = VaultApi.Call<byte[]>(new AuthorizeApc
															 {
																Cryptography= Net.Cryptography.Type,
																Application	= Name,
																Net			= Net.Name,
																Account		= t.Signer,
																Session		= GetSession(t.Signer),
																Hash		= t.Hashify(),
															 }, t.Flow);
						if(t.Signature == null)
						{	
							t.Flow?.Log.ReportError(this, $"Failed to sign");
							break;
						}

						var at = Call(ppi, new AllocateTransactionPpc {Transaction = t});
							
						if(nid == -1)
							nid = at.NextNid;
						else
							nid++;

						t.Member	 = at.Generator;
						t.Bonus		 = 0;
						t.Nid		 = nid;
						t.Expiration = at.LastConfirmedRid + Mcv.TransactionPlacingLifetime;
						t.Signature  = VaultApi.Call<byte[]>(new AuthorizeApc
															 {
																Cryptography= Net.Cryptography.Type,
																Application	= Name,
																Net			= Net.Name,
																Account		= t.Signer,
																Session		= GetSession(t.Signer),
																Hash		= t.Hashify(),
															 }, t.Flow);

						txs.Add(t);

						t.Flow.Log?.Report(this, $"Created:  Nid={t.Nid}, Expiration={t.Expiration}, Operations={{{t.Operations.Length}}}, Signer={t.Signer}, Hash={t.Hashify()}, Signature={t.Signature.ToHex()}");
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "Transaction allocation", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
					catch(EntityException ex)
					{
						if(t.ActionOnResult != ActionOnResult.RetryUntilConfirmed)
						{
							lock(Lock)
								//t.Status = TransactionStatus.FailedOrNotFound;
								OutgoingTransactions.Remove(t);
						} 

						Flow.Log?.ReportError(this, "Transaction allocation", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
					catch(ApiCallException ex)
					{
						Flow.Log?.ReportError(this, "Transaction allocation", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
				}

				if(txs.Any())
				{
					IEnumerable<byte[]> atxs = null;

					try
					{
						atxs = Call(ppi, new PlaceTransactionsPpc {Transactions = [..txs]}).Accepted;
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "PlaceTransactionsRequest", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
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
								t.Flow.Log?.Report(this, $"Transaction Rejected by {m}");

								if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
									t.Status = TransactionStatus.None;
								else
									//t.Status = TransactionStatus.FailedOrNotFound;
									OutgoingTransactions.Remove(t);
							}
						}
				}
			}

			Transaction[] accepted;
			
			lock(Lock)
				accepted = OutgoingTransactions.Where(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed).ToArray();

			///Flow.Log?.Report(this, $"accepted : {accepted.Count()}" );

			if(accepted.Any())
			{
				foreach(var g in accepted.GroupBy(i => i.Ppi))
				{
					TransactionStatusPpr ts;

					try
					{
						ts = Call(g.Key, new TransactionStatusPpc {Signatures = [..g.Select(i => i.Signature)]});
					}
					catch(NodeException ex)
					{
						Flow.Log?.ReportError(this, "TransactionStatusRequest", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}

					lock(Lock)
						foreach(var i in ts.Transactions)
						{
							var t = accepted.First(a => a.Signature.SequenceEqual(i.Signature));
																	
							if(t.Status != i.Status)
							{
								t.Flow.Log?.Report(this, $"{i.Status}");

								t.Status = i.Status;

								if(t.Status == TransactionStatus.FailedOrNotFound)
								{
									if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
										t.Status = TransactionStatus.None;
									else
										OutgoingTransactions.Remove(t);
								}
								else if(t.Status == TransactionStatus.Confirmed)
								{
									if(t.ActionOnResult == ActionOnResult.ExpectFailure)
										Debugger.Break();
									//else
									//	t.Id = i.Id; 
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
				throw new NodeException(NodeError.NoSession);

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

		foreach(var i in operations)
			i.PreTransact(Node, sponsored, flow);

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

	public R Call<R>(Ppc<R> call, Flow workflow, IEnumerable<HomoPeer> exclusions = null)  where R : Result
	{
		return Call((PeerRequest)call, workflow, exclusions) as R;
	}

	public Result Call(PeerRequest call, Flow workflow, IEnumerable<HomoPeer> exclusions = null)
	{
		HashSet<HomoPeer> tried;
		
		void init()
		{
			tried = exclusions != null ? [..exclusions] : [];
		}

		init();

		HomoPeer p;

		while(workflow.Active)
		{
			Thread.Sleep(1);

			lock(Lock)
			{
				if(Synchronization == Synchronization.Synchronized)
				{
					var c = call;
					c.Peering = this;

					return Call(c);
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

				var c = call;
				c.Peering = this;

				return p.Call(c);
			}
			catch(NodeException ex)
			{
			}
			catch(ContinueException)
			{
			}
		}

		throw new OperationCanceledException();
	}

	public void Broadcast(Vote vote, HomoPeer skip = null)
	{
		foreach(var i in Graphs.Where(i => i != skip))
		{
			try
			{
				var v = new VotePpc {Vote = vote};
				v.Peering = this;
				i.Send(v);
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