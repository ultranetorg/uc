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
	Vote, Time, Members, Funds, Pretransacting, PlaceTransactions, TransactionStatus, User, 
	Stamp, TableStamp, DownloadCluster, DownloadBucket, DownloadRounds,
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

public abstract class McvPeering : HomoTcpPeering
{
	public McvNode							Node;
	public McvNet							Net => Node.Net;
	public Mcv								Mcv => Node.Mcv; 
	public VaultApiClient					VaultApi; 

	public IEnumerable<HomoPeer>			Graphs => Connections.Where(i => i.Permanent && i.Roles.IsSet(Role.Graph));

	public bool								MinimalPeersReached;
	AutoResetEvent							TransactingWakeup = new AutoResetEvent(true);
	Thread									TransactingThread;
	public List<Transaction>				OutgoingTransactions = [];
	public List<Transaction>				CandidateTransactions = [];
	public List<Transaction>				ConfirmedTransactions = [];
	List<AccountAddress>					CandidacyDeclarations = [];

	public Synchronization					Synchronization { get; protected set; } = Synchronization.None;
	Thread									SynchronizingThread;
	public string							SynchronizationInfo;
	public Dictionary<int, List<Vote>>		SynchronizationTail = [];

	public static List<McvPeering>			All = [];

	public McvPeering(McvNode node, PeeringSettings settings, long roles, VaultApiClient vaultapi, Flow flow) : base(node, node.Name, node.Net, node.Database, settings, roles, flow)
	{
		Node = node;
		VaultApi = vaultapi;

		Constructor.Register<PeerRequest> (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Result> (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppr".Length));

		Constructor.Register(() => new Transaction {Net = Net});
		Constructor.Register(() => new Vote(Mcv));

		if(Mcv != null)
		{
			Mcv.VoteAdded += b => MainWakeup.Set();

			Mcv.ConsensusFailed += r =>	{
											foreach(var i in r.OrderedTransactions.Where(j => Mcv.Settings.Generators.Any(g => g.Signer == j.Vote.Generator)))
											{
												i.Vote = null;
												i.Status = TransactionStatus.Accepted;
											}
										};

			Mcv.Confirmed += r =>	{
										if(Synchronization == Synchronization.Synchronized)
										{
											foreach(var i in r.ConsensusTransactions.Where(j => Mcv.Settings.Generators.Any(g => g.Signer == j.Vote.Generator)))
											{
												i.Inquired = DateTime.UtcNow;
												ConfirmedTransactions.Add(i);
											}

											ConfirmedTransactions.RemoveAll(i => DateTime.UtcNow - i.Inquired > TimeSpan.FromSeconds(Node.Settings.TransactionNoInquireKeepPeriod));
											CandidateTransactions.RemoveAll(t => t.Vote?.Round?.Id == r.Id || t.Expiration <= r.Id);
										}
									};
		}

		All.Add(this);
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

	protected override void ProcessMain()
	{
		base.ProcessMain();

		if(Mcv != null)
		{
			var needed = Settings.PermanentMin - Graphs.Count();
	
			foreach(var p in Peers	.Where(p =>	p.Status == ConnectionStatus.Disconnected && DateTime.UtcNow - p.LastTry > TimeSpan.FromSeconds(5))
									.OrderBy(i => i.Retries)
									.ThenByDescending(i => i.Roles.IsSet(Role.Graph))
									.ThenBy(i => Settings.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
									.Take(needed))
			{
				OutboundConnect(p, true);
			}

			if(!MinimalPeersReached && Connections.Count(i => i.Permanent) >= Settings.PermanentMin)
			{
				MinimalPeersReached = true;
				Flow.Log?.Report(this, $"PermanentMin reached");

				if(IsListener)
				{
					foreach(var c in Connections)
						c.Send(new SharePeersPpc{Broadcast = true, 
												 Peers = [new HomoPeer(EP) {Roles = Roles}]});
				}

				lock(Mcv.Lock)
					Synchronize();
			}

			if(MinimalPeersReached)
			{
				lock(Mcv.Lock)
					Generate();
			}
		}
	}

	public void Synchronize()
	{
		if(Settings.EP != null && Settings.EP.Equals(Net.Father0IP) && Mcv.Settings.Generators.Any(g => g.Signer == Net.Father0Signer) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound)
		{
			Synchronization = Synchronization.Synchronized;
			return;
		}

		if(Synchronization != Synchronization.Downloading)
		{
			SynchronizationTail.Clear();
			CandidateTransactions.Clear();

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
			WaitHandle.WaitAny([Flow.Cancellation.WaitHandle], 100);

			try
			{
				peer = Connect(Mcv.Settings.Roles, used, Flow);

				if(Mcv.Settings.Chain == null)
				{
					void download(TableBase t)	{
													using var w = new WriteBatch();
												
													if(t.Clusters.Count() == 0)
													{
														foreach(var i in stamp.Tables[t.Id].Clusters)
														{
															var d = Call(peer,  new DownloadClusterPpc
																				{
																					Table	= t.Id,
																					Cluster	= i.Id, 
																					Hash	= i.Hash
																				});
															lock(Mcv.Lock)	
															{	
																var c = t.GetCluster(i.Id);
																
																c.Import(w, d.Main);
																c.Commit(w);

																if(!c.Hash.SequenceEqual(i.Hash))
																	throw new SynchronizationException("Same cluster data - different hashes");
															}
														}
													} 
													else
													{
														var ts = Call(peer, new TableStampPpc
																			{
																				Table = t.Id, 
																				Clusters = stamp.Tables[t.Id].Clusters.Where(i => !t.FindCluster(i.Id)?.Hash?.SequenceEqual(i.Hash) ?? true) 
																																	.Select(i => i.Id)
																																	.ToArray()
																			});
	
														foreach(var i in ts.Clusters)
														{
															var c = t.GetCluster(i.Id);
	
															foreach(var j in i.Buckets)
															{
																var b = c.GetBucket(j.Id);
					
																if(b.Hash == null || !b.Hash.SequenceEqual(j.Hash))
																{
																	var d = Call(peer,	new DownloadBucketPpc
																						{
																							Table	= t.Id,
																							Bucket	= j.Id, 
																							Hash	= j.Hash
																						});
																	lock(Mcv.Lock)	
																		b.Import(w, d.Main);
						
																	if(!b.Hash.SequenceEqual(j.Hash))
																		throw new SynchronizationException("Same bucket data - different hashes");
					
																	Flow.Log?.Report(this, $"Bucket downloaded {t.GetType().Name}, {b.Id}");
																}
															}
	
															c.Commit(w);
														}
								
													}

													Mcv.Rocks.Write(w);
												}

				resync:
					SynchronizationInfo = null;

					stamp = Call(peer, new StampPpc());
	
					foreach(var i in SynchronizationTail.Keys)
						if(i <= stamp.LastCommitedRound)
							SynchronizationTail.Remove(i);


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
			
						Mcv.OldRounds[r.Id] = r;

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

							for(int i = Mcv.LastConfirmedRound.Id + 1; i <= Mcv.LastConfirmedRound.Id + Mcv.P; i++)
							{
								if(!SynchronizationTail.TryGetValue(i, out var vs))
									goto resync;

								foreach(var v in vs.Where(i => Mcv.LastConfirmedRound.Members.Any(m => m.Since <= i.RoundId && m.Address == i.Generator)).GroupBy(i => i.Try).MaxBy(i => i.Key))
								{	
									v.Restore();
									Mcv.AddOnly(v);
								}
							}

							r = Mcv.GetRound(Mcv.LastConfirmedRound.Id + 1);
							var a = SynchronizationTail.Where(i => i.Key >= Mcv.LastConfirmedRound.Id + 1 + Mcv.P).OrderBy(i => i.Key).ToArray();
	
							foreach(var i in a)
							{
								foreach(var v in i.Value.OrderBy(i => i.Try))
									ProcessIncoming(v, true);
		
							}

							if(r.Confirmed)
							{
								Synchronization = Synchronization.Synchronized;
								SynchronizationInfo = null;
								SynchronizingThread = null;
								SynchronizationTail.Clear();
								
								MainWakeup.Set();
			
								Flow.Log?.Report(this, $"Synchronization Finished");
								return;
							}

							break;
						}
						else
						{
							#if DEBUG
								//CompareBase([this, All.First(i => i.Node.Name == peer.Name)], "a:\\1111111111111");
								lock(Mcv.Lock)
									Mcv.Dump();
									
								lock(All.First(i => i.Node.Name == peer.Name).Mcv.Lock)
									All.First(i => i.Node.Name == peer.Name).Mcv.Dump();
								
							///	Debugger.Break();
							#endif
						}
					}
				}
				else
				{
					int from = -1;
					int to = -1;

					lock(Mcv.Lock)
						from = Mcv.LastConfirmedRound.Id + 1;
		
					to = from + Mcv.P;
		
					var rp = Call(peer, new DownloadRoundsPpc {From = from, To = to});
	
					lock(Mcv.Lock)
					{
						var rounds = rp.Read(Mcv);
														
						foreach(var r in rounds)
						{
				
							Flow.Log?.Report(this, $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.EP}");
									
							if(Mcv.LastConfirmedRound.Id + 1 != r.Id)
								throw new SynchronizationException();
	
							if(Enumerable.Range(r.Id, Mcv.P + 1).All(SynchronizationTail.ContainsKey) && (Mcv.Settings.Chain != null || Mcv.FindRound(r.VotersId) != null))
							{
								var p =	SynchronizationTail[r.Id];
								var c =	SynchronizationTail[r.Id + Mcv.P];
	
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
									foreach(var i in SynchronizationTail.OrderBy(i => i.Key).Where(i => i.Key > r.Id))
									{
										foreach(var v in i.Value)
											ProcessIncoming(v, true);
									}
										
									Synchronization = Synchronization.Synchronized;
									SynchronizationInfo = null;
									SynchronizingThread = null;
									SynchronizationTail.Clear();
						
									MainWakeup.Set();
	
									Flow.Log?.Report(this, $"Synchronization Finished");
									return;
								}
							}
	
							var x = Mcv.FindRound(r.Id);
	
							if(x != null)
							{
								Mcv.Tail.Remove(x);
								Mcv.Tail.Insert(0, r);
								Mcv.Tail.Sort((a, b) => -a.Id.CompareTo(b.Id));
							}
							else
								Mcv.Tail.Insert(0, r);
	
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
									
									Debugger.Break();
								#endif
																	
								throw new SynchronizationException("!r.Hash.SequenceEqual(h)");
							}
	
							r.Confirmed = false;
							r.Confirm();
							Mcv.Save(r);
						}
					}

					Thread.Sleep(1);
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

		Synchronize();
	}

	public bool ProcessIncoming(Vote vote, bool fromsynchronization)
	{
		if(!vote.Valid)
		{	
			Flow.Log.ReportWarning(this, $"Vote rejected !Valid : {vote}");
			return false;
		}

		if(!fromsynchronization && Synchronization == Synchronization.Downloading)
		{
			if(SynchronizationTail.TryGetValue(vote.RoundId, out var r) && r.Any(i => Bytes.EqualityComparer.Equals(i.Signature, vote.Signature)))
				return false;

			if(!SynchronizationTail.TryGetValue(vote.RoundId, out r))
				r = SynchronizationTail[vote.RoundId] = new();

			vote.Created = DateTime.UtcNow;
			r.Add(vote);
		}
		else if(fromsynchronization || Synchronization == Synchronization.Synchronized)
		{
			if(vote.RoundId <= Mcv.LastConfirmedRound.Id || Mcv.LastConfirmedRound.Id + Mcv.P*2 < vote.RoundId)
			{	
				//Flow.Log.ReportWarning(this, $"Vote rejected v.RoundId={v.RoundId} LastConfirmedRound={Mcv.LastConfirmedRound.Id}");
				return false;
			}
			
			var r = Mcv.GetRound(vote.RoundId);

			if(r.Votes.Any(i => Bytes.EqualityComparer.Equals(i.Signature, vote.Signature)))
				return false;
								
			if(r.Voters.Any() && !r.Voters.Any(i => i.Address == vote.Generator))
				return false;

			if(r.Forkers.Contains(vote.User))
				return false;

			var e = r.VotesOfTry.FirstOrDefault(i => i.Generator == vote.Generator);
			
			if(e != null) /// FORK
			{
				//Flow.Log.ReportWarning(this, $"Vote rejected FORK : {v}");

				r.Votes.Remove(e);
				r.Forkers.Add(e.User);

				return false;
			}

			vote.Restore();
			
			if(r.ParentId <= Mcv.LastConfirmedRound.Id)
			{
				//if(v.Transactions.Length > r.VotersRound.PerVoteTransactionsLimit)
				//{	
				//	//Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Length > r.Parent.PerVoteTransactionsLimit : {v}");
				//	return false;
				//}

				if(vote.Transactions.Sum(i => i.Operations.Length) > Mcv.Net.OperationsPerRoundMaximum / r.Voters.Count()) //r.VotersRound.PerVoteOperationsMaximum)
				{	
					//Flow.Log.ReportWarning(this, $"Vote rejected v.Transactions.Sum(i => i.Operations.Length) > r.Parent.PerVoteOperationsLimit : {v}");
					return false;
				}

				if(vote.Transactions.Any(t => r.Voters.NearestBy(i => i.Address, t.Signer, t.Nonce).Address != vote.Generator))
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

			Mcv.Add(vote);
		}

		return true;
	}

	public void Generate()
	{
		Statistics.Generating.Begin();
	
		if(Mcv.Settings.Generators == null || Mcv.Settings.Generators.Length == 0 || Synchronization != Synchronization.Synchronized)
			return;
	
		var votes = new List<Vote>();
	
		foreach(var gs in Mcv.Settings.Generators)
		{
			var g = gs.Signer;
			var s = FindSession(gs.User);

			if(s == null)
			{	
				Thread.Sleep(NodeGlobals.TimeoutOnError);
				continue;;
			}
					
			var m = Mcv.NextVotingRound.Voters.FirstOrDefault(i => i.Address == g);
	
			if(m == null)
			{
				if(Mcv.LastConfirmedRound.Candidates.Any(i => i.Address == g))
					continue;

				try
				{
					if(!CandidacyDeclarations.Contains(g) && s != null)
					{
						var t = new Transaction();
						t.Flow			 = Flow;
						t.Net			 = Net;
						t.User			 = gs.User;
						t.Session		 = s.Session;
						t.Signer		 = s.Signer;
						t.Application	 = Name;
	 					t.ActionOnResult = ActionOnResult.RetryUntilConfirmed;
				
						t.AddOperation(Mcv.CreateCandidacyDeclaration());
	
						CandidacyDeclarations.Add(g);
	
				 		Transact(t);
					} 
					else
					{
						if(!OutgoingTransactions.Any(i => i.User == gs.User && i.Operations.Any(o => o is CandidacyDeclaration)))
							CandidacyDeclarations.Remove(g);
					}
				}
				catch(VaultException ex)
				{
					Thread.Sleep(NodeGlobals.TimeoutOnError);
				}
			}
			else
			{
				if(CandidacyDeclarations.Count > 0)
				{
					CandidacyDeclarations.Remove(g);
					OutgoingTransactions.RemoveAll(i => i.User == gs.User && i.Operations.Any(o => o is CandidacyDeclaration));
				}
	
				var r = Mcv.NextVotingRound;
	
				if(r.ConsensusFailed)
				{
					var h = r.Parent.Hash;

					if(r.Parent.Summarize().SequenceEqual(h))
						continue;
				}
				else if(r.VotesOfTry.Any(i => i.Generator == g))
					continue;
	
				Vote createvote(Round r)
				{
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

					v.User			= m.Id;
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
	
				var txs = CandidateTransactions.Where(i => i.Status == TransactionStatus.Accepted).ToArray();
	
				var must = r.Voters.Any(i => i.Address == g) && Mcv.Tail.Any(i => i.Id > Mcv.LastConfirmedRound.Id && i.Payloads.Any());
	
				if(txs.Any() || must)
				{
					var v = createvote(r);
					var deferred = new List<Transaction>();
					var pp = r.Parent.Previous;
						
					/// Compose txs list prioritizing higher fees but ensure continuous tx Nid sequence 
	
					bool tryplace(Transaction t, bool isdeferred)	
					{ 	
						if(v.Transactions.Sum(i => i.Operations.Length) + 1 > pp.PerVoteOperationsMaximum)
							return false;
	
						if(v.Transactions.Sum(i => i.Operations.Length) + t.Operations.Length > pp.PerVoteOperationsMaximum)
							return false;
	
						if(r.Id > t.Expiration)
						{
							CandidateTransactions.Remove(t);
							return true;
						}
	
						var nearest = r.Voters.NearestBy(i => i.Address, t.Signer, t.Nonce).Address;
	
						if(nearest != g)
						{
							if(!Mcv.Settings.Generators.Any(i => i.Signer == nearest))
								CandidateTransactions.Remove(t);
	
							return true;
						}
	
						if(!isdeferred)
						{
							if(txs.Any(i => i.Signer == t.Signer && i.Nonce < t.Nonce)) /// any older tx left?
							{
								deferred.Add(t);
								return true;
							}
						}
						else
							deferred.Remove(t);
	
						t.Status = TransactionStatus.Placed;
						v.AddTransaction(t);
	
						var next = deferred.Find(i => i.Signer == t.Signer && i.Nonce + 1 == t.Nonce);
	
						if(next != null)
						{
							if(tryplace(next, true) == false)
								return false;
						}
	
						Flow.Log?.Report(this, "Transaction Placed", t.ToString());
	
						return true;
					}
	
					var stxs = txs.Select(i => new {t = i, a = Mcv.Users.Find(i.User, pp.Id)});
	
					foreach(var t in stxs.Where(i => i.a != null).OrderByDescending(i => i.a.EnergyRating))	/// Allocated bandwidth first
						if(false == tryplace(t.t, false))
							break;
	
					foreach(var t in stxs.Where(i => i.a == null))
						if(false == tryplace(t.t, false))
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
																User		= gs.User,
																Session		= s.Session,
																Hash		= v.Hashify()
															}, Flow);						
						votes.Add(v);
					}
				}
	
	 			///while(r.Previous != null && !r.Previous.Confirmed && r.Previous.VotersRound != null && r.Previous.Voters.Any(i => i.Address == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
	 			///{
	 			///	r = r.Previous;
	 			///
	 			///	var v = createvote(r);
	 			///			
				///	v.Generator = g;
				///	v.Signature	= VaultApi.Call<byte[]>(new AuthorizeApc
				///										{
				///											Cryptography= Net.Cryptography.Type,
				///											Application	= Name,
				///											Net			= Net.Name,
				///											User		= gs.User,
				///											Session		= s.Session,
				///											Hash		= v.Hashify()
				///										}, Flow);						
	 			///	votes.Add(v);
	 			///}
	
				if(CandidateTransactions.Any(i => i.Status == TransactionStatus.Accepted) || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
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
	
				//for(int i = Mcv.LastConfirmedRound.Id + 1; i <= Mcv.LastNonEmptyRound.Id; i++) /// better to start from votes.Min(i => i.Id) or last excuted
				//{
				//	var r = Mcv.GetRound(i);
				//		
				//	if(r.Hash == null)
				//	{
				//		r.ConsensusTime			= r.Previous.ConsensusTime;
				//		r.ConsensusEnergyCost	= r.Previous.ConsensusEnergyCost;
				//		r.Members				= r.Previous.Members;
				//		r.Funds					= r.Previous.Funds;
				//	}
				//
				//	if(!r.Confirmed)
				//	{
				//		r.Execute(r.OrderedTransactions.Where(i => Mcv.Settings.Generators.Any(g => g.Signer == i.Vote.Generator)));
				//	}
				//}
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

	public abstract bool ValidateIncoming(Operation o);

	public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
	{
		foreach(var t in txs.OrderBy(i => i.Nonce))
		{
			if(CandidateTransactions.Any(j => j.User == t.User && j.Nonce == t.Nonce) || t.Operations.Any(o => !ValidateIncoming(o)))
				continue;

			lock(Mcv.Lock)
			{
				var r = Mcv.Examine(t);
	
				if(r == null || !t.Successful)
					continue;
			}

			if(CandidateTransactions.Sum(i => i.Operations.Length) >= Node.Settings.PoolMaximum) /// limit reached
			{
				var min = CandidateTransactions.MinBy(i => t.Operations.First().User.EnergyRating); /// find the one with the lowest bandwidth balance

				if(t.Operations.First().User.EnergyRating + t.Boost > min.Operations.First().User.EnergyRating) /// if the new one is better, replace with the old one
					CandidateTransactions.Remove(min);
				else
					continue;
			}

			CandidateTransactions.Add(t);
			t.Status = TransactionStatus.Accepted;

			yield return t;
		}

		MainWakeup.Set();
	}

	public AccountSessionSettings FindSession(string user)
	{
		return Node.Settings.Sessions.FirstOrDefault(i => i.User == user);
	}

	public AccountSessionSettings CreateSession(string user)
	{
		var a = VaultApi.Call<AuthenticationResult>(new AuthenticateApc {Application = Name, Net = Net.Name, User = user}, Flow); 

		if(a == null)
			return null;

		var ass = new AccountSessionSettings {User = user, Signer = a.Signer, Session = a.Session};
		Node.Settings.Sessions = [..Node.Settings.Sessions, ass];
		Node.Settings.Save();

		return ass;
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

			if(cr == null)
				Debugger.Break();

			if(!cr.Members.Any() || cr.Members.Any(i => !i.GraphPpcIPs.Any()))
				continue;

			var members = cr.Members;

			IHomoPeer getppi(Generator member)
			{
				//var m = members.NearestBy(i => i.Address, nonce);

				if(member.GraphPpcIPs.Contains(Settings.EP))
					return this;

				var p = GetPeer(member.GraphPpcIPs.Random());
				Connect(p, Flow);

				return p;
			}

			Statistics.Transacting.Begin();
			
			IEnumerable<IGrouping<string, Transaction>> nones;

			lock(Lock)
				nones = OutgoingTransactions.GroupBy(i => i.User).Where(g => !g.Any(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed) && g.Any(i => i.Status == TransactionStatus.None)).ToArray();

			foreach(var g in nones)
			{
				var txs = new Dictionary<IHomoPeer, List<Transaction>>();


				foreach(var t in g.Where(i => i.Status == TransactionStatus.None))
				{
					try
					{
						foreach(var i in t.Operations)
							i.PreTransact(Node, t.Flow);

						//t.Nonce		 = 0;
						//t.Expiration = 0;
						//t.Member	 = new(0, -1);
						//t.Signature	 = VaultApi.Call<byte[]>(new AuthorizeApc
						//									 {
						//										Cryptography	= Net.Cryptography.Type,
						//										Application		= t.Application,
						//										Net				= Net.Name,
						//										User			= t.User,
						//										Session			= s.Session,
						//										Hash			= t.Hashify(),
						//									 }, t.Flow);
						//if(t.Signature == null)
						//{	
						//	t.Flow?.Log.ReportError(this, $"Failed to sign: {t}");
						//	break;
						//}

						var at = Call(new PretransactingPpc {User = t.User}, t.Flow);
							

						//var m = members.NearestBy(i => i.Address, nonce);
						IHomoPeer ppi; 
						
						var m = members.NearestBy(i => i.Address, t.Signer, at.NextNonce);

						try
						{
							ppi = getppi(m);
						}
						catch(NodeException)
						{
							Thread.Sleep(NodeGlobals.TimeoutOnError);
							continue;
						}

						t.Ppi		 = ppi;
						t.Member	 = m.Id;
						t.Nonce		 = at.NextNonce;
						t.Expiration = at.LastConfirmedRid + Mcv.TransactionPlacingLifetime;
						t.Signature  = VaultApi.Call<byte[]>(new AuthorizeApc
															 {
																Cryptography	= Net.Cryptography.Type,
																Application		= t.Application,
																Net				= Net.Name,
																User			= t.User,
																Session			= t.Session,
																Hash			= t.Hashify(),
															 }, t.Flow);

						(txs.TryGetValue(ppi, out var p) ? p : (txs[ppi] = [])).Add(t);

						t.Flow.Log?.Report(this, $"Examine: {t}");
					}
					catch(NodeException ex)
					{
						t.Flow.Log?.ReportError(this, "Examine", ex);
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

						t.Flow.Log?.ReportError(this, "Examine", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
					catch(ApiCallException ex)
					{
						t.Flow.Log?.ReportError(this, "Examine", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
				}

				foreach(var i in txs)
				{
					IEnumerable<byte[]> atxs = null;

					try
					{
						atxs = Call(i.Key, new PlaceTransactionsPpc {Transactions = [..i.Value]}).Accepted;
					}
					catch(NodeException ex)
					{
						foreach(var t in i.Value)
							t.Flow.Log?.ReportError(this, "Place", ex);

						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}

					lock(Lock)
						foreach(var t in i.Value)
						{ 
							if(atxs.Any(s => s.SequenceEqual(t.Signature)))
							{
								t.Status = TransactionStatus.Accepted;
								t.Flow.Log?.Report(this, $"{TransactionStatus.Accepted}: Member={{{i.Key}}}");
							}
							else
							{
								t.Flow.Log?.Report(this, $"Rejected by {i.Key}");

								if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
									t.Status = TransactionStatus.None;
								else
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

 	public Transaction Transact(IEnumerable<Operation> operations, string application, string user, byte[] tag, byte[] session, AccountAddress signer, ActionOnResult aor, Flow flow)
 	{
		if(operations.Count() > Net.ExecutionCyclesPerTransactionLimit)
			throw new NodeException(NodeError.LimitExceeded);

		if(!operations.Any() || operations.Any(i => !i.IsValid(Net)))
			throw new NodeException(NodeError.Invalid);

		var t = new Transaction();
		t.Application			= application;
		t.User					= user;
		t.Net					= Net;
		t.Tag					= tag ?? Guid.NewGuid().ToByteArray();
		t.Session				= session ?? FindSession(user)?.Session;
		t.Signer				= signer ?? FindSession(user)?.Signer;
		t.Flow					= flow;
		t.Inquired				= DateTime.UtcNow;
		t.ActionOnResult		= aor;
		
		if(t.Session == null || !t.IsSignerSet)
			throw new NodeException(NodeError.NoSession);

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

					var r = Call(c);

					if(r == null)
						Debugger.Break();

					return r;
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
		var mcvs = All.OfType<McvPeering>().GroupBy(i => i.Net.Address);

		foreach(var i in mcvs)
		{
			var d = Path.Join(destination, "!CompareGraphs- " + i.Key);

			CompareBase(i.Where(i => i.Mcv != null).ToArray(), d);
		}
	}

	public static void CompareBase(McvPeering[] all, string destibation)
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