using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin.Secp256k1;
using Nethereum.Signer;
using Nethereum.Web3;
using Org.BouncyCastle.Asn1.X509;
using RocksDbSharp;
using static UC.Net.Download;

namespace UC.Net
{
	public delegate void VoidDelegate();
 	public delegate void CoreDelegate(Core d);

	public class Statistics
	{
		public PerformanceMeter Consensing = new();
		public PerformanceMeter Generating = new();
		public PerformanceMeter Delegating = new();
		public PerformanceMeter BlocksProcessing = new();
		public PerformanceMeter TransactionsProcessing = new();

		public void Reset()
		{
			Consensing.Reset();
			Generating.Reset();
			Delegating.Reset();
			BlocksProcessing.Reset();
			TransactionsProcessing.Reset();
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
		Chain	= 0b00000001,
		Base	= 0b00000010,
		Seed	= 0b00000100,
		Hub		= 0b00001000,
	}

	public class ReleaseInfo
	{
		public Manifest			Manifest { get; set; }
		public DownloadStatus	Download { get; set; }
	}

	public class Core : Dci
	{
		public System.Version							Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]					Versions = {1};
		public const string								FailureExt = "failure";
		public const int								Timeout = 5000;
		public const int								OperationsQueueLimit = 1000;
		const int										BalanceWidth = 24;

		//public Account[]								Generators;

		//public Log										Log;
		public Workflow									Workflow;
		public Vault									Vault;
		public INas										Nas;
		public Database									Database;
		public Filebase									Filebase;
		public Hub										Hub;
		RocksDb											DatabaseEngine;
		public bool										Networking => DelegatingThread != null;
		public bool										IsClient => Networking && ListeningThread == null;
		public object									Lock = new();
		public Settings									Settings;
		public Clock									Clock;

		public Guid										Nuid;
		public IPAddress								IP = IPAddress.None;

		public Statistics								PrevStatistics = new();
		public Statistics								Statistics = new();

		public List<Transaction>						Transactions = new();
		public List<Operation>							Operations	= new();

		bool											MinimalPeersReached;
		public List<Peer>								Peers		= new();
		public IEnumerable<Peer>						Connections	=> Peers.Where(i => i.Established);
		public List<IPAddress>							IgnoredIPs	= new();
		public List<Block>								BlockCache	= new();
		public List<BlockPiece>							BlockPieceCache	= new();
		public List<Member>								Members		= new();
		public List<Download>							Downloads	= new();

		TcpListener										Listener;
		Thread											ListeningThread;
		Thread											DelegatingThread;
		Thread											VerifingThread;
		Thread											SynchronizingThread;
		Thread											DeclaringThread;

		JsonServer										ApiServer;

		public bool										Running { get; protected set; } = true;
		public Synchronization							_Synchronization = Synchronization.Null;
		public Synchronization							Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate								SynchronizationChanged;
		//DateTime										SyncRequested;

		public IGasAsker								GasAsker; 
		public IFeeAsker								FeeAsker;

		public ColumnFamilyHandle						PeersFamily => DatabaseEngine.GetColumnFamily(nameof(Peers));

		readonly DbOptions								DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																							.SetCreateMissingColumnFamilies(true);
		
		Func<Type, object>								Constractor => t => t == typeof(Transaction) ? new Transaction(Settings) : null;
		
		public string[][] Info
		{
			get
			{
				List<string> f = new();
				List<string> v = new(); 
															
				f.Add("Version");					v.Add(Version.ToString());
				f.Add("Zone");						v.Add(Settings.Zone.Name);
				f.Add("Profile");					v.Add(Settings.Profile);
				f.Add("IP(Reported):Port");			v.Add($"{Settings.IP} ({IP}) : {Settings.Port}");
				//f.Add($"Generator{(Nci != null ? " (delegation)" : "")}");	v.Add($"{(Generator ?? Nci?.Generator)}");
				f.Add("Operations");				v.Add($"{Operations.Count}");
				f.Add("    Pending Delegation");	v.Add($"{Operations.Count(i => i.Placing == PlacingStage.PendingDelegation)}");
				f.Add("    Accepted");				v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Accepted)}");
				f.Add("    Pending Placement");		v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Verified)}");
				f.Add("    Placed");				v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Placed)}");
				f.Add("    Confirmed");				v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Confirmed)}");
				f.Add("Peers in/out/min/known");	v.Add($"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}");
				
				if(Database != null)
				{
					f.Add("Synchronization");		v.Add($"{Synchronization}");
					f.Add("Size");					v.Add($"{Database.Size}");
					f.Add("Members");				v.Add($"{Database.Members.Count}");
					f.Add("Emission");				v.Add($"{(Database.LastPayloadRound != null ? Database.LastPayloadRound.Emission.ToHumanString() : null)}");
					f.Add("Cached Blocks");			v.Add($"{BlockCache.Count()}");
					f.Add("Cached Rounds");			v.Add($"{Database.LoadedRounds.Count()}");
					f.Add("Last Non-Empty Round");	v.Add($"{(Database.LastNonEmptyRound != null ? Database.LastNonEmptyRound.Id : null)}");
					f.Add("Last Payload Round");	v.Add($"{(Database.LastPayloadRound != null ? Database.LastPayloadRound.Id : null)}");
					f.Add("Generating (μs)");		v.Add((Statistics.Generating.Avarage.Ticks/10).ToString());
					f.Add("Consensing (μs)");		v.Add((Statistics.Consensing.Avarage.Ticks/10).ToString());
					//f.Add("Delegating (μs)");		v.Add((Statistics.Delegating.Avarage.Ticks/10).ToString());
					f.Add("Block Processing (μs)");	v.Add((Statistics.BlocksProcessing.Avarage.Ticks/10).ToString());
					f.Add("Tx Processing (μs)");	v.Add((Statistics.TransactionsProcessing.Avarage.Ticks/10).ToString());
					f.Add("NAS Eth Account");		v.Add($"{Nas.Account?.Address}");

					if(Synchronization == Synchronization.Synchronized)
					{
						string formatbalance(Account a, bool confirmed)
						{
							return Database.GetAccountInfo(a, confirmed)?.Balance.ToHumanString();
						}
	
						foreach(var i in Vault.Accounts)
						{
							f.Add($"Account");	v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
						}
	
						if(Settings.Dev.UI)
						{
							foreach(var i in Database.Funds)
							{
								f.Add($"Fundable");	v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
							}
						
	// 						if(Settings.Secret != null)
	// 						{
	// 							foreach(var i in  Settings.Secret.Fathers)
	// 							{
	// 								f.Add($"Father"); v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true),BalanceWidth}");
	// 							}
	// 						}
						}
					}
				}
				else
				{
					f.Add("Members (retrieved)");	v.Add($"{Members.Count}");

					foreach(var i in Vault.Accounts)
					{
						f.Add($"Account"); v.Add($"{i}");
					}
				}

				Statistics.Reset();
		
				return new [] {f.ToArray(), v.ToArray()};
			}
		}
		
		//public Header Header
		//{
		//	get
		//	{
		//		Header h;
		//
		//		lock(Lock)
		//		{
		//			h =	new Header
		//				{ 
		//					LastNonEmptyRound	= Database?.LastNonEmptyRound == null ? -1 : Database.LastNonEmptyRound.Id,
		//					LastConfirmedRound	= Database?.LastConfirmedRound == null ? -1 : Database.LastConfirmedRound.Id,
		//					BaseHash			= Database?.BaseHash == null ? Cryptography.ZeroHash : Database.BaseHash
		//				};
		//		}
		//
		//		return h;
		//	}
		//}

		public Core(Settings settings, string exedirectory, Log log)
		{
			Settings = settings;
			Cryptography.Current = settings.Cryptography;

			Workflow = new Workflow(log);

			Directory.CreateDirectory(Settings.Profile);

			Workflow?.Log?.Report(this, $"Ultranet Node/Client {Version}");
			Workflow?.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow?.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow?.Log?.Report(this, $"Zone: {Settings.Zone.Name}");
			Workflow?.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(Settings.Dev.Any)
				Workflow?.Log?.ReportWarning(this, $"Dev: {Settings.Dev}");

			Vault = new Vault(Settings, Workflow?.Log);

			var cfamilies = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{
																new (nameof(Peers), new ()),
																new (AccountTable.MetaColumnName, new ()),
																new (AccountTable.MainColumnName, new ()),
																new (AccountTable.MoreColumnName, new ()),
																new (AuthorTable.MetaColumnName, new ()),
																new (AuthorTable.MainColumnName, new ()),
																new (AuthorTable.MoreColumnName, new ()),
																new (ProductTable.MetaColumnName, new ()),
																new (ProductTable.MainColumnName, new ()),
																new (ProductTable.MoreColumnName, new ()),
																new (RealizationTable.MetaColumnName, new ()),
																new (RealizationTable.MainColumnName, new ()),
																new (RealizationTable.MoreColumnName, new ()),
																new (ReleaseTable.MetaColumnName, new ()),
																new (ReleaseTable.MainColumnName, new ()),
																new (ReleaseTable.MoreColumnName, new ()),
																new (nameof(Net.Database.Rounds), new ()),
																new (nameof(Net.Database.Funds), new ()),
															})
				cfamilies.Add(i);

			DatabaseEngine = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
		}

		public override string ToString()
		{
			return $"{Settings.IP} {Connections.Count()}/{Settings.PeersMin} {Synchronization}";
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
		}

		public void RunClient()
		{
			Nuid = Guid.NewGuid();

			if(Settings.Filebase.Enabled)
			{
				Filebase = new Filebase(Settings);
			}

			LoadPeers();

			DelegatingThread = new Thread(() => { 
													try
													{
														Delegating();
													}
													catch(OperationCanceledException)
													{
													}
												});
			DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
			DelegatingThread.Start();
								
 			var t = new Thread(	() =>
 								{ 
									Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

									try
									{
										while(Running)
										{
											lock(Lock)
											{
												if(!Running)
													break;

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
			t.Start();

			while(!MinimalPeersReached)
			{
				Workflow.ThrowIfAborted();
				Thread.Sleep(1);
			}
		}

		public void RunNode()
		{
			Nuid = Guid.NewGuid();

			if(Settings.Hub.Enabled)
			{
				Hub = new Hub(this);
			}

			if(Settings.Filebase.Enabled)
			{
				Filebase = new Filebase(Settings);
			}

			if(Settings.Database.Base || Settings.Database.Chain)
			{
				Database = new Database(Settings, Workflow?.Log, Nas, Vault, DatabaseEngine);
		
				Database.BlockAdded += b =>	ReachConsensus();
		
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

			//overridefinal?.Invoke(Settings, Vault);

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
			ListeningThread.Start();

			DelegatingThread = new Thread(() =>	{ 
													try
													{
														Delegating();
													}
													catch(OperationCanceledException)
													{
													}
												});
			DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
			DelegatingThread.Start();

					
 			var t = new Thread(	() =>
 								{ 
									Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

									try
									{
										while(Running)
										{
											lock(Lock)
											{
												if(!Running)
													break;

												ProcessConnectivity();
												
												if(Database != null)
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
			t.Start();
		}

		public void Stop(MethodBase methodBase, Exception ex)
		{
			lock(Lock)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(methodBase.Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings.Profile, m + "." + Core.FailureExt), ex.ToString());
				Workflow?.Log?.ReportError(this, m, ex);
	
				Stop("Exception");
			}
		}

		public void Stop(string message)
		{
			if(!Running)
				return;

			Running = false;
			Workflow.Abort();
			Listener?.Stop();
			ApiServer?.Stop();

			lock(Lock)
			{
				foreach(var i in Peers.Where(i => i.Established))
					i.Disconnect();
			}

			ListeningThread?.Join();
			DelegatingThread?.Join();

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
				while(Running)
				{
					var initials = Nas.GetInitials(Settings.Zone);

					if(initials.Any())
					{
						RememberPeers(initials.Select(i => new Peer(i){LastSeen = DateTime.UtcNow}));

						Workflow?.Log?.Report(this, "Initial nodes retrieved", initials.Length.ToString());
						break;
					}
					else
						throw new RequirementException($"No initial peers found for zone '{Settings.Zone}'");

				}
			}
		}

		public void RememberPeers(IEnumerable<Peer> peers)
		{
			lock(Lock)
			{
				var tosave = new List<Peer>();
													
				foreach(var i in peers)
				{
					var p = Peers.Find(j => j.IP.Equals(i.IP));
					
					if(p == null)
					{
						Peers.Add(i);
						tosave.Add(i);
					}
					else
					{
						bool c = p.ChainRank == 0 && i.ChainRank > 0;
						bool h = p.HubRank == 0 && i.HubRank > 0;
						bool s = p.SeedRank == 0 && i.SeedRank > 0;

						if(c || h || s)
						{
							if(c)
								p.ChainRank = 1;

							if(h)
								p.HubRank = 1;

							if(s)
								p.SeedRank = 1;
						
							tosave.Add(p);
						}
					}
				}
	
				UpdatePeers(tosave.Where(i => !i.IP.Equals(IP)));
			}
		}

		public void UpdatePeers(IEnumerable<Peer> peers)
		{
			lock(Lock)
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
		}

		void ProcessConnectivity()
		{
			var needed = Settings.PeersMin - Peers.Count(i => i.Established || i.InStatus == EstablishingStatus.Initiated || i.OutStatus == EstablishingStatus.Initiated);
		
			foreach(var p in Peers	.Where(m =>	(m.InStatus == EstablishingStatus.Null || m.InStatus == EstablishingStatus.Failed) &&
												(m.OutStatus == EstablishingStatus.Null || m.OutStatus == EstablishingStatus.Failed) && 
												DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
									.OrderBy(i => i.Retries)
									.ThenByDescending(i => i.PeerRank)
									.Take(needed))
			{
				p.LastTry = DateTime.UtcNow;
				p.Retries++;
		
				OutboundConnect(p);
			}

			foreach(var i in Peers.Where(i => i.Client != null && i.Status == ConnectionStatus.Failed))
				i.Disconnect();

			if(!MinimalPeersReached && 
				Connections.Count() >= Settings.PeersMin && 
				(Database == null || Connections.Count(i => i.Roles.HasFlag(Database.Roles)) >= Settings.Database.PeersMin))
			{
				if(Filebase != null && !IsClient)
				{
					DeclaringThread = new Thread(Declaring);
					DeclaringThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Declaring";
					DeclaringThread.Start();
				}

				if(Database != null)
				{
					StartSynchronization();
				}

				MinimalPeersReached = true;
			}
		}

		void Listening()
		{
			try
			{
				Listener = new TcpListener(Settings.IP, Settings.Port);
				Listener.Start();
	
				Workflow?.Log?.Report(this, "Listening started", $"{Settings.IP}:{Settings.Port}");

				while(Running)
				{
					var client = Listener.AcceptTcpClient();
	
					lock(Lock)
					{
						if(Connections.Count() < Settings.PeersInMax)
							InboundConnect(client);
						else
							client.Close();
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
				peers = Peers.ToArray();
			}

			var h = new Hello();

			h.Roles					= (Settings.Database.Base ? Role.Base : 0) |
									  (Settings.Database.Chain ? Role.Chain : 0) |
									  (Settings.Filebase.Enabled ? Role.Seed : 0) |
									  (Settings.Hub.Enabled ? Role.Hub : 0);
			h.Versions				= Versions;
			h.Zone					= Settings.Zone.Name;
			h.IP					= ip;
			h.Nuid					= Nuid;
			h.Peers					= peers;
			//h.LastRound				= Header.LastNonEmptyRound;
			//h.LastConfirmedRound	= Header.LastConfirmedRound; 
			
			return h;
		}

		void OutboundConnect(Peer peer)
		{
			peer.OutStatus = EstablishingStatus.Initiated;

			var t = new Thread(a =>
			{
				try
				{
					var client = new TcpClient(new IPEndPoint(Settings.IP, 0));

					try
					{
						client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
						//client.ReceiveTimeout = Timeout;
						client.Connect(peer.IP, Settings.Port);
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
						client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
						client.ReceiveTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;

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
	
						RememberPeers(h.Peers);
	
						peer.OutStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, Listening, Lock, $"{Settings.IP.GetAddressBytes()[3]}");
							
						Workflow.Log?.Report(this, "Connected to", $"{peer}");
	
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
			});
			
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
						client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
						client.ReceiveTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;

						h = Peer.WaitHello(client);
					}
					catch(Exception ex) when(!Settings.Dev.ThrowOnCorrupted)
					{
						Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; WaitHello {ex.Message}");
						goto failed;
					}
				
					lock(Lock)
					{
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
						catch(Exception ex) when(!Settings.Dev.ThrowOnCorrupted)
						{
							Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; SendHello; {ex.Message}");
							goto failed;
						}
	
						if(peer == null)
						{
							peer = new Peer(ip);
							Peers.Add(peer);
						}
									
						RememberPeers(h.Peers.Append(peer));
	
						peer.InStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, Listening, Lock, $"{Settings.IP.GetAddressBytes()[3]}");
			
						Workflow.Log?.Report(this, "Accepted from", $"{peer}");
	
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

		void Listening(Peer peer)
		{
	 		try
	 		{
				while(peer.Established)
				{
					var pk = peer.Read();

					if(pk == null)
					{
						lock(Lock)
							peer.Status = ConnectionStatus.Failed;
						return;
					}

					lock(Lock)
						if(!Running || !peer.Established)
							return;
					
					var reader = new BinaryReader(pk.Data); 

					switch(pk.Type)
					{
						//case PacketType.Blocks:
						//{
						//	IEnumerable<Block> blocks;
						//				
						//	try
						//	{
						//		blocks = pk.Read((r, c) => Block.FromType(Database, (BlockType)c));
						//	}
						//	catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
						//	{
						//		peer.Disconnect();
						//		break;
						//	}
						//
						//	lock(Lock)
						//	{
						//		ProcessIncoming(blocks, peer);
						//	}
						//
						//	break;
						//}

 						case PacketType.Request:
 						{
							Request[] requests;

 							try
 							{
								requests = BinarySerializator.Deserialize(	reader,	
																			c => {
																					var o = UC.Net.Request.FromType(Database, (DistributedCall)c); 
																					o.Peer = peer; 
																					return o;
																				},
																			Constractor);

 							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								peer.Disconnect();
 								break;
 							}

							lock(peer.InRequests)
 								peer.InRequests.AddRange(requests);
 	
 							break;
 						}

						case PacketType.Response:
 						{
							Response[] responses;
							
							try
 							{
								//responses = Read(pk.Data, (r, t) => Response.FromType(Chain, (RpcType)t));
								responses = BinarySerializator.Deserialize(	reader,
																			t => UC.Net.Response.FromType(Database, (DistributedCall)t), 
																			Constractor
																			);
							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								peer.Disconnect();
 								break;
 							}

							lock(peer.OutRequests)
								foreach(var rp in responses)
								{
									var rq = peer.OutRequests.Find(j => j.Id.SequenceEqual(rp.Id));

									if(rq != null)
									{
										rp.Peer = peer;
										rq.Response = rp;
										rq.Event.Set();
 									
										if(rp.Final)
										{
											peer.OutRequests.Remove(rq);
										}
									}
								}
							break;
						}

						default:
							Workflow.Log?.ReportError(this, $"Wrong packet type {pk.Type}");
							peer.Status = ConnectionStatus.Failed;
							return;
					}
				}
	 		}
			catch(Exception) when (!Debugger.IsAttached)
			{
				peer.Disconnect();
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

			while(true)
			{
				try
				{
					int final = -1; 
					//int end = -1; 
					int from = -1;

					Database.LoadedRounds.Clear();

					var peer = Connect(Database.Roles, used, Workflow);

					if(Database.Roles.HasFlag(Role.Base) && !Database.Roles.HasFlag(Role.Chain))
					{
						Workflow.ThrowIfAborted();

						Database.Rounds.Clear();
		
						stamp = peer.GetStamp();
		
						void download<E, K>(Table<E, K> t) where E : TableEntry<K>
						{
							var ts = peer.GetTableStamp(t.Type, (t.Type switch
																		{ 
																			Tables.Accounts		=> stamp.Accounts.Where(i =>{
																																var c = Database.Accounts.SuperClusters.ContainsKey(i.Id);
																																return !c || !Database.Accounts.SuperClusters[i.Id].SequenceEqual(i.Hash);
																															}),
																			Tables.Authors		=> stamp.Authors.Where(i =>	{
																																var c = Database.Authors.SuperClusters.ContainsKey(i.Id);
																																return !c || !Database.Authors.SuperClusters[i.Id].SequenceEqual(i.Hash);
																															}),
																			Tables.Products		=> stamp.Products.Where(i =>{
																																var c = Database.Products.SuperClusters.ContainsKey(i.Id);
																																return !c || !Database.Products.SuperClusters[i.Id].SequenceEqual(i.Hash);
																															}),
																			Tables.Realizations => stamp.Realizations.Where(i =>
																															{
																																var c = Database.Realizations.SuperClusters.ContainsKey(i.Id);
																																return !c || !Database.Realizations.SuperClusters[i.Id].SequenceEqual(i.Hash);
																															}),
																			Tables.Releases		=> stamp.Releases.Where(i =>{
																																var c = Database.Releases.SuperClusters.ContainsKey(i.Id);
																																return !c || !Database.Releases.SuperClusters[i.Id].SequenceEqual(i.Hash);
																															})
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
									
										Database.Engine.Write(b);
									}
		
									Workflow.Log?.Report(this, "Cluster downloaded", $"{t.GetType().Name} {c.Id}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						download<AccountEntry, Account>(Database.Accounts);
						download<AuthorEntry, string>(Database.Authors);
						download<ProductEntry, ProductAddress>(Database.Products);
						download<RealizationEntry, RealizationAddress>(Database.Realizations);
						download<ReleaseEntry, ReleaseAddress>(Database.Releases);
		
						var r = new Round(Database){Id = stamp.FirstTailRound - 1, Hash = stamp.LastCommitedRoundHash, Confirmed = true};
		
						var rd = new BinaryReader(new MemoryStream(stamp.BaseState));
		
						rd.Read7BitEncodedInt();
						r.Hash		= rd.ReadSha3();
						r.Time		= rd.ReadTime();
						r.WeiSpent	= rd.ReadBigInteger();
						r.Factor	= rd.ReadCoin();
						r.Emission	= rd.ReadCoin();
						r.Members	= rd.ReadList<Member>();
						r.Funds		= rd.ReadList<Account>();
		
						Database.BaseState	= stamp.BaseState;
						Database.Members	= r.Members;
						Database.Funds		= r.Funds;
		
						Database.LastConfirmedRound = r;
						Database.LastCommittedRound = r;
		
						Database.Hashify();
		
						if(peer.GetStamp().BaseHash.SequenceEqual(Database.BaseHash))
							Database.LoadedRounds.Add(r.Id, r);
						else
							throw new SynchronizationException();
					}
		
					while(true)
					{
						Workflow.ThrowIfAborted();
		
						lock(Lock)
							if(final == -1)
								if(Database.Roles.HasFlag(Role.Chain))
									from = Database.LastConfirmedRound.Id + 1;
								else
									if(from == -1)
										from = Math.Max(stamp.FirstTailRound, Database.LastConfirmedRound == null ? -1 : (Database.LastConfirmedRound.Id + 1));
									else
										from = Database.LastConfirmedRound.Id + 1;
							else
								from = final;
		
						var to =/* end != -1 ? end :*/ (from + Database.Pitch);
		
						var rp = peer.Request<DownloadRoundsResponse>(new DownloadRoundsRequest{From = from, To = to});
						 	
						lock(Lock)
							if(from < rp.LastNonEmptyRound)
							{
								var rounds = rp.Read(Database);
									
								bool confirmed = true;
				
								foreach(var r in rounds.OrderBy(i => i.Id))
								{
									if(confirmed && r.Confirmed)
									{
										foreach(var p in r.Payloads)
										{
											p.Confirmed = true;

											foreach(var t in p.Transactions)
												foreach(var o in t.Operations)
													o.Placing = PlacingStage.Placed;
										}
				
										Database.Rounds.RemoveAll(i => i.Id == r.Id); /// remove old round with all its blocks
										Database.Rounds.Add(r);
										Database.Rounds = Database.Rounds.OrderByDescending(i => i.Id).ToList();
		
										var h = r.Hash;
										r.Confirmed = false;
										Database.Confirm(r, true);
#if DEBUG
										if(!r.Hash.SequenceEqual(h))
										{
											throw new SynchronizationException();
										}
#endif
										BlockCache.RemoveAll(i => i.RoundId <= r.Id);
									}
									else
									{
										if(confirmed && !r.Confirmed)
										{
											confirmed	= false;
											final		= rounds.Max(i => i.Id) + 1;
											//end			= rp.LastNonEmptyRound; 
											Synchronization	= Synchronization.Synchronizing;
										}
			
										if(!confirmed && r.Confirmed)
										{
							 				throw new SynchronizationException();
										}
				
										ProcessIncoming(r.Blocks, null);
									}
								}
										
								Workflow.Log?.Report(this, "Rounds received", $"{from}..{to}");
							}
							else
								if(Database.BaseHash.SequenceEqual(rp.BaseHash))
								{
// 									if(!Database.Roles.HasFlag(Role.Chain))
// 										Database.Rounds.Remove(Database.Rounds.Last());

									Database.Add(BlockCache.OrderBy(i => i.RoundId));
									BlockCache.Clear();
	
									Synchronization = Synchronization.Synchronized;
									SynchronizingThread = null;
	
									Workflow.Log?.Report(this, "Syncing finished");

									return;
								}
								else
									throw new SynchronizationException();
					}
				}
				catch(DistributedCallException)
				{
				}
				catch(SynchronizationException)
				{
				}
				catch(OperationCanceledException)
				{
					return;
				}
			}
		}

		public void ProcessIncoming(IEnumerable<Block> blocks, Peer peer)
		{
			Statistics.BlocksProcessing.Begin();

			var accepted = blocks.Where(b => BlockCache.All(i => !i.Signature.SequenceEqual(b.Signature)) && Database.Verify(b)).ToArray(); /// !ToArray cause will be added to Chain below

			if(!accepted.Any())
				return;

			if(Synchronization == Synchronization.Null || Synchronization == Synchronization.Synchronizing)
			{
				BlockCache.AddRange(accepted);
			}

			if(Synchronization == Synchronization.Synchronized)
			{
				var notolder = Database.LastConfirmedRound.Id - Net.Database.Pitch;
				var notnewer = Database.LastConfirmedRound.Id + Net.Database.Pitch * 2;

				var inrange = accepted.Where(b => notolder <= b.RoundId && b.RoundId <= notnewer);

				var joins = inrange.OfType<MembersJoinRequest>().Where(b => { 
																				for(int i = b.RoundId; i > b.RoundId - Net.Database.Pitch; i--) /// not more than 1 request per [Pitch] rounds
																					if(Database.JoinRequests.Any(j => j.RoundId == i && j.Generator == b.Generator))
																						return false;

																				//var r = Database.JoinRequests.Where(i => i.RoundId == b.RoundId);
																				//
																				//if(r.Count() < Net.Database.MembersMax) /// keep maximum MembersMax requests per round
																				//	return true;
																				//
																				//var min = r.Min(i => i.Bail);
																				//
																				//var a = Database.Accounts.Find(b.Generator, b.RoundId - Database.Pitch);
																				//
																				//return a != null && min < a.Bail; /// Selection of richest : if the number of members are Max then accept only those requests that have a bail greater than the existing request with minimal bail

																				return true;
																			});
				Database.Add(joins);
					
				var votes = inrange.Where(b => b is UC.Net.Vote v && (Database.Members.Any(j => j.Generator == b.Generator) || 
																	 (Database.Rounds.Any(r => r.Members == null ? false : r.Members.Any(m => m.Generator == b.Generator)))));
				Database.Add(votes);
			}

			Statistics.BlocksProcessing.End();
		}


//		public Round NeedToVote(Account generator)
//		{
// 			var r = Database.GetRound(Database.LastVotedRound.Id + 1);
// 
// 			while(r.Blocks.Any(i => i is Vote v && i.Generator == generator && v.Try == r.Try))
// 				r = Database.GetRound(r.Id + 1);
// 	
// 			if(r.Id > Database.LastVotedRound.Id + Database.Pitch)
//				return null;
//
//			var r = Database.GetRound(Database.LastConfirmedRound.Id + 1 + Database.Pitch);
//
//			if(r.Votes.Any(i => i.Generator == generator))
//				return null;
//
// 			//if(Enumerable.Range(Database.LastConfirmedRound.Id + 1, Database.Pitch).Any(i => !Database.GetRound(i).Voted))
// 			//	return null;
//
//			if(r.Parent == null || r.Parent.Payloads.Any(i => i.Hash == null)) /// cant refer to a downloaded round since its blocks have no hashes
//				return null;
//
//			return r;
//		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var blocks = new List<Block>();

			foreach(var g in Settings.Generators)
			{
				if(!Database.VoterOf(Database.GetRound(Database.LastConfirmedRound.Id + 1 + Database.Pitch)).Any(i => i.Generator == g))
				{
					var jr = Database.JoinRequests.Where(i => i.Generator == g).MaxBy(i => i.RoundId);
		
					if(jr == null || jr.RoundId + Database.Pitch * 3 <= Database.LastConfirmedRound.Id) /// to be elected we need to wait [Pitch] rounds for voting and [Pitch] rounds to confirm votes
					{
						var b = new MembersJoinRequest(Database){	RoundId	= Database.LastConfirmedRound.Id + Database.Pitch,
																	IP      = IP};
						b.Sign(g);
						blocks.Add(b);
					}
				}
				else
				{
					var r = Database.GetRound(Database.LastConfirmedRound.Id + 1 + Database.Pitch);

					if(r.Votes.Any(i => i.Generator == g))
						continue;

					if(r.Parent == null || r.Parent.Payloads.Any(i => i.Hash == null)) /// cant refer to downloaded rounds since its blocks have no hashes
						continue;

					var txs = Database.CollectValidTransactions(Transactions.Where(i => r.Id <= i.RoundMax && i.Operations.All(i => i.Placing == PlacingStage.Verified))
																			.GroupBy(i => i.Signer)
																			.Select(i => i.First()), r).ToArray();

					var prev = r.Previous.Votes.FirstOrDefault(i => i.Generator == g);
					var p = r.Parent;
	
					if(txs.Any()) /// any pending foreign transactions or any our pending operations
					{
						var rr = Database.ReferTo(p);
	
						if(rr == null)
							continue;
					
						var b = new Payload(Database)
								{
									RoundId		= r.Id,
									Try			= r.Try,
									Reference	= rr,
									Time		= Clock.Now,
									TimeDelta	= prev == null || prev.RoundId <= Database.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
									Violators	= p.Forkers.ToList(),
									Joiners		= Database.ProposeJoiners(r).ToList(),
									Leavers		= Database.ProposeLeavers(r, g).ToList(),
									FundJoiners	= new(),
									FundLeavers	= new(),
								};
					
						foreach(var i in txs)
						{
							b.AddNext(i);

							/// TODO: limit size
							///if()
							//{
							//}
	
							foreach(var o in i.Operations)
								o.Placing = PlacingStage.Placed;

							Transactions.Remove(i);
						}
							
						b.Sign(g);
						blocks.Add(b);
					}
					else if(Database.Rounds.Any(i => Database.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
					{
						var rr = Database.ReferTo(p);
					
						if(rr == null)
							continue;
	
						var b = new Vote(Database)
								{	
									RoundId		= r.Id,
									Try			= r.Try,
									Reference	= rr,
									Time		= Clock.Now,
									TimeDelta	= prev == null || prev.RoundId <= Database.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
									Violators	= p.Forkers.ToList(),
									Joiners		= Database.ProposeJoiners(r).ToList(),
									Leavers		= Database.ProposeLeavers(r, g).ToList(),
									FundJoiners	= new(),
									FundLeavers	= new(),
								};
								
						b.Sign(g);
						blocks.Add(b);
					}

					while(Database.VoterOf(r.Previous).Any(i => i.Generator == g) && !r.Previous.Votes.Any(i => i.Generator == g))
					{
						r = r.Previous;

						var rr = Database.ReferTo(p);
					
						if(rr == null)
							continue;
	
						prev = r.Previous.Votes.FirstOrDefault(i => i.Generator == g);

						var b = new Vote(Database)
								{	
									RoundId		= r.Id,
									Try			= r.Try,
									Reference	= rr,
									Time		= Clock.Now,
									TimeDelta	= prev == null || prev.RoundId <= Database.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
									Violators	= p.Forkers.ToList(),
									Joiners		= Database.ProposeJoiners(r).ToList(),
									Leavers		= Database.ProposeLeavers(r, g).ToList(),
									FundJoiners	= new(),
									FundLeavers	= new(),
								};
								
						b.Sign(g);
						blocks.Add(b);
					}

				}
			}

			if(blocks.Any())
			{
				foreach(var b in blocks)
				{
					Database.Add(b, b is Payload);
				}

				var pieces = new List<BlockPiece>();

				foreach(var b in blocks)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					w.Write(b.TypeCode);
					b.Write(w);

					s.Position = 0;
					var r = new BinaryReader(s);

					var n = Connections.Count()/*.Count(i => i.ChainRank > 0 || i.BaseRank > 0)*/;

					for(int i = 0; i < n; i++)
					{
						var p = new BlockPiece{	RoundId = b.RoundId,
												PiecesTotal = n,
												Piece = i,
												Data = r.ReadBytes((int)s.Length/n + (i < n-1 ? 0 : (int)s.Length % n))};

						p.Sign(b.Generator as PrivateAccount);

						pieces.Add(p);
					}
				}

 				var c = Connections.Where(i => i.BaseRank > 0).GetEnumerator();
 				var d = new Dictionary<Peer, List<BlockPiece>>();
 
 				foreach(var i in pieces)
 				{
 					if(!c.MoveNext())
 					{
 						c = Connections.Where(i => i.BaseRank > 0).GetEnumerator(); /// go to the beginning
 						c.MoveNext();
 					}
 
 					if(!d.ContainsKey(c.Current))
 						d[c.Current] = new();
 
 					d[c.Current].Add(i);
 				}

				foreach(var i in d)
				{
					i.Key.Request<object>(new UploadBlocksPiecesRequest{Pieces = i.Value});
				}

//				foreach(var i in Connections.Where(i => i.BaseRank > 0))
//				{
//					i.Request<object>(new UploadBlocksPiecesRequest{Pieces = pieces});
//				}
													
				Workflow.Log?.Report(this, "Block(s) generated", string.Join(", ", blocks.Select(i => $"{i.Type}({i.RoundId})")));
			}

			Statistics.Generating.End();
		}

		void ReachConsensus()
		{
			if(Synchronization != Synchronization.Synchronized)
				return;

			Statistics.Consensing.Begin();

			var r = Database.GetRound(Database.LastConfirmedRound.Id + 1 + Database.Pitch);
	
			if(!r.Voted)
			{
				if(Database.QuorumReached(r))
				{
					r.Voted = true;

					Database.Confirm(r.Parent, false);

					if(r.Parent.Confirmed)
					{
						Transactions.RemoveAll(t => t.RoundMax <= r.Parent.Id);
					}
					else
					{
						StartSynchronization();
						return;
					}
				}
				else if(Database.QuorumFailed(r) || (!Settings.Dev.DisableTimeouts && DateTime.UtcNow - r.FirstArrivalTime > TimeSpan.FromSeconds(15)))
				{
					foreach(var i in Transactions.Where(i => i.Payload.RoundId == r.Id).SelectMany(i => i.Operations))
					{
						i.Placing = PlacingStage.Verified;
					}

					r.FirstArrivalTime = DateTime.MaxValue;
					r.Try++;
				}
			}

			Statistics.Consensing.End();
		}

		void Delegating()
		{
			Operation[]					pendings;
			bool						ready;
			IEnumerable<Operation>		accepted;

			Workflow.Log?.Report(this, "Delegating started");

			MemberDci m = null;

			while(Running)
			{
				Thread.Sleep(1);
				Workflow.ThrowIfAborted();

				if(m == null)
				{	
					try
					{
						m = ConnectToMember(Workflow);
					}
					catch(ConnectionFailedException)
					{
						continue;
					}
				}

				try
				{
					Statistics.Delegating.Begin();

					lock(Lock)
					{
						if(m.Dci == this && Synchronization != Synchronization.Synchronized)
							continue;

						pendings = Operations.Where(i => i.Placing == PlacingStage.PendingDelegation).ToArray();
						ready = pendings.Any() && !Operations.Any(i => i.Placing == PlacingStage.Accepted);
					}

					if(ready) /// Any pending ops and no delegated cause we first need to recieve a valid block to keep tx id sequential correctly
					{
						var txs = new List<Transaction>();

						var rmax = m.GetNextRound().NextRoundId;

						lock(Lock)
						{
							foreach(var g in pendings.GroupBy(i => i.Signer))
							{
								if(!Vault.OperationIds.ContainsKey(g.Key))
								{
									Monitor.Exit(Lock);

									try
									{
										Vault.OperationIds[g.Key] = m.GetAccountInfo(g.Key, false).Info.LastOperationId;
									}
									catch(DistributedCallException ex) when(ex.Error == Error.AccountNotFound)
									{
										Vault.OperationIds[g.Key] = -1;
									}
									catch(Exception) when(!Debugger.IsAttached)
									{
									}
									
									Monitor.Enter(Lock);
								}

								var t = new Transaction(Settings, g.Key as PrivateAccount);

								foreach(var o in g)
								{
									o.Id = ++Vault.OperationIds[g.Key];
									t.AddOperation(o);
								}

								t.Sign(m.Generator, Database.GetValidityPeriod(rmax));
								txs.Add(t);
							}
						}
	
						var accs = m.DelegateTransactions(txs).Accepted;
	
						lock(Lock)
							foreach(var o in txs.SelectMany(i => i.Operations))
							{
								if(accs.Any(i => i.Account == o.Signer && i.Id == o.Id))
 									o.Placing = PlacingStage.Accepted;

								o.FlowReport?.Log?.ReportWarning(this, $"Placing has been delegated to {m}");
							}
									
						Workflow.Log?.Report(this, "Operation(s) delegated", $"{txs.Sum(i => i.Operations.Count(o => accs.Any(i => i.Account == o.Signer && i.Id == o.Id)))} op(s) in {accs.Count()} tx(s) -> {m.Generator} {(m.Dci is Peer p ? p.IP : "Self")}");

						Thread.Sleep(200); /// prevent flooding
					}

					lock(Lock)
						accepted = Operations.Where(i => i.Placing == PlacingStage.Accepted).ToArray();
	
					if(accepted.Any())
					{
						var rp = m.GetOperationStatus(accepted.Select(i => new OperationAddress{Account = i.Signer, Id = i.Id}));
							
						lock(Lock)
						{
							foreach(var i in rp.Operations)
							{
								var o = accepted.First(d => d.Signer == i.Account && d.Id == i.Id);
																		
								if(o.Placing != i.Placing)
								{
									if(i.Placing == PlacingStage.Placed)
									{
										//if(o.Placing == PlacingStage.Null)
										//	Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);
									}

									if(i.Placing == PlacingStage.Confirmed)
									{
										//if(o.Placing == PlacingStage.Null)
										//	Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);

										o.Placing = i.Placing;
										Operations.Remove(o);
									}
	
									if(i.Placing == PlacingStage.FailedOrNotFound)
									{
										//if(o.Placing == PlacingStage.Null)
										//	Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);

										o.Placing = i.Placing;
										Operations.Remove(o);
									}
								
								}
							}
						}
					}

					Statistics.Delegating.End();
				}
				catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException)
				{
					Workflow.Log?.ReportWarning(this, "Delegation", $"Member={m}", ex);

					if(m.Failures < 3)
						m.Failures++;
					else
						m = null;

					Thread.Sleep(500); /// prevent any flooding
				}
				catch(OperationCanceledException)
				{
					break;
				}
				catch(Exception ex) when (!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
		}

		void Enqueue(Operation o)
		{
			if(Operations.Count <= OperationsQueueLimit)
			{
				o.Placing = PlacingStage.PendingDelegation;
				Operations.Add(o);
			} 
			else
			{
				Workflow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
		public Operation Enqueue(Operation operation, PlacingStage waitstage, Workflow workflow)
		{
#if DEBUG
			operation.__ExpectedPlacing = waitstage;
#endif
			if(FeeAsker.Ask(this, operation.Signer as PrivateAccount, operation))
			{
				lock(Lock)
				 	Enqueue(operation);

				Await(operation, waitstage, workflow);

				return operation;
			}
			else
				return null;
		}

		void Verifing()
		{
			Workflow.Log?.Report(this, "Verifing started");

			try
			{
				while(Running)
				{
					Thread.Sleep(1);
					Workflow.ThrowIfAborted();

					lock(Lock)
					{
						foreach(var t in Transactions.Where(i => i.Operations.All(i => i.Placing == PlacingStage.Accepted)).ToArray())
						{
							bool valid = true;

							foreach(var o in t.Operations)
							{
								if(o is Emission e)
								{
									Monitor.Exit(Lock);

									try
									{
										valid = Nas.CheckEmission(e);
										
										if(!valid)
										{	
											Transactions.Remove(t);
											break;
										}
									}
									catch(Exception ex)
									{
										valid = false;
										Workflow.Log?.ReportError(this, "Can't verify Emission operation", ex);
										break;
									}
									finally
									{
										Monitor.Enter(Lock);
									}
								}
								
								o.Placing = PlacingStage.Verified;
							}
						}
					}
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

		void Declaring()
		{
			Workflow.Log?.Report(this, "Declaring started");

			try
			{
				while(Running)
				{
					Thread.Sleep(1);
					Workflow.ThrowIfAborted();

					Release[] rs;
					List<Peer> used;

					lock(Lock)
					{
						rs = Filebase.Releases.Where(i => i.Hubs.Count < 8).ToArray();
						used = rs.SelectMany(i => i.Hubs).Distinct().Where(h => rs.All(r => r.Hubs.Contains(h))).ToList();
					}

					if(rs.Any())
					{
						Call(	Role.Hub, 
								p => {
										lock(Lock)
											rs = rs.Where(i => !i.Hubs.Contains(p)).ToArray();

										p.DeclareRelease(rs.ToDictionary(i => i.Address, i => (i.Manifest.CompleteHash != null ? Distributive.Complete : 0) |
																							  (i.Manifest.IncrementalHash != null ? Distributive.Incremental : 0)));
												
										lock(Lock)
											foreach(var i in rs)
												i.Hubs.Add(p);

										used.Add(p);
									},
								Workflow,
								used,
								true);
					}
				}
			}
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		public List<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			if(!Settings.Generators.Any(g => Database.Members.Any(m => g == m.Generator))) /// not ready to process external transactions
				return new();

			Statistics.TransactionsProcessing.Begin();

			var accepted = txs.Where(i =>	!Transactions.Any(j => i.EqualBySignature(j)) &&
											i.RoundMax > Database.LastConfirmedRound.Id &&
											Settings.Generators.Any(g => g == i.Generator) &&
											i.Valid).ToList();
								
			foreach(var i in accepted)
				foreach(var o in i.Operations)
					o.Placing = PlacingStage.Accepted;

			Transactions.AddRange(accepted);

			Statistics.TransactionsProcessing.End();

			return accepted;
		}

		public MemberDci ConnectToMember(Workflow workflow)
		{
			if(Settings.Generators.Any())
			{
				return new MemberDci(Settings.Generators.First(), this);
			}

			Peer peer;
				
			while(true)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = Peers.OrderByDescending(i => i.Established).ThenBy(i => i.ReachFailures).FirstOrDefault();
	
					if(peer == null)
						continue;
				}
	
				try
				{
					Connect(peer, workflow);

					var cr = peer.GetMembers();
	
					lock(Lock)
					{
						if(cr.Members.Any())
						{
							RememberPeers(cr.Members.SelectMany(i => i.IPs).Select(i => new Peer(i)));

							peer.ReachFailures = 0;
	
							Members = cr.Members.ToList();
							
							foreach(var i in Members)
							{
								var c = Connections.FirstOrDefault(i => Members.SelectMany(i => i.IPs).Any(ip => i.IP.Equals(ip)));

								if(c != null)
								{
									Workflow.Log?.Report(this, "Member chosen", $"{i} {c}");
									return new MemberDci(i.Generator, c);
								}
							}

							var m = Members.Random();
							var ip = m.IPs.Random();
							var p = GetPeer(ip);

							Connect(p, workflow);
		
							Workflow.Log?.Report(this, "Member chosen", $"{m} {p}");
							return new MemberDci(m.Generator, p);
						}
					}
				}
				catch(Exception ex) when (ex is ConnectionFailedException || ex is AggregateException || ex is DistributedCallException)
				{
					peer.ReachFailures++;
				}
			}
	
			throw new ConnectionFailedException("Aborted, timeour of overall abort");
		}

		public Peer FindBestPeer(Role role, HashSet<Peer> exclusions)
		{
			lock(Lock)
				return Peers.Where(i => i.GetRank(role) > 0 && (exclusions == null || !exclusions.Contains(i))).OrderByDescending(i => i.Established)
																												.ThenBy(i => i.ReachFailures)
																												.ThenByDescending(i => i.GetRank(role))
																												.FirstOrDefault();
		}

		public Peer Connect(Role role, HashSet<Peer> exclusions, Workflow workflow)
		{
			Peer peer;
				
			while(Running)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = FindBestPeer(role, exclusions);
	
					if(peer == null)
					{
						exclusions.Clear();
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

		public R Call<R>(Role role, Func<Peer, R> call, Workflow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer peer;
				
			while(true)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = FindBestPeer(role, tried);
	
					if(peer == null)
						continue;
				}

				tried?.Add(peer);

				try
				{
					Connect(peer, workflow);

					return call(peer);
				}
				catch(ConnectionFailedException)
				{
				}
				catch(DistributedCallException)
				{
				}
			}
		}

		public void Call(Role role, Action<Peer> call, Workflow workflow, IEnumerable<Peer> exclusions = null, bool exitifnomore = false)
		{
			var excl = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer peer;
				
			while(true)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = FindBestPeer(role, excl);
	
					if(peer == null)
						if(exitifnomore)
							return;
						else
							continue;
				}

				excl?.Add(peer);

				try
				{
					Connect(peer, workflow);

					call(peer);

					break;
				}
				catch(ConnectionFailedException)
				{
				}
				catch(DistributedCallException)
				{
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

		public R Call<R>(IEnumerable<IPAddress> any, Func<Peer, R> call, Workflow workflow)
		{
			foreach(var i in any)
			{
				try
				{
					return Call(i, call, workflow);
				}
				catch(ConnectionFailedException)
				{
				}
				catch(DistributedCallException)
				{
				}
			}

			throw new DistributedCallException(Error.AllNodesFailed);
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

			while(true)
			{
				workflow.ThrowIfAborted();

				lock(Lock)
					if(peer.Established)
						return;
					else if(peer.OutStatus == EstablishingStatus.Failed)
						throw new ConnectionFailedException("Failed");

				if(!Settings.Dev.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new ConnectionFailedException("Timed out");

				Thread.Sleep(1);
			}

			//throw new ConnectionFailedException("Overall abort or timeout");
		}


		public Coin EstimateFee(IEnumerable<Operation> operations)
		{
			return Database != null ? Operation.CalculateFee(Database.LastConfirmedRound.Factor, operations) : Coin.Zero;
		}

		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, PrivateAccount signer, PlacingStage awaitstage, Workflow workflow)
		{
			var	l = Call(Role.Base, p =>{
											try
											{
												return p.GetAccountInfo(signer, true);
											}
											catch(DistributedCallException ex) when (ex.Error == Error.AccountNotFound)
											{
												return new AccountInfoResponse();
											}
										}, 
										workflow);
			
			var eid = l.Info == null ? 0 : l.Info.LastEmissionId + 1;

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

		public Emission FinishTransfer(PrivateAccount signer, Workflow workflow = null)
		{
			lock(Lock)
			{
			var	l = Call(Role.Base, p =>{
											try
											{
												return p.GetAccountInfo(signer, true);
											}
											catch(DistributedCallException ex) when (ex.Error == Error.AccountNotFound)
											{
												return new AccountInfoResponse();
											}
										}, 
										workflow);
			
			var eid = l.Info == null ? 0 : l.Info.LastEmissionId + 1;

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

		public Download DownloadRelease(ReleaseAddress release, Workflow workflow)
		{
			lock(Lock)
			{
				if(Filebase.FindRelease(release) != null)
					return null;

				var d = new Download(this, release, workflow);
	
				Downloads.Add(d);
		
				return d;
			}
		}

		public ReleaseInfo GetReleaseInfo(ReleaseAddress release)
		{
			var m = Filebase.FindRelease(release);
			
			if(m != null)
			{
				return new ReleaseInfo { Manifest = m.Manifest };
			}

			Download d;

			d = Downloads.Find(i => i.Release == release);

			if(d != null)
			{
				return new () { Download = new () {	Distributive			= d.Distributive,
													Length					= d.Length, 
													CompletedLength			= d.CompletedLength,
													DependenciesCount		= d.DependenciesCount,
													AllDependenciesFound	= d.AllDependenciesFound,
													DependenciesSuccessful	= d.DependenciesSuccessfulCount} };
			}

			return new ReleaseInfo();
		}

		public void AddRelease(ReleaseAddress release, string channel, IEnumerable<string> sources, string dependsdirectory, bool confirmed, Workflow workflow)
		{
			var qlatest = Call(Role.Base, p => p.QueryRelease(release, release.Version, VersionQuery.Latest, channel, confirmed), workflow);
			var previos = qlatest.Releases.FirstOrDefault()?.Registration.Release.Version;

			Filebase.AddRelease(release, previos, sources, dependsdirectory, workflow);
		}

		public override Rp Request<Rp>(Request rq) where Rp : class
  		{
			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;
				rq = BinarySerializator.Deserialize(new(s), rq.GetType(), Constractor) as Request;
			}

 			return rq.Execute(this) as Rp;
 		}

		void Await(Operation o, PlacingStage s, Workflow workflow)
		{
			while(true)
			{ 
				Thread.Sleep(1);
				workflow.ThrowIfAborted();

				switch(s)
				{
					case PlacingStage.Null :				return;
					case PlacingStage.Accepted :			if(o.Placing >= PlacingStage.Accepted) return; else break;
					case PlacingStage.Placed :				if(o.Placing >= PlacingStage.Placed) return; else break;
					case PlacingStage.Confirmed :			if(o.Placing == PlacingStage.Confirmed) return; else break;
					case PlacingStage.FailedOrNotFound :	if(o.Placing == PlacingStage.FailedOrNotFound) return; else break;
				}
			}
		}
	}
}
