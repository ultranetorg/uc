using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void VoidDelegate();
 	public delegate void SunDelegate(Sun d);

	public class Statistics
	{
		public PerformanceMeter Consensing = new();
		public PerformanceMeter Generating = new();
		public PerformanceMeter Transacting = new();
		public PerformanceMeter Verifying = new();
		public PerformanceMeter Declaring = new();
		public PerformanceMeter Sending = new();
		public PerformanceMeter Reading = new();

		public void Reset()
		{
			Consensing.Reset();
			Generating.Reset();
			Transacting.Reset();
			Verifying.Reset();
			Declaring.Reset();
			Sending.Reset();
			Reading.Reset();
		}
	}

	public enum Synchronization
	{
		Null, Downloading, Synchronizing, Synchronized
	}

	[Flags]
	public enum Role : uint
	{
		Null,
		Base		= 0b00000001,
		Chain		= 0b00000011,
		//Analyzer	= 0b00000101,
		Seed		= 0b00000100,
	}

	public class ReleaseStatus
	{
		public bool						ExistsRecursively { get; set; }
		public Manifest					Manifest { get; set; }
		public PackageDownloadReport	Download { get; set; }
	}

	public class Sun : RdcInterface
	{
		public Role						Roles => (Mcv != null ? Mcv.Roles : Role.Null)|(ResourceHub != null ? Role.Seed : Role.Null);

		public System.Version			Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]	Versions = {1};
		public const string				FailureExt = "failure";
		public const int				Timeout = 5000;
		public const int				OperationsQueueLimit = 1000;
		const int						BalanceWidth = 24;

		public Zone						Zone;
		public Settings					Settings;
		public Workflow					Workflow;
		JsonServer						ApiServer;
		public Vault					Vault;
		public INas						Nas;
		LookupClient					Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});
		public Mcv						Mcv;
		public ResourceHub				ResourceHub;
		public PackageHub				PackageHub;
		public SeedHub					SeedHub;
		public object					Lock = new();
		public Clock					Clock;

		public RocksDb					Database;
		public ColumnFamilyHandle		PeersFamily => Database.GetColumnFamily(nameof(Peers));
		readonly DbOptions				DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																			.SetCreateMissingColumnFamilies(true);

		public Guid						Nuid;
		public IPAddress				IP = IPAddress.None;
		public bool						IsNodeOrUserRun => MainThread != null;
		public bool						IsClient => ListeningThread == null;

		public Statistics				PrevStatistics = new();
		public Statistics				Statistics = new();

		public List<Transaction>		IncomingTransactions = new();
		List<Transaction>				OutgoingTransactions = new();
		public List<Analysis>			Analyses = new();
		public List<OperationId>		ApprovedEmissions = new();
		public List<OperationId>		ApprovedDomainBids = new();

		public Coin						TransactionPerByteMinFee;
		public int						TransactionThresholdExcessRound;

		public bool						MinimalPeersReached;
		bool							OnlineBroadcasted;
		public List<Peer>				Peers = new();
		public IEnumerable<Peer>		Connections	=> Peers.Where(i => i.Status == ConnectionStatus.OK);
		public IEnumerable<Peer>		Bases
										{
											get
											{
												return Connections.Where(i => i.BaseRank > 0);
											}
										}

		public List<IPAddress>			IgnoredIPs	= new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		Thread							TransactingThread;
		//Thread							VerifingThread;
		Thread							SynchronizingThread;
		AutoResetEvent					MainSignal = new AutoResetEvent(true);

		public Synchronization			_Synchronization = Synchronization.Null;
		public Synchronization			Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public SunDelegate				SynchronizationChanged;
		
		public SunDelegate				Stopped;

		public bool						IsMember => Synchronization == Synchronization.Synchronized && Settings.Generators.Any(g => Mcv.LastConfirmedRound.Members.Any(i => i.Account == g));

		public class SyncRound
		{
			public List<Vote>					Votes = new();
			public List<AnalyzerVoxRequest>		AnalyzerVoxes = new();
			public List<MemberJoinRequest>		Joins = new();
		}
		
		public Dictionary<int, SyncRound>	SyncCache	= new();

		public IGasAsker				GasAsker; 
		public IFeeAsker				FeeAsker;

		public SunDelegate				MainStarted;
		public SunDelegate				ApiStarted;


		
		public List<KeyValuePair<string, string>> Summary
		{
			get
			{
				List<KeyValuePair<string, string>> f = new();
															
				f.Add(new ("Version",					Version.ToString()));
				f.Add(new ("Zone",						Zone.Name));
				f.Add(new ("Profile",					Settings.Profile));
				f.Add(new ("IP(Reported):Port",			$"{Settings.IP} ({IP}) : {Zone.Port}"));
				f.Add(new ("Incoming Transactions",		$"{IncomingTransactions.Count}"));
				f.Add(new ("Outgoing Transactions",		$"{OutgoingTransactions.Count}"));
				f.Add(new ("    Pending Delegation",	$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Pending)}"));
				f.Add(new ("    Accepted",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Accepted)}"));
				//f.Add(new ("    Pending Placement",		$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Verified)}"));
				f.Add(new ("    Placed",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Placed)}"));
				f.Add(new ("    Confirmed",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Confirmed)}"));
				//f.Add(new ("Peers in/out/min/known",	$"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}"));
				
				if(Mcv != null)
				{
					f.Add(new ("Synchronization",		$"{Synchronization}"));
					f.Add(new ("Size",					$"{Mcv.Size}"));
					f.Add(new ("Members",				$"{Mcv.LastConfirmedRound?.Members.Count}"));
					f.Add(new ("Emission",				$"{(Mcv.LastPayloadRound != null ? Mcv.LastPayloadRound.Emission.ToHumanString() : null)}"));
					f.Add(new ("SyncCache Blocks",		$"{SyncCache.Sum(i => i.Value.Votes.Count)}"));
					f.Add(new ("Cached Rounds",			$"{Mcv.LoadedRounds.Count()}"));
					f.Add(new ("Last Non-Empty Round",	$"{(Mcv.LastNonEmptyRound != null ? Mcv.LastNonEmptyRound.Id : null)}"));
					f.Add(new ("Last Payload Round",	$"{(Mcv.LastPayloadRound != null ? Mcv.LastPayloadRound.Id : null)}"));
					f.Add(new ("Base Hash",				$"{Hex.ToHexString(Mcv.BaseHash)}"));
					f.Add(new ("Generating (μs)",		(Statistics.Generating.Avarage.Ticks/10).ToString()));
					f.Add(new ("Consensing (μs)",		(Statistics.Consensing.Avarage.Ticks/10).ToString()));
					f.Add(new ("Transacting (μs)",		(Statistics.Transacting.Avarage.Ticks/10).ToString()));
					f.Add(new ("Verifying (μs)",		(Statistics.Verifying.Avarage.Ticks/10).ToString()));
					f.Add(new ("Declaring (μs)",		(Statistics.Declaring.Avarage.Ticks/10).ToString()));
					f.Add(new ("Sending (μs)",			(Statistics.Sending.Avarage.Ticks/10).ToString()));
					f.Add(new ("Reading (μs)",			(Statistics.Reading.Avarage.Ticks/10).ToString()));

					if(Synchronization == Synchronization.Synchronized)
					{
						string formatbalance(AccountAddress a)
						{
							return Mcv.Accounts.Find(a, Mcv.LastConfirmedRound.Id)?.Balance.ToHumanString();
						}
	
						foreach(var i in Vault.Wallets)
						{
							var a = i.Key.ToString();
							f.Add(new ($"{a.Substring(0, 8)}...{a.Substring(a.Length-8, 8)}", $"{formatbalance(i.Key)}"));
						}
	
						if(DevSettings.UI)
						{
						}
					}
				}
				else
				{
					//f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in Vault.Wallets)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				Statistics.Reset();
		
				return f;
			}
		}
		
		public Sun(Zone zone, Settings settings)
		{
			Zone = zone;
			Settings = settings;
			TransactionPerByteMinFee = settings.TransactionPerByteMinFee;

			Directory.CreateDirectory(Settings.Profile);

			Vault = new Vault(Zone, Settings);

			var cfamilies = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{	new (nameof(Peers),					new ()),
																new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (AuthorTable.MetaColumnName,	new ()),
																new (AuthorTable.MainColumnName,	new ()),
																new (AuthorTable.MoreColumnName,	new ()),
																new (Mcv.ChainFamilyName,			new ()),
																new (ResourceHub.FamilyName,		new ()) })
				cfamilies.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
		}

		public override string ToString()
		{
			var gens = Mcv?.LastConfirmedRound != null ? Settings.Generators.Where(i => Mcv.LastConfirmedRound.Members.Any(j => j.Account == i)) : new AccountKey[0];
	
			return	$"{(Roles.HasFlag(Role.Base) ? "B" : "")}" +
					$"{(Roles.HasFlag(Role.Chain) ? "C" : "")}" +
					$"{(Roles.HasFlag(Role.Seed) ? "S" : "")}" +
					$"{(Connections.Count() < Settings.PeersMin ? " - Low Peers" : "")}" +
					$"{(Settings.Anonymous ? " - A" : "")}" +
					$"{(!IP.Equals(IPAddress.None) ? $" - {IP}" : "")}" +
					$" - {Synchronization}" +
					(Mcv?.LastConfirmedRound != null ? $" - {gens.Count()}/{Mcv.LastConfirmedRound.Members.Count()} members" : "");
		}

		public object Constract(Type t, byte b)
		{
			if(t == typeof(Transaction)) return new Transaction(Zone);
			if(t == typeof(Vote)) return new Vote(Mcv);
			if(t == typeof(Manifest)) return new Manifest(Zone);
			if(t == typeof(RdcRequest)) return RdcRequest.FromType((Rdc)b); 
			if(t == typeof(RdcResponse)) return RdcResponse.FromType((Rdc)b); 

			return null;
		}

		public void RunApi()
		{
			if(!HttpListener.IsSupported)
			{
				Environment.ExitCode = -1;
				throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
			}

			lock(Lock)
			{
				ApiServer = new JsonServer(this);
			}
		
			ApiStarted?.Invoke(this);
		}

		public void Run(Xon xon, Workflow workflow)
		{
			if(xon.Has("api"))
				RunApi();
			
			if(xon.Has("node"))
				RunNode(workflow, (xon.Has("base") ? Role.Base : Role.Null) | (xon.Has("chain") ? Role.Chain : Role.Null));
			else if(xon.Has("user"))
				RunUser(workflow);

			if(xon.Has("seed"))
				RunSeed();
		}

		public void RunUser(Workflow workflow)
		{
			Workflow = workflow != null ? workflow.CreateNested("RunUser", workflow?.Log) : new Workflow("RunUser", workflow?.Log);
			Workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Node.log"), FileMode.Create);

			Workflow.Log?.Report(this, $"Ultranet Client {Version}");
			Workflow.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(DevSettings.Any)
				Workflow.Log?.ReportWarning(this, $"Dev: {DevSettings.AsString}");

			Nuid = Guid.NewGuid();

			LoadPeers();
			
 			MainThread = new (() =>	{ 
										Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

										try
										{
											while(Workflow.Active)
											{
												lock(Lock)
												{
													ProcessConnectivity();
												}
	
												Thread.Sleep(1);
											}
										}
										catch(Exception ex) when (!Debugger.IsAttached)
										{
											Stop(MethodBase.GetCurrentMethod(), ex);
										}
 									});
			MainThread.Start();
			MainStarted?.Invoke(this);
		}

		public void RunNode(Workflow workflow, Role roles)
		{
			Workflow = workflow != null ? workflow.CreateNested("RunNode", new Log()) : new Workflow("RunNode", new Log());
			Workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Node.log"), FileMode.Create);

			Workflow.Log?.Report(this, $"Ultranet Node {Version}");
			Workflow.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(DevSettings.Any)
				Workflow.Log?.ReportWarning(this, $"Dev: {DevSettings.AsString}");

			Nuid = Guid.NewGuid();

			ListeningThread = new Thread(Listening);
			ListeningThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Listening";

			if(Settings.Generators.Any())
			{
				SeedHub = new SeedHub(this);
			}

			if(roles.HasFlag(Role.Base) || roles.HasFlag(Role.Chain))
			{
				Mcv = new Mcv(Zone, roles, Settings.Mcv, Database);

				Mcv.BlockAdded += b =>	{
											MainSignal.Set();
										};

				Mcv.ConsensusConcluded += (r, reached) =>	{
																if(reached)
																{
																	/// Check OUR blocks that are not come back from other peer, means first peer went offline, if any - force broadcast them
																	var notcomebacks = r.Parent.Votes.Where(i => i.Peers != null && !i.BroadcastConfirmed).ToArray();
					
																	foreach(var v in notcomebacks)
																		Broadcast(v);
																}
																else
																{
																	foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId >= r.Id))
																	{
																		i.Vote = null;
																		i.Placing = PlacingStage.Accepted;
																	}
																}
															};

				Mcv.Confirmed +=	(Round r) =>
									{
										var ops = r.ConfirmedTransactions.SelectMany(t => t.Operations).ToArray();

										foreach(var o in ops)
										{
											if(o is AuthorBid ab && ab.Tld.Any())
											{
 												if(!DevSettings.SkipDomainVerification)
 												{
													Task.Run(() =>	{
 																		try
 																		{
 																			var result = Dns.QueryAsync(ab.Name + '.' + ab.Tld, QueryType.TXT, QueryClass.IN, Workflow.Cancellation);
 															
 																			var txt = result.Result.Answers.TxtRecords().FirstOrDefault(r => r.DomainName == ab.Name + '.' + ab.Tld + '.');
 		
 																			if(txt != null && txt.Text.Any(i => i == o.Transaction.Signer.ToString()))
 																			{
																				lock(Lock)
																				{	
																					ApprovedDomainBids.Add(ab.Id);
																				}
 																			}
 																		}
 																		catch(DnsResponseException ex)
 																		{
 																			Workflow.Log?.ReportError(this, "Can't verify AuthorBid domain", ex);
 																		}
																	});
 												}
												else
													ApprovedDomainBids.Add(ab.Id);
											}

											if(o is Emission e)
											{
												Task.Run(() =>	{
 																	try
 																	{
 																		if(Nas.CheckEmission(e))
 																		{
																			lock(Lock)
																			{	
																				ApprovedEmissions.Add(e.Id);
																			}
 																		}
 																	}
 																	catch(Exception ex)
 																	{
 																		Workflow.Log?.ReportError(this, "Can't verify Emission operation", ex);
 																	}
																});
											}
										}


										ApprovedEmissions.RemoveAll(i => r.ConfirmedEmissions.Contains(i) || r.Id > i.Round + Zone.ExternalVerificationDuration);
										ApprovedDomainBids.RemoveAll(i => r.ConfirmedDomainBids.Contains(i) || r.Id > i.Round + Zone.ExternalVerificationDuration);
										IncomingTransactions.RemoveAll(t => t.Vote != null && t.Vote.Round.Id <= r.Id || t.Expiration <= r.Id);
										Analyses.RemoveAll(i => r.ConfirmedAnalyses.Any(j => j.Resource == i.Resource && j.Finished));
											
										if(r.ConfirmedTransactions.Length > Settings.TransactionCountPerRoundThreshold)
										{
											TransactionPerByteMinFee *= 2;
											TransactionThresholdExcessRound = r.Id;
										}
										else if(TransactionPerByteMinFee > Settings.TransactionPerByteMinFee && r.Id - TransactionThresholdExcessRound > Mcv.Pitch)
										{
											TransactionPerByteMinFee /= 2;
										}
									};
		
				if(Settings.Generators.Any())
				{
		  			try
		  			{
		 				new Uri(Settings.Nas.Provider);
		  			}
		  			catch(Exception)
		  			{
		  				Nas.ReportEthereumJsonAPIWarning($"Ethereum Json-API provider required to run the node as a generator.", true);
						return;
		  			}

					//Generator = PrivateAccount.Parse(Settings.Generator);

					//VerifingThread = new Thread(Verifing);
					//VerifingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Verifing";
					//VerifingThread.Start();
				}

				Workflow.Log?.Report(this, "Chain started");
			}

			LoadPeers();

			ListeningThread.Start();

 			MainThread = new Thread(() =>
 									{ 
										Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

										try
										{
											while(Workflow.Active)
											{
												WaitHandle.WaitAny(new[] {MainSignal, Workflow.Cancellation.WaitHandle}, 500);

												lock(Lock)
												{
													ProcessConnectivity();
												
													if(Mcv != null)
													{
														if(Synchronization == Synchronization.Synchronized)
														{
															/// TODO: Rethink
															///var conns = Connections.Where(i => i.Roles.HasFlag(Database.Roles)).ToArray(); /// Not cool, cause Peer.Established may change after this and 'conn' items will change
															///
															///if(conns.Any())
															///{
															///	var max = conns.Aggregate((i, j) => i.Count() > j.Count() ? i : j);
															///
															///	if(max.Key - Database.LastConfirmedRound.Id > Net.Database.Pitch) /// we are late, force to resync
															///	{
															///		 StartSynchronization();
															///	}
															///}
															///
															if(Settings.Generators.Any())
															{
																Generate();
															}
														}
													}
												}
	
												//Thread.Sleep(1);
											}
										}
										catch(OperationCanceledException)
										{
											Stop("Canceled");
										}
										catch(Exception ex) when (!Debugger.IsAttached)
										{
											Stop(MethodBase.GetCurrentMethod(), ex);
										}
 									});
			MainThread.Start();
			MainStarted?.Invoke(this);
		}

		public void RunSeed()
		{
			ResourceHub = new ResourceHub(this, Zone, System.IO.Path.Join(Settings.Profile, "Resources"));
			PackageHub = new PackageHub(this, ResourceHub, Settings.Packages);
		}

		public void Stop(MethodBase methodBase, Exception ex)
		{
			lock(Lock)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(methodBase.Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings.Profile, m + "." + Sun.FailureExt), ex.ToString());
				Workflow?.Log?.ReportError(this, m, ex);
	
				Stop("Exception");
			}
		}

		public void Stop(string message)
		{
			ApiServer?.Stop();
			Workflow?.Abort();
			
			Listener?.Stop();

			while(Peers.Any(i => i.Status != ConnectionStatus.Disconnected))
				lock(Lock)
				{
					foreach(var i in Peers.Where(i => i.Status != ConnectionStatus.Disconnected))
						i.Disconnect();

				}


			MainThread?.Join();
			ListeningThread?.Join();
			TransactingThread?.Join();
			//VerifingThread?.Join();
			SynchronizingThread?.Join();

			Database?.Dispose();

			Workflow?.Log?.Report(this, "Stopped", message);

			Stopped?.Invoke(this);
		}

		public Peer GetPeer(IPAddress ip)
		{
			Peer p = null;

			lock(Lock)
			{
				p = Peers.Find(i => i.IP.Equals(ip));
	
				if(p != null)
					return p;
	
				p = new Peer(ip);
				Peers.Add(p);
			}

			return p;
		}

		void LoadPeers()
		{
			using(var i = Database.NewIterator(PeersFamily))
			{
				for(i.SeekToFirst(); i.Valid(); i.Next())
				{
	 				var p = new Peer(new IPAddress(i.Key()));
					p.Fresh = false;
	 				p.LoadNode(new BinaryReader(new MemoryStream(i.Value())));
	 				Peers.Add(p);
				}
			}
			
			if(Peers.Any())
			{
				Workflow?.Log?.Report(this, "Peers loaded", $"n={Peers.Count}");
			}
			else
			{
				Peers = Zone.Initials.Select(i => new Peer(i) {Fresh = false, LastSeen = DateTime.MinValue}).ToList();

				UpdatePeers(Peers);
			}
		}

		public void UpdatePeers(IEnumerable<Peer> peers)
		{
			using(var b = new WriteBatch())
			{
				foreach(var i in peers)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
					i.SaveNode(w);
					b.Put(i.IP.GetAddressBytes(), s.ToArray(), PeersFamily);
				}
	
				Database.Write(b);
			}
		}

		public List<Peer> RefreshPeers(IEnumerable<Peer> peers)
		{
			lock(Lock)
			{
				var toupdate = new List<Peer>();
													
				foreach(var i in peers.Where(i => !i.IP.Equals(IP)))
				{
					var p = Peers.Find(j => j.IP.Equals(i.IP));
					
					if(p == null)
					{
						i.Fresh = true;
						
						Peers.Add(i);
						toupdate.Add(i);
					}
					else
					{
						p.Fresh = true;

						bool b = p.BaseRank == 0 && i.BaseRank > 0;
						bool c = p.ChainRank == 0 && i.ChainRank > 0;

						if(b || c)
						{
							if(b) p.BaseRank = 1;
							if(c) p.ChainRank = 1;
						
							toupdate.Add(p);
						}
					}
				}
	
				UpdatePeers(toupdate);

				return toupdate;
			}
		}

		void ProcessConnectivity()
		{
			var needed = Settings.PeersMin - Peers.Count(i => i.Status != ConnectionStatus.Disconnected);
		
			foreach(var p in Peers.Where(m =>	m.Status == ConnectionStatus.Disconnected &&
												DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
									.OrderByDescending(i => i.PeerRank)
									.ThenBy(i => i.Retries)
									.Take(needed))
			{
				OutboundConnect(p);
			}

			foreach(var i in Peers.Where(i => i.Status == ConnectionStatus.Failed))
				i.Disconnect();

			if(!MinimalPeersReached && 
				Connections.Count() >= Settings.PeersMin && 
				(!Roles.HasFlag(Role.Base) || Connections.Count(i => i.Roles.HasFlag(Role.Base)) >= Settings.Mcv.PeersMin))
			{
				if(Mcv != null)
				{
					Synchronize();
				}

				MinimalPeersReached = true;

				foreach(var c in Connections)
				{
					c.Send(new PeersBroadcastRequest {Peers = Connections.Where(i => i != c).ToArray()});
				}
			}
		}

		void Listening()
		{
			try
			{
				Workflow?.Log?.Report(this, "Listening starting", $"{Settings.IP}:{Zone.Port}");

				Listener = new TcpListener(Settings.IP, Zone.Port);
				Listener.Start();
	
				while(Workflow.Active)
				{
					var c = Listener.AcceptTcpClient();

					if(Workflow.Aborted)
						return;
	
					lock(Lock)
					{
						if(!Workflow.Aborted && Connections.Count() < Settings.PeersInMax)
							InboundConnect(c);
						else
							c.Close();
					}
				}
			}
			catch(SocketException e) when(e.SocketErrorCode == SocketError.Interrupted)
			{
				Listener = null;
			}
			catch(ObjectDisposedException)
			{
				Listener = null;
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
			catch(OperationCanceledException)
			{
			}
		}

		Hello CreateHello(IPAddress ip)
		{
			Peer[] peers;
		
			lock(Lock)
			{
				peers = Peers.Where(i => i.Fresh).ToArray();
			}

			var h = new Hello();

			h.Roles			= Roles;
			h.Versions		= Versions;
			h.Zone			= Zone.Name;
			h.IP			= ip;
			h.Nuid			= Nuid;
			h.Peers			= peers;
			//h.Generators	= Members;
			
			return h;
		}

		void OutboundConnect(Peer peer)
		{
			peer.Status = ConnectionStatus.Initiated;
			peer.LastTry = DateTime.UtcNow;
			peer.Retries++;

			void f()
			{
				try
				{
					var client = new TcpClient(new IPEndPoint(Settings.IP, 0));

					try
					{
						client.SendTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;
						//client.ReceiveTimeout = Timeout;
						client.Connect(peer.IP, Zone.Port);
					}
					catch(SocketException ex) 
					{
						Workflow.Log?.Report(this, "connectivity", $"Establishing failed To {peer.IP}; Connect; {ex.Message}" );
						goto failed;
					}
	
					IEnumerable<Peer> peers;
											
					lock(Lock)
					{
						peers = Peers.ToArray();
					}
											
					Hello h = null;
									
					try
					{
						client.SendTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;
						client.ReceiveTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;

						Peer.SendHello(client, CreateHello(peer.IP));
						h = Peer.WaitHello(client);
					}
					catch(Exception ex)// when(!Settings.Dev.ThrowOnCorrupted)
					{
						Workflow.Log?.Report(this, "connectivity", $"Establishing failed to {peer.IP}; Send/Wait Hello; {ex.Message}" );
						goto failed;
					}
	
					lock(Lock)
					{
						if(Workflow.Aborted)
						{
							client.Close();
							return;
						}

						if(!h.Versions.Any(i => Versions.Contains(i)))
						{
							client.Close();
							return;
						}

						if(h.Zone != Zone.Name)
						{
							client.Close();
							return;
						}

						if(h.Nuid == Nuid)
						{
							Workflow.Log?.Report(this, "connectivity", "Establishing failed: It's me");
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
							client.Close();
							return;
						}
													
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, "connectivity", $"Reported IP {IP}");
						}
	
						if(peer.Status == ConnectionStatus.OK)
						{
							Workflow.Log?.Report(this, "connectivity", $"Establishing failed from {peer.IP}: Already established" );
							client.Close();
							return;
						}
	
						RefreshPeers(h.Peers.Append(peer));
	
						peer.Start(this, client, h, $"{Settings.IP.GetAddressBytes()[3]}", false);
							
						//Workflow.Log?.Report(this, "Connected", $"to {peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
						return;
					}
	
					failed:
					{
						lock(Lock)
						{
							peer.Status = ConnectionStatus.Failed;
						}
									
						client.Close();
					}
				}
				catch(Exception ex) when(!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
			
			var t = new Thread(f);
			t.Name = Settings.IP.GetAddressBytes()[3] + " -> out -> " + peer.IP.GetAddressBytes()[3];
			t.Start();
						
		}

		private void InboundConnect(TcpClient client)
		{
			var ip = (client.Client.RemoteEndPoint as IPEndPoint).Address.MapToIPv4();
			var peer = Peers.Find(i => i.IP.Equals(ip));

			if(ip.Equals(IP))
			{
				IgnoredIPs.Add(ip);
				
				if(peer != null)
					Peers.Remove(peer);
				
				client.Close();
				return;
			}

			if(IgnoredIPs.Contains(ip))
			{
				if(peer != null)
					Peers.Remove(peer);

				client.Close();
				return;
			}

			if(peer != null)
			{
				if(peer.Status == ConnectionStatus.OK || peer.Status == ConnectionStatus.Initiated)
				{
					client.Close();
					return;
				}

				if(peer.Status == ConnectionStatus.Failed)
				{
					peer.Disconnect();
				
					while(peer.Status != ConnectionStatus.Disconnected) 
						Thread.Sleep(1);
				}
								
				peer.Status = ConnectionStatus.Initiated;
			}

			var t = new Thread(a => incon());
			t.Name = Settings.IP.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
			t.Start();

			void incon()
			{
				try
				{
					Hello h = null;
	
					try
					{
						client.SendTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;
						client.ReceiveTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;

						h = Peer.WaitHello(client);
					}
					catch(Exception ex) when(!DevSettings.ThrowOnCorrupted)
					{
						Workflow.Log?.Report(this, "connectivity", $"Establishing failed from {ip}; WaitHello {ex.Message}");
						goto failed;
					}
				
					lock(Lock)
					{
						if(Workflow.Aborted)
							return;

						if(!h.Versions.Any(i => Versions.Contains(i)))
						{
							client.Close();
							return;
						}

						if(h.Zone != Zone.Name)
						{
							client.Close();
							return;
						}

						if(h.Nuid == Nuid)
						{
							Workflow.Log?.Report(this, "connectivity", "Establishing failed: It's me");
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
							client.Close();
							return;
						}

						if(peer != null && peer.Status == ConnectionStatus.OK)
						{
							Workflow.Log?.Report(this, "connectivity", $"Establishing failed from {ip}: Already established" );
							client.Close();
							return;
						}
	
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, "connectivity", $"Reported IP {IP}");
						}
		
						try
						{
							Peer.SendHello(client, CreateHello(ip));
						}
						catch(Exception ex) when(!DevSettings.ThrowOnCorrupted)
						{
							Workflow.Log?.Report(this, "connectivity", $"Establishing failed from {ip}; SendHello; {ex.Message}");
							goto failed;
						}
	
						if(peer == null)
						{
							peer = new Peer(ip);
							Peers.Add(peer);
						}

						//foreach(var i in h.Generators)
						//{
						//	if(!Members.Any(j => j.Generator == i.Generator))
						//	{
						//		i.OnlineSince = ChainTime.Zero;
						//		i.Proxy = peer;
						//		Members.Add(i);
						//	}
						//}

						RefreshPeers(h.Peers.Append(peer));
	
						//peer.InStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, $"{Settings.IP.GetAddressBytes()[3]}", true);
			
						//Workflow.Log?.Report(this, "Accepted from", $"{peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
						return;
					}
	
				failed:
					lock(Lock)
						if(peer != null)
							peer.Status = ConnectionStatus.Failed;

					client.Close();
				}
				catch(Exception ex) when(!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
		}

		void Synchronize()
		{
			if(DevSettings.SkipSynchronizetion)
			{
				Synchronization = Synchronization.Synchronized;
				return;
			}

			if(Synchronization != Synchronization.Downloading && Synchronization != Synchronization.Synchronizing)
			{
				Workflow.Log?.Report(this, "Syncing started");

				SynchronizingThread = new Thread(Synchronizing);
				SynchronizingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Synchronizing";
				SynchronizingThread.Start();
		
				Synchronization = Synchronization.Downloading;
			}
		}

		void Synchronizing()
		{
			var used = new HashSet<Peer>();
	
			StampResponse stamp = null;

			while(Workflow.Active)
			{
				try
				{
					Thread.Sleep(1000);

					Mcv.LoadedRounds.Clear();

					var peer = Connect(Mcv.Roles.HasFlag(Role.Chain) ? Role.Chain : Role.Base, used, Workflow);

					if(Mcv.Roles.HasFlag(Role.Base) && !Mcv.Roles.HasFlag(Role.Chain))
					{
						Mcv.Tail.Clear();
		
						stamp = peer.GetStamp();
		
						void download<E, K>(Table<E, K> t) where E : ITableEntry<K>
						{
							var ts = peer.GetTableStamp(t.Type, (t.Type switch
																		{ 
																			Tables.Accounts	=> stamp.Accounts.Where(i => {
																															var c = Mcv.Accounts.SuperClusters.ContainsKey(i.Id);
																															return !c || !Mcv.Accounts.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														 }),
																			Tables.Authors	=> stamp.Authors.Where(i =>	{
																															var c = Mcv.Authors.SuperClusters.ContainsKey(i.Id);
																															return !c || !Mcv.Authors.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														}),
																			_ => throw new SynchronizationException()
																		}
																).Select(i => i.Id).ToArray());
		
							foreach(var i in ts.Clusters)
							{
								var c = t.Clusters.Find(j => j.Id == i.Id);
		
								if(c == null || !c.Hash.SequenceEqual(i.Hash))
								{
									if(c == null)
									{
										c = new Table<E, K>.Cluster(t, (ushort)i.Id);
										t.Clusters.Add(c);
									}
		
									var d = peer.DownloadTable(t.Type, (ushort)i.Id, 0, i.Length);
											
									c.Read(new BinaryReader(new MemoryStream(d.Data)));
										
									using(var b = new WriteBatch())
									{
										c.Save(b);
		
										if(!c.Hash.SequenceEqual(i.Hash))
										{
											throw new SynchronizationException();
										}
									
										Mcv.Engine.Write(b);
									}
		
									Workflow.Log?.Report(this, "Cluster downloaded", $"{t.GetType().Name} {c.Id}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						download<AccountEntry, AccountAddress>(Mcv.Accounts);
						download<AuthorEntry, string>(Mcv.Authors);
		
						var r = new Round(Mcv) {Confirmed = true};
		
						r.ReadBaseState(new BinaryReader(new MemoryStream(stamp.BaseState)));
		
						Mcv.BaseState = stamp.BaseState;
						Mcv.LastConfirmedRound = r;
						Mcv.LastCommittedRound = r;
		
						Mcv.Hashify();
		
						if(peer.GetStamp().BaseHash.SequenceEqual(Mcv.BaseHash))
 							Mcv.LoadedRounds.Add(r.Id, r);
						else
							throw new SynchronizationException();
					}
		
					int final = -1; 
					//int end = -1; 
					int from = -1;

					while(Workflow.Active)
					{
						lock(Lock)
							if(final == -1)
								if(Mcv.Roles.HasFlag(Role.Chain))
									from = Mcv.LastConfirmedRound.Id + 1;
								else
									if(from == -1)
										from = Math.Max(stamp.FirstTailRound, Mcv.LastConfirmedRound == null ? -1 : (Mcv.LastConfirmedRound.Id + 1));
									else
										from = Mcv.LastConfirmedRound.Id + 1;
							else
								from = final;
		
						var to = from + Mcv.Pitch;
		
						var rp = peer.Request<DownloadRoundsResponse>(new DownloadRoundsRequest{From = from, To = to});

// 						if(!Mcv.Roles.HasFlag(Role.Chain))
// 						{ 
// 							var d = rp.LastConfirmedRound % Zone.TailLength;
// 											
// 							if(d < Mcv.Pitch || Zone.TailLength - Mcv.Pitch * 3 > d)
// 								throw new SynchronizationException();
// 						}

						lock(Lock)
						{
							if(from <= rp.LastNonEmptyRound)
							{
								foreach(var i in SyncCache.Keys)
								{
									if(i < rp.LastConfirmedRound + 1 - Mcv.Pitch)
									{
										SyncCache.Remove(i);
									}
								}

								var rounds = rp.Read(Mcv);
									
								bool confirmed = true;
				
								foreach(var r in rounds.OrderBy(i => i.Id))
								{
									if(r.Confirmed)
									{
										if(!confirmed)
							 				throw new SynchronizationException();

										foreach(var t in r.Transactions)
										{
											//t.Round = r;
											t.Placing = PlacingStage.Placed;
										}
				
										Mcv.Tail.RemoveAll(i => i.Id == r.Id); /// remove old round with all its blocks
										Mcv.Tail.Add(r);
										Mcv.Tail = Mcv.Tail.OrderByDescending(i => i.Id).ToList();
		
										r.Confirmed = false;
										Mcv.Confirm(r, false);
//#if DEBUG
//										if(!r.Hash.SequenceEqual(h))
//										{
//											throw new SynchronizationException();
//										}
//#endif
									}
									else
									{
										if(confirmed)
										{
											confirmed		= false;
											final			= rounds.Max(i => i.Id) + 1;
											//end			= rp.LastNonEmptyRound;
											Synchronization	= Synchronization.Synchronizing;
										}
			

										foreach(var i in r.Votes)
											ProcessIncoming(i);

										foreach(var i in r.JoinRequests)
											i.Execute(this);
									}
								}
										
								Workflow.Log?.Report(this, "Rounds received", $"{rounds.Min(i => i.Id)}..{rounds.Max(i => i.Id)}");
							}
							else if(Mcv.BaseHash.SequenceEqual(rp.BaseHash))
							{
								Synchronization = Synchronization.Synchronized;

								foreach(var i in SyncCache.OrderBy(i => i.Key))
								{
									foreach(var v in i.Value.Votes)
										ProcessIncoming(v);

									foreach(var jr in i.Value.Joins)
										jr.Execute(this);
								}
									
								SyncCache.Clear();
	
								SynchronizingThread = null;
									
								MainSignal.Set();

								Workflow.Log?.Report(this, "Syncing finished");

								return;
							}
							else
								throw new SynchronizationException();
						}
					}
				}
				catch(RdcNodeException)
				{
				}
				catch(RdcEntityException)
				{
				}
				catch(SynchronizationException)
				{
				}
				catch(ConnectionFailedException)
				{
				}
				catch(OperationCanceledException)
				{
					return;
				}
			}
		}

		public bool ProcessIncoming(Vote v)
		{
			if(!v.Valid)
				return false;

			if(Synchronization == Synchronization.Null || Synchronization == Synchronization.Downloading || Synchronization == Synchronization.Synchronizing)
			{
 				var min = SyncCache.Any() ? SyncCache.Max(i => i.Key) - Mcv.Pitch * 3 : 0; /// keep latest Pitch * 3 rounds only
 
				if(v.RoundId < min || (SyncCache.ContainsKey(v.RoundId) && SyncCache[v.RoundId].Votes.Any(j => j.Signature.SequenceEqual(v.Signature))))
					return false;

				if(v.Transactions.Any(i => !i.Valid(Mcv)))
					return false;

				if(!SyncCache.TryGetValue(v.RoundId, out SyncRound r))
				{
					r = SyncCache[v.RoundId] = new();
				}

				r.Votes.Add(v);

				foreach(var i in SyncCache.Keys)
				{
					if(i < min)
					{
						SyncCache.Remove(i);
					}
				}
			}
			else if(Synchronization == Synchronization.Synchronized)
			{
				if(v.RoundId <= Mcv.LastConfirmedRound.Id || Mcv.LastConfirmedRound.Id + Mcv.Pitch * 2 < v.RoundId)
					return false;

				if(v.RoundId <= Mcv.LastVotedRound.Id - Mcv.Pitch / 2)
					return false;

				var r = Mcv.GetRound(v.RoundId);
				
				if(r.Parent != null && r.Parent.Members.Count > 0 && v.Transactions.Length > r.Parent.TransactionCountPerVoteMax || r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
					return false;

				try
				{
					Mcv.Add(v);
				}
				catch(ConfirmationException)
				{
					Synchronize();
					return false;
				}

				if(v.Transactions.Any(i => !i.Valid(Mcv))) /// do it only after adding to the chainbase
				{
					r.Votes.Remove(v);
					return false;
				}
			}

			return true;
		}

		public List<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			if(!Settings.Generators.Any(g => Mcv.LastConfirmedRound.Members.Any(m => g == m.Account))) /// not ready to process external transactions
				return new();

			var accepted = txs.Where(i =>	!IncomingTransactions.Any(j => i.EqualBySignature(j)) &&
											i.Fee >= i.Operations.Sum(i => i.CalculateSize()) * TransactionPerByteMinFee &&
											i.Expiration > Mcv.LastConfirmedRound.Id &&
											Settings.Generators.Any(g => g == i.Generator) &&
											i.Valid(Mcv)).ToList();
			
			foreach(var i in accepted)
				i.Placing = PlacingStage.Accepted;

			IncomingTransactions.AddRange(accepted);

			return accepted;
		}

		//void Verifing()
		//{
		//	Workflow.Log?.Report(this, "Verifing started");
		//
		//	try
		//	{
		//		while(Workflow.Active)
		//		{
		//			Thread.Sleep(1);
		//
		//			Statistics.Verifying.Begin();
		//
		//			lock(Lock)
		//			{
		//				foreach(var t in IncomingTransactions.Where(i => i.Placing == PlacingStage.Accepted).ToArray())
		//				{
		//					if(Valid(t))
		//					{
		//						t.Placing = PlacingStage.Verified;
		//						MainSignal.Set();
		//					} 
		//					else
		//					{
		//						IncomingTransactions.Remove(t);
		//					}
		//				}
		//			}
		//
		//			Statistics.Verifying.End();
		//		}
		//	}
		//	catch(Exception ex) when (!Debugger.IsAttached)
		//	{
		//		Stop(MethodBase.GetCurrentMethod(), ex);
		//	}
		//	catch(OperationCanceledException)
		//	{
		//	}
		//}

		void GenerateAnalysis()
		{
			Statistics.Generating.Begin();

			if(Mcv.AnalyzersOf(Mcv.LastConfirmedRound.Id + 1 + Mcv.Pitch).Any(i => i.Account == Settings.Analyzer))
			{
				var r = Mcv.GetRound(Mcv.LastConfirmedRound.Id + 1 + Mcv.Pitch);

				if(r.AnalyzerVoxes.Any(i => i.Account == Settings.Analyzer))
					return;
	
				if(Analyses.Any())
				{
					var v = new AnalyzerVoxRequest();
					var a = new List<Analysis>();

					//var s = new MemoryStream(); 
					//var w = new BinaryWriter(s);
					//v.Sign(Zone, Settings.Analyzer);
					//v.Write(w);
	
					foreach(var i in Analyses)
					{
						//w.Write(i.Resource);
						//w.Write((byte)i.Result);
						//
						//if(s.Position > Vote.SizeMax)
						//	break;
		
						a.Add(i);
					}
					
					v.Sign(Zone, Settings.Analyzer);

					foreach(var i in Bases)
					{
						i.Send(new AnalyzerVoxRequest {RoundId = r.Id, Analyses = a, Signature = v.Signature});
					}

					Workflow.Log?.Report(this, "AnalyzerVoxRequest generated", $"by {Settings.Analyzer}");
				}
			}

			Statistics.Generating.End();
		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var votes = new List<Vote>();

			foreach(var g in Settings.Generators)
			{
				if(!Mcv.LastConfirmedRound.Members.Any(i => i.Account == g))
				{
					var d = Mcv.Accounts.Find(g, Mcv.LastConfirmedRound.Id);

					if(d == null || d.BailStatus != BailStatus.Active || d.Bail < Zone.BailMin)
						break;

					///var jr = Database.FindLastBlock(i => i is JoinMembersRequest jr && jr.Generator == g, Database.LastConfirmedRound.Id - Database.Pitch) as JoinMembersRequest;

					MemberJoinRequest jr = null;

					for(int i = Mcv.LastNonEmptyRound.Id; i >= Mcv.LastConfirmedRound.Id - Mcv.Pitch; i--)
					{
						var r = Mcv.FindRound(i);

						if(r != null)
						{
							jr = r.JoinRequests.Find(j => j.Generator == g);
							
							if(jr != null)
								break;
						}
					}

					if(jr == null || jr.RoundId + Mcv.Pitch <= Mcv.LastConfirmedRound.Id)
					{
						jr = new MemberJoinRequest()
							{	
								RoundId	= Mcv.LastConfirmedRound.Id + Mcv.Pitch,
								//IPs  = new [] {IP}
							};
						
						jr.Sign(Zone, g);
						
						Mcv.GetRound(jr.RoundId).JoinRequests.Add(jr);
						//blocks.Add(b);

						//if(BaseConnections.Count(i => i.Established) < Settings.Database.PeersMin)
						//{
						//	BaseConnections = Connect(Role.Base, Settings.Database.PeersMin, Workflow);
						//}

						foreach(var i in Connections)
						{
							var bjr = new MemberJoinRequest {RoundId = jr.RoundId, Signature = jr.Signature};

							i.Send(bjr);
						}
					}
				}
				else
				{
					var r = Mcv.GetRound(Mcv.LastConfirmedRound.Id + 1 + Mcv.Pitch);

					if(r.VotesOfTry.Any(i => i.Generator == g))
						continue;

					//if(r.Parent == null || r.Parent.Payloads.Any(i => i.Hash == null)) /// cant refer to downloaded rounds since its blocks have no hashes
					//	continue;

					var txs = IncomingTransactions.Where(i => i.Generator == g && r.Id <= i.Expiration && i.Placing == PlacingStage.Accepted).OrderByDescending(i => i.Fee).ToArray();

					var prev = r.Previous.VotesOfTry.FirstOrDefault(i => i.Generator == g);

					Vote createvote()
					{
						return new Vote(Mcv){	RoundId			= r.Id,
												Try				= r.Try,
												ParentSummary	= Mcv.Summarize(r.Parent),
												Created			= Clock.Now,
												TimeDelta		= prev == null || prev.RoundId <= Mcv.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Created).TotalMilliseconds,
												Violators		= Mcv.ProposeViolators(r).ToArray(),
												MemberJoiners	= Mcv.ProposeMemberJoiners(r).ToArray(),
												MemberLeavers	= Mcv.ProposeMemberLeavers(r, g).ToArray(),
												AnalyzerJoiners	= Settings.ProposedAnalyzerJoiners.ToArray(),
												AnalyzerLeavers	= Settings.ProposedAnalyzerLeavers.ToArray(),
												FundJoiners		= Settings.ProposedFundJoiners.ToArray(),
												FundLeavers		= Settings.ProposedFundLeavers.ToArray(),
												Emissions		= ApprovedEmissions.ToArray(),
												DomainBids		= ApprovedDomainBids.ToArray(),
												BaseIPs			= Settings.Anonymous ? new IPAddress[] {} : new IPAddress[] {IP},
												HubIPs			= Settings.Anonymous ? new IPAddress[] {} : new IPAddress[] {IP} };
					};
	
					if(txs.Any() || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any())) /// any pending foreign transactions or any our pending operations OR some unconfirmed payload 
					{
						var v = createvote();
	
						if(txs.Any())
						{
							foreach(var i in txs)
							{
								if(v.Transactions.Length > r.Parent.TransactionCountPerVoteMax)
									break;

								v.AddTransaction(i);
			
								i.Placing = PlacingStage.Placed;
							}
						}
						
						v.Sign(g);
						votes.Add(v);
					}

					if(txs.Any(i => i.Placing == PlacingStage.Accepted) || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						MainSignal.Set();

					while(Mcv.MembersOf(r.Previous.Id).Any(i => i.Account == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
					{
						r = r.Previous;

						prev = r.Previous.VotesOfTry.FirstOrDefault(i => i.Generator == g);

						var b = createvote();
								
						b.Sign(g);
						votes.Add(b);
					}
				}
			}

			if(votes.Any())
			{
				try
				{
					foreach(var b in votes)
					{
						Mcv.Add(b);
					}
				}
				catch(ConfirmationException)
				{
					Synchronize();
				}

				foreach(var i in votes)
				{
					Broadcast(i);
				}
													
				 Workflow.Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{Hex.ToHexString(((byte[])i.Generator).Take(4).ToArray())}-{i.RoundId}")));
			}

			Statistics.Generating.End();
		}

		void Transacting()
		{
			IEnumerable<Transaction>	accepted;

			Workflow.Log?.Report(this, "Delegating started");

			RdcInterface rdi = null;
			AccountAddress m = null;

			while(Workflow.Active)
			{
				lock(Lock)
				{
					if(!OutgoingTransactions.Any())
					{
						TransactingThread = null;
						return;
					}
				}
				
				chooserdi:

				if(rdi == null)
				{
					if(Settings.Generators.Any())
					{
						m = Settings.Generators.First();
						rdi = this;
					}
					else
					{ 
						while(Workflow.Active)
						{
							var cr = Call(i => i.GetMembers(), Workflow);
	
							if(!cr.Members.Any())
								continue;

							lock(Lock)
							{
								var members = cr.Members;

								//Members.RemoveAll(i => !members.Contains(i.Generator));
								
								//foreach(var i in members.Where(i => i.BaseIPs.Any()).OrderByRandom()) /// look for public IP in connections
								//{
								//	var p = Connections.OrderByRandom().FirstOrDefault(j => i.BaseIPs.Any(ip => j.IP.Equals(ip)));
								//
								//	if(p != null)
								//	{
								//		rdi = p;
								//		m = i.Account;
								//		Workflow.Log?.Report(this, "Generator direct connection established", $"{i} {p}");
								//		break;
								//	}
								//}
								//
								//if(rdi != null)
								//	break;

								foreach(var i in members.Where(i => i.BaseIPs.Any()).OrderByRandom()) /// try by public IP address
								{
									foreach(var j in i.BaseIPs.OrderByRandom())
									{
										var p = GetPeer(j);

										try
										{
											Monitor.Exit(Lock);

											Connect(p, Workflow);
											rdi = p;
											m = i.Account;
											Workflow.Log?.Report(this, "Generator direct connection established", $"{i} {p}");
											break;
										}
										catch(ConnectionFailedException)
										{
										}
										finally
										{
											Monitor.Enter(Lock);
										}
									}

									if(rdi != null)
										break;
								}

								if(rdi != null)
									break;

								//foreach(var i in members.Where(i => i.Proxyable).OrderByRandom()) /// look for a Proxy in connections
								//{
								//	var p = Connections.OrderByRandom().FirstOrDefault(j => i.Proxy == j);
								//
								//	if(p != null)
								//	{
								//		try
								//		{
								//			Monitor.Exit(Lock);
								//		
								//			Connect(p, Workflow);
								//			m = i.Account;
								//			rdi = new ProxyRdi(m, p);
								//			Workflow.Log?.Report(this, "Generator proxy connection established", $"{i} {p}");
								//			break;
								//		}
								//		catch(Exception)
								//		{
								//		}
								//		finally
								//		{
								//			Monitor.Enter(Lock);
								//		}
								//	}
								//}
								//
								//if(rdi != null)
								//	break;
									
								//foreach(var i in members.Where(i => i.Proxyable).OrderByRandom()) /// connect via random proxy
								//{
								//	try
								//	{
								//		Monitor.Exit(Lock);
								//
								//		//Connect(i.Proxy, Workflow);
								//		m = i.Account;
								//		rdi = new ProxyRdi(m, cr.Peer);
								//		Workflow.Log?.Report(this, "Generator proxy connection established", $"{m} {cr.Peer}");
								//		break;
								//	}
								//	catch(Exception)
								//	{
								//	}
								//	finally
								//	{
								//		Monitor.Enter(Lock);
								//	}
								//}

								//if(rdi != null)
								//	break;

								var gp = GetPeer(Zone.GenesisIP);

								try
								{
									Monitor.Exit(Lock);

									Connect(gp, Workflow);

									m = Zone.Father0;
									rdi = gp;
									Workflow.Log?.Report(this, "Generator connection established", $"{m}, {gp}");
								}
								catch(ConnectionFailedException)
								{
								}
								finally
								{
									Monitor.Enter(Lock);
								}
							}
						}
					}
				}


				Statistics.Transacting.Begin();

				var txs = new List<Transaction>();
				
				lock(Lock)
				{
					if(rdi == this && Synchronization != Synchronization.Synchronized)
						continue;

					if(!OutgoingTransactions.Any(i => i.Placing >= PlacingStage.Accepted))
					{
						foreach(var g in OutgoingTransactions.Where(i => i.Placing == PlacingStage.Null).GroupBy(i => i.Signer))
						{
							Monitor.Exit(Lock);

							AllocateTransactionResponse at = null;

							try
							{
								at = rdi.Request<AllocateTransactionResponse>(new AllocateTransactionRequest {Account = g.Key});
							}
							catch(RdcNodeException)
							{
								rdi = null;
								Monitor.Enter(Lock);
								goto chooserdi;
							}
									
							Monitor.Enter(Lock);
							
							int tid = at.NextTransactionId;

							foreach(var t in g)
							{
								t.Id = tid++;
								t.Generator = m;
								t.Expiration = at.MaxRoundId;
	
								foreach(var o in t.Operations)
								{
									t.Fee += o.CalculateTransactionFee(at.PerByteMinFee);
								}
	
								t.Sign(Vault.GetKey(t.Signer), at.PowHash);
								txs.Add(t);
							}
						}
					}
				}

				IEnumerable<Transaction> atxs = null;

				try
				{
					atxs = rdi.SendTransactions(txs).Accepted.Select(i => txs.Find(t => t.Signature.SequenceEqual(i))).ToArray();
				}
				catch(RdcNodeException)
				{
					rdi = null;
					goto chooserdi;
				}
	
				lock(Lock)
				{	
					foreach(var i in atxs)
						i.Placing = PlacingStage.Accepted;

					//OutgoingTransactions.AddRange(atxs);

					//foreach(var i in txs.Where(t => !atxs.Contains(t)))
					//	foreach(var o in i.Operations)
					//		o.Transaction = null;
					//
					
					if(atxs.Any())
					{
						if(atxs.Sum(i => i.Operations.Count) <= 1)
							Workflow.Log?.Report(this, "Operations sent", atxs.SelectMany(i => i.Operations).Select(i => i.ToString()));
						else
							Workflow.Log?.Report(this, "Operation sent", $"{atxs.First().Operations.First()} -> {m} {rdi}");
					}
				}

				lock(Lock)
					accepted = OutgoingTransactions.Where(i => i.Placing >= PlacingStage.Accepted).ToArray();
	
				if(accepted.Any())
				{
					try
					{
						var rp = rdi.GetTransactionStatus(accepted.Select(i => new TransactionsAddress{Account = i.Signer, Id = i.Id}));

						lock(Lock)
						{
							foreach(var i in rp.Transactions)
							{
								var t = accepted.First(d => d.Signer == i.Account && d.Id == i.Id);
																		
								if(t.Placing != i.Placing)
								{
									t.Placing = i.Placing;

									if(i.Placing == PlacingStage.Confirmed || i.Placing == PlacingStage.FailedOrNotFound)
									{
										#if DEBUG
										if(t.__ExpectedPlacing >= PlacingStage.FailedOrNotFound && t.__ExpectedPlacing != i.Placing)
										{	
											//rdi.GetTransactionStatus(accepted.Select(i => new TransactionsAddress{Account = i.Signer, Id = i.Id}));
											Debugger.Break();
										}
										#endif

										OutgoingTransactions.Remove(t);
									}
								}
							}
						}
					}
					catch(RdcNodeException)
					{
						rdi = null;
						goto chooserdi;
					}
				}

				Statistics.Transacting.End();
			}
		}

		void Enqueue(Transaction t)
		{
			if(OutgoingTransactions.Count <= OperationsQueueLimit)
			{
				if(TransactingThread == null)
				{
					TransactingThread = new Thread(() => { 
															try
															{
																Transacting();
															}
															catch(OperationCanceledException)
															{
															}
															catch(Exception ex) when (!Debugger.IsAttached && Workflow.Active)
															{
																Stop(MethodBase.GetCurrentMethod(), ex);
															}
														});

					TransactingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Transacting";
					TransactingThread.Start();
				}

				//o.Placing = PlacingStage.PendingDelegation;
				OutgoingTransactions.Add(t);
			} 
			else
			{
				Workflow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
// 		public Operation Enqueue(Operation operation, PlacingStage waitstage, Workflow workflow)
// 		{
// 			operation.__ExpectedPlacing = waitstage;
// 
// 			if(FeeAsker.Ask(this, operation.Signer as AccountKey, operation))
// 			{
// 				lock(Lock)
// 				 	Enqueue(operation);
// 
// 				Await(operation, waitstage, workflow);
// 
// 				return operation;
// 			}
// 			else
// 				return null;
// 		}
		
		public void Enqueue(Operation operation, AccountAddress signer, PlacingStage await, Workflow workflow)
		{
			Enqueue(new Operation[] {operation}, signer, await, workflow);
		}

 		public void Enqueue(IEnumerable<Operation> operations, AccountAddress signer, PlacingStage awaitstage, Workflow workflow)
 		{
			var t = new Transaction(Zone);

 			lock(Lock)
			{	
				t.Signer = signer;
 				t.__ExpectedPlacing = awaitstage;
			
				foreach(var i in operations)
				{
					t.AddOperation(i);
				}
				 
 				if(FeeAsker.Ask(this, signer, null))
 				{
 				 	Enqueue(t);
 				}
			}
 
			Await(t, awaitstage, workflow);
 		}

		void Await(Transaction t, PlacingStage s, Workflow workflow)
		{
			while(workflow.Active)
			{ 
				switch(s)
				{
					case PlacingStage.Null :				return;
					case PlacingStage.Accepted :			if(t.Placing >= PlacingStage.Accepted) goto end; else break;
					case PlacingStage.Placed :				if(t.Placing >= PlacingStage.Placed) goto end; else break;
					case PlacingStage.Confirmed :			if(t.Placing == PlacingStage.Confirmed) goto end; else break;
					case PlacingStage.FailedOrNotFound :	if(t.Placing == PlacingStage.FailedOrNotFound) goto end; else break;
				}

				Thread.Sleep(100);
			}

			end:
				workflow.Log?.Report(this, $"Transaction is {t.Placing}", t.ToString());
		}

		public Peer ChooseBestPeer(Role role, HashSet<Peer> exclusions)
		{
			return Peers.Where(i => i.GetRank(role) > 0 && (exclusions == null || !exclusions.Contains(i))).OrderByDescending(i => i.Status == ConnectionStatus.OK)
																											.ThenBy(i => i.GetRank(role))
																											//.ThenByDescending(i => i.ReachFailures)
																											.FirstOrDefault();
		}

		public Peer Connect(Role role, HashSet<Peer> exclusions, Workflow workflow)
		{
			Peer peer;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					peer = ChooseBestPeer(role, exclusions);
	
					if(peer == null)
					{
						exclusions?.Clear();
						continue;
					}
				}

				exclusions?.Add(peer);

				try
				{
					Connect(peer, workflow);
	
					return peer;
				}
				catch(ConnectionFailedException)
				{
				}
			}

			throw new ConnectionFailedException("Aborted, overall abort or timeout");
		}

		public Peer[] Connect(Role role, int n, Workflow workflow)
		{
			var peers = new HashSet<Peer>();
				
			while(workflow.Active)
			{
				Peer p;
	
				lock(Lock)
					p = ChooseBestPeer(role, peers);
	
				if(p != null)
				{
					try
					{
						Connect(p, workflow);
					}
					catch(ConnectionFailedException)
					{
						continue;
					}

					peers.Add(p);

					if(peers.Count == n)
					{
						return peers.ToArray();
					}
				}
			}

			throw new ConnectionFailedException("Aborted, overall abort or timeout");
		}

		public void Connect(Peer peer, Workflow workflow)
		{
			lock(Lock)
			{
				if(peer.Status != ConnectionStatus.OK && peer.Status != ConnectionStatus.Initiated && peer.Status != ConnectionStatus.Disconnecting)
				{
					OutboundConnect(peer);
				}
			}

			var t = DateTime.Now;

			while(workflow.Active)
			{
				Thread.Sleep(1);

				lock(Lock)
					if(peer.Status == ConnectionStatus.OK)
						return;
					else if(peer.Status == ConnectionStatus.Failed)
						throw new ConnectionFailedException("Failed");

				if(!DevSettings.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new ConnectionFailedException("Timed out");
			}
		}


		public R Call<R>(Func<Peer, R> call, Workflow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					p = ChooseBestPeer(Role.Base, tried);
	
					if(p == null)
					{
						tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();
						continue;
					}
				}

				tried.Add(p);

				try
				{
					Connect(p, workflow);

					return call(p);
				}
				catch(ConnectionFailedException)
				{
					p.LastFailure[Role.Base] = DateTime.UtcNow;
				}
 				catch(RdcNodeException)
 				{
					p.LastFailure[Role.Base] = DateTime.UtcNow;
 				}
				catch(ContinueException)
				{
				}
			}

			throw new OperationCanceledException();
		}

// 		public void Call(Role role, Action<Peer> call, Workflow workflow, IEnumerable<Peer> exclusions = null, bool exitifnomore = false)
// 		{
// 			var excl = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();
// 
// 			Peer p;
// 				
// 			while(!workflow.IsAborted)
// 			{
// 				Thread.Sleep(1);
// 				workflow.ThrowIfAborted();
// 	
// 				lock(Lock)
// 				{
// 					p = ChooseBestPeer(role, excl);
// 	
// 					if(p == null)
// 						if(exitifnomore)
// 							return;
// 						else
// 							continue;
// 				}
// 
// 				excl?.Add(p);
// 
// 				try
// 				{
// 					Connect(p, workflow);
// 
// 					call(p);
// 
// 					break;
// 				}
// 				catch(ConnectionFailedException)
// 				{
// 					p.LastFailure[role] = DateTime.UtcNow;
// 				}
// 				catch(RdcNodeException)
// 				{
// 					p.LastFailure[role] = DateTime.UtcNow;
// 				}
// 			}
// 		}

		public R Call<R>(IPAddress ip, Func<Peer, R> call, Workflow workflow)
		{
			Peer p;
				
			p = GetPeer(ip);

			Connect(p, workflow);

			return call(p);
		}

		public void Send(IPAddress ip, Action<Peer> call, Workflow workflow)
		{
			Peer p;
				
			p = GetPeer(ip);

			Connect(p, workflow);

			call(p);
		}

		public double EstimateFee(IEnumerable<Operation> operations)
		{
			return Mcv != null ? operations.Sum(i => (double)i.CalculateTransactionFee(TransactionPerByteMinFee).ToDecimal()) : double.NaN;
		}

		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, AccountKey signer, PlacingStage awaitstage, Workflow workflow)
		{
			var	l = Call(p =>{
								try
								{
									return p.GetAccountInfo(signer);
								}
								catch(RdcEntityException ex) when (ex.Error == RdcEntityError.NotFound)
								{
									return new AccountResponse();
								}
							}, 
							workflow);
			
			var eid = l.Account == null ? 0 : l.Account.LastEmissionId + 1;

			Nas.Emit(a, wei, signer, GasAsker, eid, workflow);		
						
			var o = new Emission(wei, eid);


			//flow?.SetOperation(o);
						
			if(FeeAsker.Ask(this, signer, o))
			{
				return o;
			}

			return null;
		}

		public Emission FinishEmission(AccountKey signer, Workflow workflow)
		{
			lock(Lock)
			{
				var	l = Call(p =>{
									try
									{
										return p.GetAccountInfo(signer);
									}
									catch(RdcEntityException ex) when (ex.Error == RdcEntityError.NotFound)
									{
										return new AccountResponse();
									}
								}, 
								workflow);
			
				var eid = l.Account == null ? 0 : l.Account.LastEmissionId + 1;

				var wei = Nas.FinishEmission(signer, eid);

				if(wei == 0)
					throw new RequirementException("No corresponding Ethrereum transaction found");

				var o = new Emission(wei, eid);

				if(FeeAsker.Ask(this, signer, o))
				{
					Enqueue(o, signer, PlacingStage.Confirmed, workflow);
					return o;
				}
			}

			return null;
		}

		public override Rp Request<Rp>(RdcRequest rq) where Rp : class
  		{
			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;
				rq = BinarySerializator.Deserialize<RdcRequest>(new(s), Constract);
			}

 			return rq.Execute(this) as Rp;
 		}

		public override void Send(RdcRequest rq)
  		{
			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;
				rq = BinarySerializator.Deserialize<RdcRequest>(new(s), Constract);
			}

 			rq.Execute(this);
 		}

		public void Broadcast(Vote vote, Peer skip = null)
		{
			foreach(var i in Bases.Where(i => i != skip))
			{
				try
				{
					i.Send(new MemberVoxRequest {Raw = vote.RawForBroadcast });
				}
				catch(ConnectionFailedException)
				{
				}
			}
		}

		//public QueryResourceResponse QueryResource(string query, Workflow workflow) => Call(c => c.QueryResource(query), workflow);
		//public ResourceResponse FindResource(ResourceAddress query, Workflow workflow) => Call(c => c.FindResource(query), workflow);
	}
}
