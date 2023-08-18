using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Threading;
using DnsClient;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void VoidDelegate();
 	public delegate void CoreDelegate(Sun d);

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
		Seeder		= 0b00001000,
	}

	public class ReleaseStatus
	{
		public bool				ExistsRecursively { get; set; }
		public Manifest			Manifest { get; set; }
		public DownloadReport	Download { get; set; }
	}

	public class Sun : RdcInterface
	{
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
		public ResourceBase				Resources;
		public PackageBase				Packages;
		public Hub						Hub;
		public bool						IsClient => ListeningThread == null;
		public object					Lock = new();
		public Clock					Clock;

		public RocksDb					DatabaseEngine;
		public ColumnFamilyHandle		PeersFamily => DatabaseEngine.GetColumnFamily(nameof(Peers));
		readonly DbOptions				DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																			.SetCreateMissingColumnFamilies(true);

		public Guid						Nuid;
		public IPAddress				IP = IPAddress.None;

		public Statistics				PrevStatistics = new();
		public Statistics				Statistics = new();

		public List<Transaction>		IncomingTransactions = new();
		List<Transaction>				OutgoingTransactions = new();
		public List<Operation>			Operations	= new();
		public List<Analysis>			Analyses = new();

		bool							MinimalPeersReached;
		bool							OnlineBroadcasted;
		public List<Peer>				Peers		= new();
		public IEnumerable<Peer>		Connections	=> Peers.Where(i => i.Established);
		//public Peer[]					_Bases = new Peer[0];
		public IEnumerable<Peer>		Bases
										{
											get
											{
												//if(Connections.Count(i => i.BaseRank > 0) < Settings.Database.PeersMin)
												//{
												//	Connect(Role.Base, Settings.Database.PeersMin, Workflow);
												//}

												return Connections.Where(i => i.BaseRank > 0);
											}
										}

		public List<IPAddress>			IgnoredIPs	= new();
		//public List<Member>				Members = new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		Thread							TransactingThread;
		Thread							VerifingThread;
		Thread							SynchronizingThread;

		//public bool						Running { get; protected set; } = true;
		public Synchronization			_Synchronization = Synchronization.Null;
		public Synchronization			Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate				SynchronizationChanged;
		
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

		public CoreDelegate				MainStarted;
		public CoreDelegate				ApiStarted;

		
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
				f.Add(new ("    Pending Delegation",	$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.PendingDelegation)}"));
				f.Add(new ("    Accepted",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Accepted)}"));
				f.Add(new ("    Pending Placement",		$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Verified)}"));
				f.Add(new ("    Placed",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Placed)}"));
				f.Add(new ("    Confirmed",				$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Confirmed)}"));
				f.Add(new ("Peers in/out/min/known",	$"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}"));
				
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
	
						foreach(var i in Vault.Accounts)
						{
							f.Add(new ($"Account", $"{i.ToString().Insert(6, "-")} {formatbalance(i), BalanceWidth}"));
						}
	
						if(DevSettings.UI)
						{
							foreach(var i in Mcv.LastConfirmedRound.Funds)
							{
								f.Add(new ($"Fundable", $"{i.ToString().Insert(6, "-")} {formatbalance(i), BalanceWidth}"));
							}
						}
					}
				}
				else
				{
					//f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in Vault.Accounts)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				Statistics.Reset();
		
				return f;
			}
		}
		
		public Sun(Zone zone, Settings settings, Log log)
		{
			Zone = zone;
			Settings = settings;
			//Cryptography.Current = settings.Cryptography;

			Workflow = new Workflow(log);

			Directory.CreateDirectory(Settings.Profile);

			Workflow.Log?.Report(this, $"Ultranet Node/Client {Version}");
			Workflow.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(DevSettings.Any)
				Workflow.Log?.ReportWarning(this, $"Dev: {DevSettings.AsString}");

			Vault = new Vault(Zone, Settings, Workflow?.Log);

			var cfamilies = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{
																new (nameof(Peers),					new ()),
																new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (AuthorTable.MetaColumnName,	new ()),
																new (AuthorTable.MainColumnName,	new ()),
																new (AuthorTable.MoreColumnName,	new ()),
																new (Mcv.ChainFamilyName,		new ()),
															})
				cfamilies.Add(i);

			DatabaseEngine = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
		}

		public override string ToString()
		{
			var gens = Mcv?.LastConfirmedRound != null ? Settings.Generators.Where(i => Mcv.LastConfirmedRound.Members.Any(j => j.Account == i)) : new AccountKey[0];
	
			return	$"{(Settings.Roles.HasFlag(Role.Base) ? "B" : "")}" +
					$"{(Settings.Roles.HasFlag(Role.Chain) ? "C" : "")}" +
					$"{(Settings.Roles.HasFlag(Role.Seeder) ? "S" : "")}" +
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

		public void RunUser()
		{
			Nuid = Guid.NewGuid();

			if(Settings.Roles.HasFlag(Role.Seeder))
			{
				Resources = new ResourceBase(this, Zone, System.IO.Path.Join(Settings.Profile, typeof(ResourceBase).Name));
				Packages = new PackageBase(this, Resources, Settings.Packages);
			}

			LoadPeers();
			
 			MainThread = new Thread(() =>
 									{ 
										Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

										try
										{
											while(!Workflow.IsAborted)
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

			//if(waitconnections)
// 			{
// 				while(!MinimalPeersReached)
// 				{
// 					Workflow.ThrowIfAborted();
// 					Thread.Sleep(1);
// 				}
// 			}
		}

		public void RunNode()
		{
			Nuid = Guid.NewGuid();

			ListeningThread = new Thread(() =>	{
													try
													{
														Listening();
													}
													catch(OperationCanceledException)
													{
													}
												});
			ListeningThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Listening";

			if(Settings.Roles.HasFlag(Role.Base) && Settings.Generators.Any())
			{
				Hub = new Hub(this);
			}

			if(Settings.Roles.HasFlag(Role.Seeder))
			{
				Resources = new ResourceBase(this, Zone, System.IO.Path.Join(Settings.Profile, typeof(ResourceBase).Name));
				Packages = new PackageBase(this, Resources, Settings.Packages);
			}

			if(Settings.Roles.HasFlag(Role.Base) || Settings.Roles.HasFlag(Role.Chain))
			{
				Mcv = new Mcv(Zone, Settings.Roles, Settings.Mcv, Workflow?.Log, DatabaseEngine);
		
				//if(Database.LastConfirmedRound != null && Database.LastConfirmedRound.Members.FirstOrDefault().Generator == Zone.Father0)
				//{
				//	Members = new List<OnlineMember>{ new() {Generator = Zone.Father0, IPs = new[] {Zone.GenesisIP}}};
				//}

				Mcv.ConsensusConcluded += (r, reached) =>
												{
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
															i.Placing = PlacingStage.Verified;
														}
													}
												};

				Mcv.Confirmed += r =>	{
											IncomingTransactions.RemoveAll(t => t.Vote != null && t.Vote.Round.Id <= r.Id || t.Expiration <= r.Id);
											Analyses.RemoveAll(i => r.ConfirmedAnalyses.Any(j => j.Resource == i.Resource && j.Finished));
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

					VerifingThread = new Thread(Verifing);
					VerifingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Verifing";
					VerifingThread.Start();
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
											while(!Workflow.IsAborted)
											{
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
																GenerateChainbase();
															}
														}
													}
												}
	
												Thread.Sleep(1);
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
			if(Workflow.IsAborted)
				return;

			Workflow.Abort();
			Listener?.Stop();
			ApiServer?.Stop();

			lock(Lock)
			{
				foreach(var i in Peers.Where(i => i.Established))
					i.Disconnect();
			}

			ListeningThread?.Join();
			TransactingThread?.Join();

			DatabaseEngine?.Dispose();

			Workflow?.Log?.Report(this, "Stopped", message);
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
			using(var i = DatabaseEngine.NewIterator(PeersFamily))
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
	
				DatabaseEngine.Write(b);
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
			var needed = Settings.PeersMin - Peers.Count(i => i.Established || i.InStatus == EstablishingStatus.Initiated || i.OutStatus == EstablishingStatus.Initiated);
		
			foreach(var p in Zone.ChoosePeers(Peers.Where(m =>	(m.InStatus == EstablishingStatus.Null || m.InStatus == EstablishingStatus.Failed) &&
																(m.OutStatus == EstablishingStatus.Null || m.OutStatus == EstablishingStatus.Failed) && 
																DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
												    .OrderByDescending(i => i.PeerRank)
												    .ThenBy(i => i.Retries),
											  needed)
								.Take(needed))
			{
				p.LastTry = DateTime.UtcNow;
				p.Retries++;
		
				OutboundConnect(p);
			}

			foreach(var i in Peers.Where(i => i.Tcp != null && i.Status == ConnectionStatus.Failed))
				i.Disconnect();

			if(!MinimalPeersReached && 
				Connections.Count() >= Settings.PeersMin && 
				(!Settings.Roles.HasFlag(Role.Base) || Connections.Count(i => i.Roles.HasFlag(Role.Base)) >= Settings.Mcv.PeersMin))
			{
				if(Mcv != null)
				{
					StartSynchronization();
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
	
				while(!Workflow.IsAborted)
				{
					var c = Listener.AcceptTcpClient();

					if(Workflow.IsAborted)
						return;
	
					lock(Lock)
					{
						if(!Workflow.IsAborted && Connections.Count() < Settings.PeersInMax)
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
		}

		Hello CreateHello(IPAddress ip)
		{
			Peer[] peers;
		
			lock(Lock)
			{
				peers = Peers.Where(i => i.Fresh).ToArray();
			}

			var h = new Hello();

			h.Roles			= Settings.Roles;
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
			peer.OutStatus = EstablishingStatus.Initiated;

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
						Workflow.Log?.Report(this, "Establishing failed", $"To {peer.IP}; Connect; {ex.Message}" );
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
						Workflow.Log?.Report(this, "Establishing failed", $"To {peer.IP}; Send/Wait Hello; {ex.Message}" );
						goto failed;
					}
	
					lock(Lock)
					{
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
							Workflow.Log?.Report(this, "Establishing failed", "It's me");
							Peers.Remove(peer);
							client.Close();
							return;
						}
													
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, "Detected IP", IP.ToString());
						}
	
						if(peer.Established)
						{
							Workflow.Log?.Report(this, "Establishing failed", $"From {peer.IP}; Already established" );
							client.Close();
							return;
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
	
						peer.OutStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, $"{Settings.IP.GetAddressBytes()[3]}");
							
						Workflow.Log?.Report(this, "Connected", $"to {peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
						return;
					}
	
					failed:
						lock(Lock)
							peer.OutStatus = EstablishingStatus.Failed;
									
						client.Close();
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

			if(ip.Equals(IP) || IgnoredIPs.Any(j => j.Equals(ip)))
			{
				Peers.Remove(peer);
				client.Close();
				return;
			}

			if(peer != null)
			{
				peer.InStatus = EstablishingStatus.Initiated;
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
						Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; WaitHello {ex.Message}");
						goto failed;
					}
				
					lock(Lock)
					{
						if(Workflow.IsAborted)
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
							Workflow.Log?.Report(this, "Establishing failed", "It's me");
							Peers.Remove(peer);
							client.Close();
							return;
						}

						if(peer != null && peer.Established)
						{
							Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; Already established" );
							client.Close();
							return;
						}
	
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, "Reported IP", IP.ToString());
						}
		
						try
						{
							Peer.SendHello(client, CreateHello(ip));
						}
						catch(Exception ex) when(!DevSettings.ThrowOnCorrupted)
						{
							Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; SendHello; {ex.Message}");
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
	
						peer.InStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, $"{Settings.IP.GetAddressBytes()[3]}");
			
						Workflow.Log?.Report(this, "Accepted from", $"{peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
						return;
					}
	
				failed:
					lock(Lock)
						if(peer != null)
							peer.InStatus = EstablishingStatus.Failed;

					client.Close();
				}
				catch(Exception ex) when(!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
		}

		void StartSynchronization()
		{
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

			while(!Workflow.IsAborted)
			{
				try
				{
					Thread.Sleep(1000);

					int final = -1; 
					//int end = -1; 
					int from = -1;

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
		
						download<AccountEntry,	AccountAddress>(Mcv.Accounts);
						download<AuthorEntry, string>(Mcv.Authors);
		
						var r = new Round(Mcv){Id = stamp.FirstTailRound - 1, Hash = stamp.LastCommitedRoundHash, Confirmed = true};
		
						var rd = new BinaryReader(new MemoryStream(stamp.BaseState));
		
						rd.Read7BitEncodedInt();
						r.Hash			= rd.ReadSha3();
						r.ConfirmedTime	= rd.ReadTime();
						r.WeiSpent		= rd.ReadBigInteger();
						r.Factor		= rd.ReadCoin();
						r.Emission		= rd.ReadCoin();
						r.Members		= rd.Read<Member>(m => m.ReadForBase(rd)).ToList();
						r.Analyzers		= rd.Read<Analyzer>(m => m.ReadForBase(rd)).ToList();
						r.Funds			= rd.ReadList<AccountAddress>();
		
						Mcv.BaseState	= stamp.BaseState;
						//Database.Members	= r.Members;
						//Database.Funds		= r.Funds;
		
						Mcv.LastConfirmedRound = r;
						Mcv.LastCommittedRound = r;
		
						Mcv.Hashify();
		
						if(peer.GetStamp().BaseHash.SequenceEqual(Mcv.BaseHash))
 							Mcv.LoadedRounds.Add(r.Id, r);
						else
							throw new SynchronizationException();

						//Members = r.Members.Select(i => new OnlineMember {Generator = i.Generator, IPs = new IPAddress[]{} }).ToList();
					}
		
					while(!Workflow.IsAborted)
					{
						Thread.Sleep(1);
						Workflow.ThrowIfAborted();
		
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
						 	
						lock(Lock)
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
									if(confirmed && r.Confirmed)
									{
										foreach(var p in r.Payloads)
										{
											//p.Confirmed = true;

											foreach(var t in p.Transactions)
												t.Placing = PlacingStage.Placed;
										}
				
										Mcv.Tail.RemoveAll(i => i.Id == r.Id); /// remove old round with all its blocks
										Mcv.Tail.Add(r);
										Mcv.Tail = Mcv.Tail.OrderByDescending(i => i.Id).ToList();
		
										r.Confirmed = false;
										Mcv.Confirm(r, true);
//#if DEBUG
//										if(!r.Hash.SequenceEqual(h))
//										{
//											throw new SynchronizationException();
//										}
//#endif
									}
									else
									{
										if(!Mcv.Roles.HasFlag(Role.Chain))
										{ 
											var d = rp.LastConfirmedRound % Mcv.TailLength;
											
											if(d < Mcv.Pitch || Mcv.TailLength - d < Mcv.Pitch * 3)
												break;
										}

										if(confirmed)
										{
											confirmed		= false;
											final			= rounds.Max(i => i.Id) + 1;
											//end			= rp.LastNonEmptyRound;
											Synchronization	= Synchronization.Synchronizing;
										}
			
										if(r.Confirmed)
							 				throw new SynchronizationException();

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
	
								Workflow.Log?.Report(this, "Syncing finished");

								return;
							}
							else
								throw new SynchronizationException();
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

				if(v.Transactions.Any(i => !Valid(i) || !i.Valid(Mcv)))
					return false;

				Sun.SyncRound r;
						
				if(!SyncCache.TryGetValue(v.RoundId, out r))
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
				
				if(r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
					return false;

				try
				{
					Mcv.Add(v);
				}
				catch(ConfirmationException)
				{
					StartSynchronization();
				}

				if(v.Transactions.Any(i => !Valid(i) || !i.Valid(Mcv))) /// do it only after adding to the chainbase
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
											i.Expiration > Mcv.LastConfirmedRound.Id &&
											Settings.Generators.Any(g => g == i.Generator) &&
											i.Valid(Mcv)).ToList();
			
			foreach(var i in accepted)
				i.Placing = PlacingStage.Accepted;

			IncomingTransactions.AddRange(accepted);

			return accepted;
		}

		bool Valid(Transaction transaction)
		{
 			foreach(var i in transaction.Operations)
 			{
 				if(i is Emission e)
 				{
 					try
 					{
 						Monitor.Exit(Lock);
 			
 						if(Nas.CheckEmission(e))
 						{
 						}
 						else
 							return false;
 					}
 					catch(Exception ex)
 					{
 						Workflow.Log?.ReportError(this, "Can't verify Emission operation", ex);
 						return false;
 					}
 					finally
 					{
 						Monitor.Enter(Lock);
 					}
 				}
 	
 				if(i is AuthorBid b && b.Tld.Any() && Zone.CheckDomains)
 				{
 					try
 					{
 						Monitor.Exit(Lock);
 	
 						var result = Dns.QueryAsync(b.Name + '.' + b.Tld, QueryType.TXT, QueryClass.IN, Workflow.Cancellation.Token);
 															
 						var txt = result.Result.Answers.TxtRecords().FirstOrDefault(i => i.DomainName == b.Name + '.' + b.Tld + '.');
 		
 						if(txt != null && txt.Text.Any(i => i == transaction.Signer.ToString()))
 						{
 						}
 						else
 							return false;
 					}
 					catch(DnsResponseException ex)
 					{
 						Workflow.Log?.ReportError(this, "Can't verify AuthorBid domain", ex);
 						return false;
 					}
 					finally
 					{
 						Monitor.Enter(Lock);
 					}
 				}
 			}

			return true;
		}

		void Verifing()
		{
			Workflow.Log?.Report(this, "Verifing started");

			try
			{
				while(!Workflow.IsAborted)
				{
					Thread.Sleep(1);

					Statistics.Verifying.Begin();

					lock(Lock)
					{
						foreach(var t in IncomingTransactions.Where(i => i.Placing == PlacingStage.Accepted).ToArray())
						{
							if(Valid(t))
							{
								t.Placing = PlacingStage.Verified;
							} 
							else
							{
								IncomingTransactions.Remove(t);
							}
						}
					}

					Statistics.Verifying.End();
				}
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
			catch(OperationCanceledException)
			{
			}
		}

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

					var s = new MemoryStream(); 
					var w = new BinaryWriter(s);
					v.Sign(Zone, Settings.Analyzer);
					v.Write(w);
	
					foreach(var i in Analyses)
					{
						w.Write(i.Resource);
						w.Write((byte)i.Result);

						if(s.Position > Vote.SizeMax)
							break;
		
						a.Add(i);
					}
					
					v.Sign(Zone, Settings.Analyzer);

					foreach(var i in Bases)
					{
						i.Send(new AnalyzerVoxRequest {Analyses = a, RoundId = r.Id, Signature = v.Signature});
					}

					Workflow.Log?.Report(this, "AnalyzerVoxRequest generated", $"by {Settings.Analyzer}");
				}
			}

			Statistics.Generating.End();
		}

		void GenerateChainbase()
		{
			Statistics.Generating.Begin();

			var votes = new List<Vote>();

			foreach(var g in Settings.Generators)
			{
				if(!Mcv.MembersOf(Mcv.LastConfirmedRound.Id + 1 + Mcv.Pitch).Any(i => i.Account == g))
				{
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

					if(r.Parent == null || r.Parent.Payloads.Any(i => i.Hash == null)) /// cant refer to downloaded rounds since its blocks have no hashes
						continue;

					// i.Operations.Any() required because in Database.Confirm operations and transactions may be deleted
					var txs = Mcv.CollectValidTransactions(IncomingTransactions.Where(i => i.Generator == g && r.Id <= i.Expiration && i.Placing == PlacingStage.Verified), r).ToArray();

					var prev = r.Previous.VotesOfTry.FirstOrDefault(i => i.Generator == g);

					Vote createvote()
					{
						return new Vote(Mcv){	RoundId				= r.Id,
													Try					= r.Try,
													ParentSummary		= Mcv.Summarize(r.Parent),
													Created				= Clock.Now,
													TimeDelta			= prev == null || prev.RoundId <= Mcv.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Created).TotalMilliseconds,
													Violators			= Mcv.ProposeViolators(r).ToList(),
													MemberJoiners		= Mcv.ProposeMemberJoiners(r).ToList(),
													MemberLeavers		= Mcv.ProposeMemberLeavers(r, g).ToList(),
													//HubJoiners			= Database.ProposeHubJoiners(r).ToList(),
													//HubLeavers			= Database.ProposeHubLeavers(r, g).ToList(),
													AnalyzerJoiners		= Settings.ProposedAnalyzerJoiners,
													AnalyzerLeavers		= Settings.ProposedAnalyzerLeavers,
													FundJoiners			= Settings.ProposedFundJoiners,
													FundLeavers			= Settings.ProposedFundLeavers,
													BaseIPs				= Settings.Anonymous ? new IPAddress[] {} : new IPAddress[] {IP},
													HubIPs				= new IPAddress[] {IP} };
					}
	
					if(txs.Any() || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any())) /// any pending foreign transactions or any our pending operations OR some unconfirmed payload 
					{
						var b = createvote();

						if(txs.Any())
						{
							var s = new MemoryStream(); 
							var w = new BinaryWriter(s);
							b.Sign(g);
							b.WriteForBroadcast(w);
	
							foreach(var i in txs)
							{
								i.WriteForVote(w);
	
								if(s.Position > Vote.SizeMax)
									break;
	
								b.AddNext(i);
		
								i.Placing = PlacingStage.Placed;
							}
						}
						
						b.Sign(g);
						votes.Add(b);
					}

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
					StartSynchronization();
				}


// 				var pieces = new List<BlockPiece>();
// 
// 				var npieces = 1;
// 				var ppp = 3; // peers per peice
// 
// 				foreach(var b in blocks)
// 				{
// 					var s = new MemoryStream();
// 					var w = new BinaryWriter(s);
// 
// 					//w.Write(b.TypeCode);
// 					b.WriteForPiece(w);
// 
// 					s.Position = 0;
// 					var r = new BinaryReader(s);
// 
// 					//var guid = new byte[BlockPiece.GuidLength];
// 					//Cryptography.Random.NextBytes(guid);
// 
// 					for(int i = 0; i < npieces; i++)
// 					{
// 						var p = new BlockPiece(Zone){	Type = b.Type,
// 														Try = b is Vote v ? v.Try : 0,
// 														RoundId = b.RoundId,
// 														Total = npieces,
// 														Index = i,
// 														Data = r.ReadBytes((int)s.Length/npieces + (i < npieces-1 ? 0 : (int)s.Length % npieces))};
// 
// 						p.Sign(b.Generator as AccountKey);
// 
// 						pieces.Add(p);
// 						Database.GetRound(b.RoundId).BlockPieces.Add(p);
// 					}
// 				}
// 
// 				IEnumerator<Peer> start() => Bases.GetEnumerator();
// 
//  				var c = start();
//  
//  				foreach(var i in pieces)
//  				{
// 					i.Peers = new();   
// 
// 					for(int j = 0; j < ppp; j++)
// 					{
//  						if(!c.MoveNext())
//  						{
//  							c = start(); /// go to the beginning
//  							c.MoveNext();
//  						}
// 
// 						i.Peers.Add(c.Current);
// 					}
//  				}

				
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
			Operation[]					next;
			bool						ready;
			IEnumerable<Transaction>	accepted;

			Workflow.Log?.Report(this, "Delegating started");

			RdcInterface rdi = null;
			AccountAddress m = null;

			while(Workflow.Active)
			{
				lock(Lock)
				{
					if(!Operations.Any())
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
							var cr = Call<MembersResponse>(Role.Base, i => i.GetMembers(), Workflow);
	
							if(!cr.Members.Any())
								continue;

							lock(Lock)
							{
								var members = cr.Members;

								//Members.RemoveAll(i => !members.Contains(i.Generator));
																										
								foreach(var i in members.Where(i => i.HubIPs.Any()).OrderByRandom()) /// look for public IP in connections
								{
									var p = Connections.OrderByRandom().FirstOrDefault(j => i.HubIPs.Any(ip => j.IP.Equals(ip)));

									if(p != null)
									{
										rdi = p;
										m = i.Account;
										Workflow.Log?.Report(this, "Generator direct connection established", $"{i} {p}");
										break;
									}
								}

								if(rdi != null)
									break;

								foreach(var i in members.Where(i => i.HubIPs.Any()).OrderByRandom()) /// try by public IP address
								{
									var ip = i.HubIPs.Random();
									var p = GetPeer(ip);

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
									
								foreach(var i in members.Where(i => i.Proxyable).OrderByRandom()) /// connect via random proxy
								{
									try
									{
										Monitor.Exit(Lock);

										//Connect(i.Proxy, Workflow);
										m = i.Account;
										rdi = new ProxyRdi(m, cr.Peer);
										Workflow.Log?.Report(this, "Generator proxy connection established", $"{m} {cr.Peer}");
										break;
									}
									catch(Exception)
									{
									}
									finally
									{
										Monitor.Enter(Lock);
									}
								}

								if(rdi != null)
									break;

								try
								{
									Monitor.Exit(Lock);

									var p = GetPeer(Zone.GenesisIP);
									Connect(p, Workflow);
									m = Zone.Father0;
									rdi = p;
									Workflow.Log?.Report(this, "Generator proxy connection established", $"{m} {p}");
								}
								catch(Exception)
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

				lock(Lock)
				{
					if(rdi == this && Synchronization != Synchronization.Synchronized)
						continue;

					next = Operations.Where(i => i.Transaction == null).ToArray();
					ready = next.Any() && !OutgoingTransactions.Any(i => i.Placing >= PlacingStage.Accepted);
				}

				if(ready) /// Any pending ops and no delegated cause we first need to recieve a valid block to keep tx id sequential correctly
				{
					var txs = new List<Transaction>();

					lock(Lock)
					{
						foreach(var g in next.GroupBy(i => i.Signer))
						{
							Monitor.Exit(Lock);

							var at = Call<AllocateTransactionResponse>(Role.Base, i => i.Request<AllocateTransactionResponse>(new AllocateTransactionRequest {Account = g.Key}), Workflow);
									
							Monitor.Enter(Lock);

							var t = new Transaction(Zone);
							t.Id = at.NextTransactionId;

							foreach(var o in g)
							{
								t.AddOperation(o);
							}

							t.Sign(Vault.GetKey(g.Key), m, at.MaxRoundId, at.PowHash);
							txs.Add(t);
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

						OutgoingTransactions.AddRange(atxs);

						foreach(var i in txs.Where(t => !atxs.Contains(t)))
							foreach(var o in i.Operations)
								o.Transaction = null;

					}
					
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
											if(t.Operations.Any(o => o.__ExpectedPlacing != PlacingStage.Null && o.__ExpectedPlacing != i.Placing))
											{	
												rdi.GetTransactionStatus(accepted.Select(i => new TransactionsAddress{Account = i.Signer, Id = i.Id}));
												Debugger.Break();
											}
										#endif

										OutgoingTransactions.Remove(t);

										foreach(var o in t.Operations)
										{
											Operations.Remove(o);
										}
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

		void Enqueue(Operation o)
		{
			if(Operations.Count <= OperationsQueueLimit)
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
															catch(Exception ex) when (!Debugger.IsAttached)
															{
																Stop(MethodBase.GetCurrentMethod(), ex);
															}
														});

					TransactingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Transacting";
					TransactingThread.Start();
				}

				//o.Placing = PlacingStage.PendingDelegation;
				Operations.Add(o);
			} 
			else
			{
				Workflow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
		public Operation Enqueue(Operation operation, PlacingStage waitstage, Workflow workflow)
		{
			operation.__ExpectedPlacing = waitstage;

			if(FeeAsker.Ask(this, operation.Signer as AccountKey, operation))
			{
				lock(Lock)
				 	Enqueue(operation);

				Await(operation, waitstage, workflow);

				return operation;
			}
			else
				return null;
		}
		
		public void Enqueue(IEnumerable<Operation> operations, PlacingStage waitstage, Workflow workflow)
		{
			lock(Lock)
				foreach(var i in operations)
				{
					i.__ExpectedPlacing = waitstage;

					if(FeeAsker.Ask(this, i.Signer as AccountKey, i))
					{
				 		Enqueue(i);
					}
				}

			while(workflow.Active)
			{ 
				Thread.Sleep(100);

				switch(waitstage)
				{
					case PlacingStage.Null :				return;
					case PlacingStage.Accepted :			if(operations.All(o => o.Transaction.Placing >= PlacingStage.Accepted))			return; else break;
					case PlacingStage.Placed :				if(operations.All(o => o.Transaction.Placing >= PlacingStage.Placed))			return; else break;
					case PlacingStage.Confirmed :			if(operations.All(o => o.Transaction.Placing == PlacingStage.Confirmed))		return; else break;
					case PlacingStage.FailedOrNotFound :	if(operations.All(o => o.Transaction.Placing == PlacingStage.FailedOrNotFound)) return; else break;
				}
			}
		}

		void Await(Operation o, PlacingStage s, Workflow workflow)
		{
			while(workflow.Active)
			{ 
				Thread.Sleep(100);

				if(o.Transaction == null)
					continue;

				switch(s)
				{
					case PlacingStage.Null :				return;
					case PlacingStage.Accepted :			if(o.Transaction.Placing >= PlacingStage.Accepted) return; else break;
					case PlacingStage.Placed :				if(o.Transaction.Placing >= PlacingStage.Placed) return; else break;
					case PlacingStage.Confirmed :			if(o.Transaction.Placing == PlacingStage.Confirmed) return; else break;
					case PlacingStage.FailedOrNotFound :	if(o.Transaction.Placing == PlacingStage.FailedOrNotFound) return; else break;
				}
			}
		}

		public Peer ChooseBestPeer(Role role, HashSet<Peer> exclusions)
		{
			return Peers.Where(i => i.GetRank(role) > 0 && (exclusions == null || !exclusions.Contains(i))).OrderByDescending(i => i.Established)
																											.ThenBy(i => i.GetRank(role))
																											//.ThenByDescending(i => i.ReachFailures)
																											.FirstOrDefault();
		}

		public Peer Connect(Role role, HashSet<Peer> exclusions, Workflow workflow)
		{
			Peer peer;
				
			while(true)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
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
				if(!peer.Established && peer.InStatus != EstablishingStatus.Initiated && peer.OutStatus != EstablishingStatus.Initiated)
				{
					peer.LastTry = DateTime.UtcNow;
					peer.Retries++;
			
					OutboundConnect(peer);
				}
			}

			var t = DateTime.Now;

			while(workflow.Active)
			{
				Thread.Sleep(1);

				lock(Lock)
					if(peer.Established)
						return;
					else if(peer.OutStatus == EstablishingStatus.Failed)
						throw new ConnectionFailedException("Failed");

				if(!DevSettings.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new ConnectionFailedException("Timed out");
			}
		}


		public R Call<R>(Role role, Func<Peer, R> call, Workflow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					p = ChooseBestPeer(role, tried);
	
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
					p.LastFailure[role] = DateTime.UtcNow;
				}
 				catch(RdcNodeException)
 				{
					p.LastFailure[role] = DateTime.UtcNow;
 				}
			}

			throw new OperationCanceledException();
		}

		public void Call(Role role, Action<Peer> call, Workflow workflow, IEnumerable<Peer> exclusions = null, bool exitifnomore = false)
		{
			var excl = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(!workflow.IsAborted)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					p = ChooseBestPeer(role, excl);
	
					if(p == null)
						if(exitifnomore)
							return;
						else
							continue;
				}

				excl?.Add(p);

				try
				{
					Connect(p, workflow);

					call(p);

					break;
				}
				catch(ConnectionFailedException)
				{
					p.LastFailure[role] = DateTime.UtcNow;
				}
				catch(RdcNodeException)
				{
					p.LastFailure[role] = DateTime.UtcNow;
				}
			}
		}

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

		public Coin EstimateFee(IEnumerable<Operation> operations)
		{
			return Mcv != null ? Operation.CalculateTransactionFee(Mcv.LastConfirmedRound.Factor, operations) : Coin.Zero;
		}

		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, AccountKey signer, PlacingStage awaitstage, Workflow workflow)
		{
			var	l = Call(Role.Base, p =>{
											try
											{
												return p.GetAccountInfo(signer);
											}
											catch(RdcEntityException ex) when (ex.Error == RdcEntityError.AccountNotFound)
											{
												return new AccountResponse();
											}
										}, 
										workflow);
			
			var eid = l.Account == null ? 0 : l.Account.LastEmissionId + 1;

			Nas.Emit(a, wei, signer, GasAsker, eid, workflow);		
						
			var o = new Emission(signer, wei, eid);


			//flow?.SetOperation(o);
						
			if(FeeAsker.Ask(this, signer, o))
			{
				Enqueue(o, awaitstage, workflow);
	
				workflow?.Log?.Report(this, "State changed", $"{o} is queued for placing and confirmation");
						
				return o;
			}

			return null;
		}

		public Emission FinishTransfer(AccountKey signer, Workflow workflow = null)
		{
			lock(Lock)
			{
				var	l = Call(Role.Base, p =>{
												try
												{
													return p.GetAccountInfo(signer);
												}
												catch(RdcEntityException ex) when (ex.Error == RdcEntityError.AccountNotFound)
												{
													return new AccountResponse();
												}
											}, 
											workflow);
			
				var eid = l.Account == null ? 0 : l.Account.LastEmissionId + 1;

				var wei = Nas.FinishEmission(signer, eid);

				if(wei == 0)
					throw new RequirementException("No corresponding Ethrereum transaction found");

				var o = new Emission(signer, wei, eid);

				if(FeeAsker.Ask(this, signer, o))
				{
					Enqueue(o);
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
	}
}
