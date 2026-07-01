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
	SubnetPeers,
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

public class TransactionResult
{
	public byte[]	Tag { set; get; }
	public string	Error { set; get; }
}

public abstract class McvPeering : HomoPeering
{
	public McvNode							Node;
	public McvNet							Net => Node.Net;
	public Mcv								Mcv => Node.Mcv; 
	public VaultApiClient					VaultApi; 

	AutoResetEvent							TransactingWakeup = new (true);
	Thread									TransactingThread;
	public List<Transaction>				OutgoingTransactions = [];
	public List<Transaction>				CandidateTransactions = [];
	public List<Transaction>				ConfirmedTransactions = [];
	List<AutoId>							CandidacyDeclarations = [];

	public volatile Synchronization			Synchronization = Synchronization.None;
	Thread									SynchronizingThread;
	public string							SynchronizationInfo;

	public static List<McvPeering>			All = [];

	public IEnumerable<HomoPeer>			Graphs => Connections.Where(i => i.Permanent && i.Roles.IsSet(Role.Graph));
	public bool								IsMember => Synchronization == Synchronization.Synchronized &&
														Mcv.LastConfirmedRound != null && 
														Mcv.LastConfirmedRound.Members.Any(i => Mcv.Settings.Generators.Any(j => j.Id == i.User));


	public McvPeering(McvNode node, PeeringSettings settings, long roles, VaultApiClient vaultapi, Flow flow) : base(node, node.Name, node.Net, node.Database, settings, roles, flow)
	{
		Node = node;
		VaultApi = vaultapi;

		Constructor.Merge(node.Net.Constructor);
		Constructor.Register(() => new Vote(Mcv));
		Constructor.Register<PeerRequest> (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Result>	  (Assembly.GetExecutingAssembly(), typeof(McvPpcClass), i => i.Remove(i.Length - "Ppr".Length));
		Constructor.Register<CodeException>(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));

		if(Mcv != null)
		{
			Mcv.VoteAdded += b => MainWakeup.Set();

			Mcv.Confirmed += r =>	{
										if(Synchronization == Synchronization.Synchronized)
										{
											lock(ConfirmedTransactions)
											{
												foreach(var i in r.ConsensusTransactions.Where(j => Mcv.Settings.Generators.Any(g => g.Id == j.Vote.Member)))
												{
													i.Inquired = DateTime.UtcNow;
													ConfirmedTransactions.Add(i);
												}

												ConfirmedTransactions.RemoveAll(i => DateTime.UtcNow - i.Inquired > TimeSpan.FromSeconds(Node.Settings.TransactionsKeepPeriod));
											}
											
											lock(CandidateTransactions)
												CandidateTransactions.RemoveAll(t => t.Vote?.Round?.Id == r.Id || t.Expiration <= r.Id);
										}
									};

			Mcv.ConsensusReached += r => {
											Generate(r);
											MainWakeup.Set(); /// send next try vote
										 };

			Mcv.ConsensusFailed += r =>	{
											foreach(var i in r.OrderedTransactions.Where(j => Mcv.Settings.Generators.Any(g => g.Id == j.Vote.Member)))
											{
												i.Vote = null;
												i.Status = TransactionStatus.Accepted;
											}

											MainWakeup.Set(); /// send next try vote
										};
		}

		All.Add(this);
	}

	public override void Stop()
	{
		All.Remove(this);

		TransactingThread?.Join();
		SynchronizingThread?.Join();

		base.Stop();

	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);

		if(ex.Error == NodeError.NotGraph)	peer.Roles  &= ~(long)Role.Graph;
		if(ex.Error == NodeError.NotChain)	peer.Roles  &= ~(long)Role.Chain;
	}

	protected override void Main()
	{
		lock(Lock)
		{
			base.Main();

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
				Flow.Log?.Report(this, $"{nameof(MinimalPeersReached)} reached");
	
				if(Mcv != null)
					Synchronize();
			}
		}

		if(Mcv != null)
		{
			if(MinimalPeersReached)
			{
				lock(Mcv.Lock)
					Generate();
			}
		}
	}

	public void Synchronize()
	{
		lock(Mcv.Lock)
		{
			if(Settings.Endpoint != null && Settings.Endpoint.Equals(Net.Father0EP) && Mcv.Settings.Generators.Any(g => g.User == Net.Father0Name) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound)
			{
				Synchronization = Synchronization.Synchronized;
				return;
			}

			if(Synchronization != Synchronization.Downloading)
			{
				CandidateTransactions.Clear();
				Synchronization = Synchronization.Downloading;
	
				SynchronizingThread = Node.CreateThread(Synchronizing);
				SynchronizingThread.Name = $"{Node.Name} Synchronizing";
				SynchronizingThread.Start();
		
				Flow.Log?.Report(this, $"Synchronization Started");
			}
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
				if(peer == null)
				{
					peer = Connect(Mcv.Settings.Roles, used, Flow);
				}

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
																				}, Flow);

															var c = t.GetCluster(i.Id);
															c.Import(w, d.Main);
															c.Commit(w);

															if(!c.Hash.SequenceEqual(i.Hash))
																throw new SynchronizationException("Same cluster data - different hashes");
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
																			}, Flow);
	
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
																						}, Flow);
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

					stamp = Call(peer, new StampPpc(), Flow);
	
					lock(Mcv.Lock)
						Mcv.Tail.RemoveAll(i => i.Id <= stamp.LastCommitedRound);

					foreach(var i in Mcv.Tables.Where(i => !i.IsIndex))
					{
						download(i);
					}
		
					var r = Mcv.CreateRound();
					r.Confirmed = true;
					r.ReadGraphState(new Reader(stamp.GraphState, Constructor));
		
					var s = Call(peer, new StampPpc(), Flow);
	
					lock(Mcv.Lock)
					{
						Mcv.GraphState = stamp.GraphState;
						Mcv.Hashify();
			
						if(s.GraphHash.SequenceEqual(Mcv.GraphHash))
	 					{	
							Mcv.LastConfirmedRound = r;
							Mcv.LastCommitedRound = r;
							Mcv.InsertRound(r);

							using(var w = new WriteBatch())
							{
								foreach(var i in Mcv.Tables.Where(i => i.IsIndex))
									i.Index(w, r);
						
								Mcv.Rocks.Write(w);
							}

							for(int i = Mcv.LastConfirmedRound.Id + 1; i <= Mcv.LastConfirmedRound.Id + Net.P; i++)
							{
								var vs = Mcv.FindRound(i);
								
								if(vs == null)
									goto resync;

								foreach(var v in vs.Votes.Where(i => Mcv.LastConfirmedRound.Members.Any(m => m.Since <= i.RoundId && m.User == i.Member)).GroupBy(i => i.Try).MaxBy(i => i.Key))
								{	
									v.Restore();
								//	vs.Update();
								}
							}

							Mcv.NextVotingRound.ReUpdate();

							if(Mcv.TryConfirm(Mcv.NextVotingRound))
							{
								SynchronizingThread = null;
								SynchronizationInfo = null;
											
								Flow.Log?.Report(this, $"Synchronization Finished");
								
								Synchronization = Synchronization.Synchronized;

								MainWakeup.Set();
								return;
							}
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
					while(Flow.Active)
					{
						int from;
	
						lock(Mcv.Lock)
							from = Mcv.LastConfirmedRound.Id + 1;
			
						var rp = Call(peer, new DownloadRoundsPpc {From = from}, Flow);
		
						lock(Mcv.Lock)
						{
							var rounds = rp.Read(Mcv, Constructor);
															
							foreach(var r in rounds.SkipLast(1)) /// skip the last one to give a chance for TryConfirm
							{
								if(r.Id <= Mcv.LastConfirmedRound.Id)
									continue;
	
								Flow.Log?.Report(this, $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.EP}");
			
								Mcv.InsertRound(r);
	
								var h = r.Hash;
		
								r.Hashify();
		
								if(!r.Hash.SequenceEqual(h))
								{
									#if DEBUG
										//CompareBase([this, All.First(i => i.Node.Name == peer.Name)], "a:\\1111111111111");
										if(All.Any(i => i.Node.Name == peer.Name))
										{
											lock(All.First(i => i.Node.Name == peer.Name).Mcv.Lock)
												All.First(i => i.Node.Name == peer.Name).Mcv.FindRound(r.Id).Hashify();
											
											Debugger.Break();
										}
										
									#endif
																		
									break;
								}
									
								r.Confirmed = false;
								r.Confirm();
								Mcv.Save(r);
							}
	
							Mcv.NextVotingRound.ReUpdate();
	
							if(Mcv.TryConfirm(Mcv.NextVotingRound))
							{
								SynchronizingThread = null;
								SynchronizationInfo = null;
				
								Flow.Log?.Report(this, $"Synchronization Finished");
			
								Synchronization = Synchronization.Synchronized;
	
								MainWakeup.Set();
								return;
							}
						}
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
				{	
					used.Add(peer);
					peer = null;
				}
			}
			catch(EntityException)
			{
			}
			catch(RequestException)
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

	public void Generate(Round round = null)
	{
		Statistics.Generating.Begin();
	
		if(Mcv.Settings.Generators == null || Mcv.Settings.Generators.Length == 0 || Synchronization != Synchronization.Synchronized)
			return;
	
		var votes = new List<Vote>();
	
		foreach(var gs in Mcv.Settings.Generators)
		{
			var s = FindSession(gs.User);

			if(s == null)
			{	
				//Thread.Sleep(NodeGlobals.TimeoutOnError);
				continue;;
			}
			
			if(gs.Id == null)
			{
				var u = Mcv.Users.Latest(gs.User);
				
				if(u != null)
					gs.Id = u.Id;
				else
				{
					//Thread.Sleep(NodeGlobals.TimeoutOnError);
					continue;
				}	
			}
				
			var m = Mcv.NextVotingRound.Senders.FirstOrDefault(i => i.User == gs.Id);
	
			if(m == null)
			{
				if(Mcv.LastConfirmedRound.Candidates.Any(i => i.User == gs.Id))
					continue;

				try
				{
					if(!CandidacyDeclarations.Contains(gs.Id))
					{
						if(Mcv.Users.Latest(gs.Id).Energy < Net.DeclarationCost)
							continue;
						
						CandidacyDeclarations.Add(gs.Id);
					
						Transact([Mcv.CreateCandidacyDeclaration()], gs.User, 0, null, s.Session, ActionOnResult.RetryUntilConfirmed, Flow);
					} 
					else
					{
						lock(OutgoingTransactions)
							if(!OutgoingTransactions.Any(i => i.User == gs.User && i.Operations.Any(o => o is CandidacyDeclaration)))
								CandidacyDeclarations.Remove(gs.Id);
					}
				}
				catch(VaultException ex)
				{
					//Thread.Sleep(NodeGlobals.TimeoutOnError);
					continue;
				}
			}
			else
			{
				if(CandidacyDeclarations.Count > 0)
				{
					CandidacyDeclarations.Remove(gs.Id);
			
					lock(OutgoingTransactions)
						OutgoingTransactions.RemoveAll(i => i.User == gs.User && i.Operations.Any(o => o is CandidacyDeclaration));
				}
	
				var r = round ?? Mcv.NextVotingRound;
	
				if(r.VotesOfTry.Any(i => i.Member == gs.Id))
					continue;

				if(round == null) /// when consensus is already reached
					r.ReUpdate();
	
				if(r.Target.Hash == null)
				{
					r.Target.Summarize();

					if(r.Target.Hash == null) /// Summarize failed
						continue;
				}

				Vote createvote(Round r)
				{
					var v = Mcv.CreateVote();

					v.Try							= r.Try;
					v.TargetHash					= r.Target.Hash;
					v.Member						= m.User;
					v.RoundId						= r.Id;
					v.Time							= Time.Now(Mcv.Clock);
					v.Violators						= [..r.ProposeViolators()];
					v.Leavers						= [..r.ProposeMemberLeavers(gs.Id)];
					v.FriendTransferRequests		= [..Mcv.FriendTransferRequests.Select(i => i.Hash)];
					v.FriendTransferConfirmations	= [..Mcv.FriendTransferResults.Keys];
					v.OutwardResults				= [..Mcv.OutwardResults];
						
					//v.FundJoiners	= Settings.ProposedFundJoiners.Where(i => !LastConfirmedRound.Funds.Contains(i)).ToArray();
					//v.FundLeavers	= Settings.ProposedFundLeavers.Where(i => LastConfirmedRound.Funds.Contains(i)).ToArray();
	
					Mcv.FillVote(v);
	
					return v;
				}
	
				lock(CandidateTransactions)
				{
					var txs = CandidateTransactions.Where(i => i.Status == TransactionStatus.Accepted).ToArray();
				
					var must = round != null || r.Senders.Any(i => i.User == gs.Id) && Mcv.Tail.Any(i => i.Id > Mcv.LastConfirmedRound.Id && i.Payloads.Any());
				
					if(txs.Any() || must)
					{
						var v = createvote(r);
						var deferred = new List<Transaction>();
						var pp = r.Target.Previous;
									
						var cs = new CountStream();
						v.Signature = Net.Cryptography.ZeroSignature;
						v.Write(new Writer(cs, Constructor));
						v.RawPayload = null;
	
						var l = cs.Length;
	
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
				
							l += t.Length;
							
							if(l > Peer.PacketLengthMaximum)
								return false;
				
							var nearest = r.Senders.NearestBy(t.Signature);
				
							if(nearest.User != gs.Id)
							{
								if(!Mcv.Settings.Generators.Any(i => i.Id == nearest.User))
									CandidateTransactions.Remove(t);
				
								return true;
							}
				
							if(!isdeferred)
							{
								if(txs.Any(i => i.User == t.User && i.Nonce < t.Nonce)) /// any older tx left?
								{
									deferred.Add(t);
									return true;
								}
							}
							else
								deferred.Remove(t);
	
							t.Status = TransactionStatus.Placed;
							v.AddTransaction(t);
				
							var next = deferred.Find(i => i.User == t.User && i.Nonce + 1 == t.Nonce);
				
							if(next != null)
							{
								if(tryplace(next, true) == false)
									return false;
							}
				
							Flow.Log?.Report(this, "Transaction Placed", t.ToString());
				
							return true;
						}
				
						var stxs = txs.Select(i => new {t = i, a = Mcv.Users.Latest(i.User)});
				
						foreach(var t in stxs.Where(i => i.a != null).OrderByDescending(i => i.a.EnergyRating))	/// Allocated bandwidth first
							if(false == tryplace(t.t, false))
								break;
				
						foreach(var t in stxs.Where(i => i.a == null))
							if(false == tryplace(t.t, false))
								break;
				
						if(v.Transactions.Any() || must)
						{
							v.Signature	= VaultApi.Call<byte[]>(new AuthorizeApc
																{
																	Cryptography= Net.Cryptography.Type,
																	Net			= Net.Address,
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
		}
	

		if(votes.Any())
		{
			try
			{
				foreach(var v in votes.GroupBy(i => i.RoundId).OrderBy(i => i.Key))
				{
					foreach(var i in v)
					{
						Mcv.Add(i, true, false);
							
						Flow.Log?.Report(this, $"{nameof(Vote)} generated - {i} - {i.Round}");
					}
					
					Mcv.TryConfirm(Mcv.FindRound(v.Key));
				}
			}
			catch(ConfirmationException ex)
			{
				ProcessConfirmationException(ex);
			}
	
			foreach(var v in votes)
				Broadcast(v);
		}
	
		Statistics.Generating.End();
	}

	public abstract bool ValidateIncoming(Operation o);

	public TransactionResult[] ProcessIncoming(IEnumerable<Transaction> txs)
	{
		var o = new TransactionResult[txs.Count()];
		var i = 0;

		foreach(var t in txs.OrderBy(i => i.Nonce))
		{
			var res = new TransactionResult {Tag = t.Tag};
			o[i++] = res;

			lock(CandidateTransactions)
				if(CandidateTransactions.Any(j => j.User == t.User && j.Nonce == t.Nonce))
				{	
					res.Error = "Already accepted (by User/Nonce)";
					continue;
				}

			if(t.Operations.Any(o => !ValidateIncoming(o)))
			{	
				res.Error = "Invalid data";
				continue;
			}

			var s = new CountStream();
			t.WriteForVote(new Writer(s, Constructor));

			t.Length = (int)s.Length;

			if(t.Length > Peer.PacketLengthMaximum)
			{	
				res.Error = "PacketLengthMaximum Exceeded";
				continue;
			}

			lock(Mcv.Lock)
			{
				Mcv.Examine(t);
			}
					
			if(t.OverallError != null)
			{	
				res.Error = t.OverallError;
				continue;
			}
	
			lock(CandidateTransactions)
			{
				if(CandidateTransactions.Sum(i => i.Operations.Length) >= Node.Settings.PoolMaximum) /// limit reached
				{
					var min = CandidateTransactions.MinBy(i => t.Operations.First().User.EnergyRating); /// find the one with the lowest bandwidth balance
	
					if(t.Operations.First().User.EnergyRating + t.Boost > min.Operations.First().User.EnergyRating) /// if the new one is better, replace with the old one
						CandidateTransactions.Remove(min);
					else
					{	
						res.Error = "Pool limit exceeded";
						continue;
					}
				}
	
				CandidateTransactions.Add(t);
				t.Status = TransactionStatus.Accepted;
			}
		}

		MainWakeup.Set();

		return o;
	}

	public AccountSessionSettings FindSession(string user)
	{
		return Node.Settings.Sessions.FirstOrDefault(i => i.User == user);
	}

	public AccountSessionSettings CreateSession(string application, string user)
	{
		var a = VaultApi.Call<AuthenticationResult>(new AuthenticateApc {Application = application, Net = Net.Address, User = user}, Flow); 

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

			lock(OutgoingTransactions)
				nothing = OutgoingTransactions.All(i => i.Status == TransactionStatus.Confirmed);
			
			if(nothing)		
				WaitHandle.WaitAny([TransactingWakeup, Flow.Cancellation.WaitHandle]);

			MembersPpr cr = null;

			try
			{
				cr = Call(new MembersPpc(), Flow);
			}
			catch(EntityException ex)
			{
				Flow.Log?.ReportError(this, $"MembersPpc", ex);
				continue;
			}

			if(cr == null)
				Debugger.Break();

			if(!cr.Members.Any() || cr.Members.Any(i => !i.GraphPpiEndpoints.Any()))
				continue;

			var members = cr.Members;

			IHomoPeer getppi(Generator member)
			{
				//var m = members.NearestBy(i => i.Address, nonce);

				if(member.GraphPpiEndpoints.Contains(Settings.Endpoint))
					return this;

				var p = GetPeer(member.GraphPpiEndpoints.Random());
				Connect(p, Flow);

				return p;
			}

			Statistics.Transacting.Begin();
			
			IEnumerable<IGrouping<string, Transaction>> nones;

			lock(OutgoingTransactions)
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

						var at = Call(new PretransactingPpc {User = t.User}, t.Flow);
							
						t.Nonce		 = at.NextNonce;
						t.Expiration = at.LastConfirmedRid + Net.P * 2;
						t.Signature  = VaultApi.Call<byte[]>(new AuthorizeApc
															 {
																Cryptography	= Net.Cryptography.Type,
																Net				= Net.Address,
																User			= t.User,
																Session			= t.Session,
																Hash			= t.Hashify(Net),
															 }, 
															 t.Flow);


						var m = members.NearestBy(t.Signature);

						try
						{
							t.Peer = getppi(m);
						}
						catch(NodeException)
						{
							Thread.Sleep(NodeGlobals.TimeoutOnError);
							continue;
						}

						//t.Member	 = m.User;

						(txs.TryGetValue(t.Peer, out var p) ? p : (txs[t.Peer] = [])).Add(t);

						t.Flow.Log?.Report(this, $"Signed {t}, Member {m}");
					}
					catch(NodeException ex)
					{
						t.Error = $"NodeException - {ex.Message}";
						t.Flow.Log?.ReportError(this, "Pretransacting", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
					catch(VaultException ex)
					{
						t.Error = $"VaultException - {ex.Message}";
						t.Flow.Log?.ReportError(this, "Pretransacting", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
					///catch(EntityException ex)
					///{
					///	if(t.ActionOnResult != ActionOnResult.RetryUntilConfirmed)
					///	{
					///		lock(OutgoingTransactions)
					///			//t.Status = TransactionStatus.FailedOrNotFound;
					///			OutgoingTransactions.Remove(t);
					///	} 
					///
					///	///t.Error = $"EntityException - {ex.Message}";
					///	t.Flow.Log?.ReportError(this, "Examine", ex);
					///	Thread.Sleep(NodeGlobals.TimeoutOnError);
					///	continue;
					///}
					catch(ApiCallException ex)
					{
						t.Error = $"ApiCallException - {ex.Message}";
						t.Flow.Log?.ReportError(this, "Pretransacting", ex);
						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}
				}

				foreach(var i in txs)
				{
					TransactionResult[] atxs = null;

					try
					{
						atxs = Call(i.Key, new PlaceTransactionsPpc {Transactions = [..i.Value]}, Flow).Results;
					}
					catch(NodeException ex)
					{
						foreach(var t in i.Value)
						{	
							t.Error = $"NodeException - {ex.Message}";
							t.Flow.Log?.ReportError(this, "Place", ex);
						}

						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}

					foreach(var t in i.Value)
					{ 
						var r = atxs.FirstOrDefault(s => s.Tag.SequenceEqual(t.Tag));

						if(r == null)
						{
							t.Status = TransactionStatus.FailedOrNotFound;
							t.Error = $"Incorrect behavior of the member {i.Key}";
							t.Flow.Log?.Report(this, $"{t.Error} - Member={i.Key}");
						}
						else if(r.Error == null)
						{
							t.Status = TransactionStatus.Accepted;
							t.Flow.Log?.Report(this, $"{TransactionStatus.Accepted} - Member={{{i.Key}}}");
						}
						else
						{
							t.Error = r.Error;
							t.Flow.Log?.ReportError(this, $"{r.Error} - Member={i.Key}");

							if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
								t.Status = TransactionStatus.None;
							else
								lock(OutgoingTransactions)
									OutgoingTransactions.Remove(t);
						}
					}
				}
			}

			Transaction[] accepted;
			
			lock(OutgoingTransactions)
				accepted = OutgoingTransactions.Where(i => i.Status == TransactionStatus.Accepted || i.Status == TransactionStatus.Placed).ToArray();

			///Flow.Log?.Report(this, $"accepted : {accepted.Count()}" );

			if(accepted.Any())
			{
				foreach(var g in accepted.GroupBy(i => i.Peer))
				{
					TransactionStatusPpr ts;

					try
					{
						ts = Call(g.Key, new TransactionStatusPpc {Tags = [..g.Select(i => i.Tag)]}, Flow);
					}
					catch(NodeException ex)
					{	
						foreach(var t in g)
						{	
							t.Error = $"TransactionStatusRequest - {ex.Message}";
							Flow.Log?.ReportError(this, "TransactionStatusRequest", ex);
						}

						Thread.Sleep(NodeGlobals.TimeoutOnError);
						continue;
					}

					foreach(var i in ts.Transactions)
					{
						var t = accepted.First(a => a.Tag.SequenceEqual(i.Tag));
						
						t.Error = i.Error;
						
						if(t.Status != i.Status)
						{
							t.Flow.Log?.Report(this, $"{i.Status}");

							t.Status = i.Status;

							if(t.Status == TransactionStatus.FailedOrNotFound)
							{
								if(t.ActionOnResult == ActionOnResult.RetryUntilConfirmed)
									t.Status = TransactionStatus.None;
								else
									lock(OutgoingTransactions)
										OutgoingTransactions.Remove(t);
							}
							else if(t.Status == TransactionStatus.Confirmed)
							{
								t.Error = null;
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
			OutgoingTransactions.RemoveAll(i => DateTime.UtcNow - i.Inquired > TimeSpan.FromSeconds(Node.Settings.TransactionsKeepPeriod));
		}
		
		if(OutgoingTransactions.Count(i => i.Status != TransactionStatus.Confirmed) <= Mcv.TransactionQueueLimit)
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

 	public Transaction Transact(IEnumerable<Operation> operations, string user, long boost, byte[] tag, byte[] session, ActionOnResult aor, Flow flow)
 	{
		if(operations.Count() > Net.ExecutionCyclesPerTransactionLimit)
			throw new NodeException(NodeError.LimitExceeded);

		if(!operations.Any() || operations.Any(i => !i.IsValid(Net)))
			throw new NodeException(NodeError.Invalid);

		var t = new Transaction()
				{
					User			= user,
					Boost			= boost,
					Tag				= tag ?? Guid.NewGuid().ToByteArray(),
					Session			= session ?? FindSession(user)?.Session,
					Flow			= flow,
					Inquired		= DateTime.UtcNow,
					ActionOnResult	= aor,
				};
		
		if(t.Session == null)
			throw new NodeException(NodeError.NoSession);

		foreach(var i in operations)
			t.AddOperation(i);

		lock(OutgoingTransactions)
		 	Transact(t);

		return t;
 	}

	public R Call<R>(Ppc<R> call, Flow flow, IEnumerable<HomoPeer> exclusions = null)  where R : Result
	{
		return Call((PeerRequest)call, flow, exclusions) as R;
	}

	public Result Call(PeerRequest call, Flow flow, IEnumerable<HomoPeer> exclusions)
	{
		HashSet<HomoPeer> tried;
		
		void init()
		{
			tried = exclusions != null ? [..exclusions] : [];
		}

		init();

		HomoPeer p;

		while(flow.Active)
		{
			Thread.Sleep(1);

			try
			{
				lock(Lock)
				{
					if(Synchronization == Synchronization.Synchronized)
					{
						call.Peering = this;

						var r = CallMe(call, flow);

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
				
				Connect(p, flow);

				call.Peering = this;

				return p.CallMe(call, flow);
			}
			catch(NodeException ex)
			{
			}
			catch(ObjectDisposedException ex)
			{
			}
			catch(IOException ex)
			{
			}
			catch(ContinueException)
			{
			}
			catch(OperationCanceledException)
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

		var jo = new JsonSerializerOptions(NetJsonConfiguration.CreateOptions());
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