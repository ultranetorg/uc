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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Web3;
using RocksDbSharp;

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
		Chain	= 0b00000001,
		Seed	= 0b00000010,
		Hub		= 0b00000100,
	}
 
	public class Core : Dci
	{
		public static readonly int[]					Versions = {1};
		public const string								FailureExt = "failure";
		public const int								Timeout = 5000;
		public const int								OperationsQueueLimit = 1000;
		const int										BalanceWidth = 24;

		public Log										Log;
		Workflow										Workflow;
		public Vault									Vault;
		public INas										Nas;
		public Roundchain								Chain;
		public Filebase									Filebase;
		public Hub										Hub;
		RocksDb											Database;
		public bool										Networking => DelegatingThread != null;
		public bool										IsClient => Networking && ListeningThread == null;
		public object									Lock = new();
		public Settings									Settings;
		public Clock									Clock;

		CandidacyDeclaration							Declaration;
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
		public List<Block>								Cache		= new();
		public List<Peer>								Members		= new();
		//Dictionary<PackageAddress, List<IPAddress>>		PackagePeers = new();
		public List<Download>							Downloads = new();

		TcpListener										Listener;
		Thread											ListeningThread;
		Thread											DelegatingThread;
		Thread											VerifingThread;
		Thread											SynchronizingThread;

		JsonServer										ApiServer;

		public bool										Running { get; protected set; } = true;
		public Synchronization							_Synchronization = Synchronization.Null;
		public Synchronization							Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate								SynchronizationChanged;
		//DateTime										SyncRequested;

		public IGasAsker								GasAsker; 
		public IFeeAsker								FeeAsker;

		public ColumnFamilyHandle						PeersFamily => Database.GetColumnFamily(nameof(Peers));
		//public ColumnFamilyHandle						ReleasesFamily => Database.GetColumnFamily(nameof(Releases));

		readonly DbOptions								DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																							.SetCreateMissingColumnFamilies(true);
		
		Func<Type, object>								Constractor => t => t == typeof(Transaction) ? new Transaction(Settings){ Generator = Generator } : null;
		
		public string[][] Info
		{
			get
			{
				List<string> f = new();
				List<string> v = new(); 
															
				f.Add("Zone");					v.Add(Settings.Zone.Name);
				f.Add("Profile");				v.Add(Settings.Profile);
				f.Add("IP(Reported):Port");		v.Add($"{Settings.IP} ({IP}) : {Settings.Port}");
				//f.Add($"Generator{(Nci != null ? " (delegation)" : "")}");	v.Add($"{(Generator ?? Nci?.Generator)}");
				f.Add("Operations");			v.Add($"{Operations.Count}");
				f.Add("    Pending");			v.Add($"{Operations.Count(i => i.Delegation == DelegationStage.Pending)}");
				f.Add("    Delegated");			v.Add($"{Operations.Count(i => i.Delegation == DelegationStage.Delegated)}");
				f.Add("       Accepted");		v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Accepted)}");
				f.Add("       Pending");		v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Pending)}");
				f.Add("       Placed");			v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Placed)}");
				f.Add("       Confirmed");		v.Add($"{Operations.Count(i => i.Placing == PlacingStage.Confirmed)}");
				f.Add("Peers in/out/min/known");v.Add($"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}");
				
				//f.Add("Transactions");			v.Add($"{Transactions.Count}");
				//f.Add("    pending");			v.Add($"{Transactions.Count(i => i.Stage == ProcessingStage.Pending)}");
				//f.Add("    placed");			v.Add($"{Transactions.Count(i => i.Stage == ProcessingStage.Placed)}");

				if(Chain != null)
				{
					f.Add("Synchronization");		v.Add($"{Synchronization}");
					f.Add("Members");				v.Add($"{Chain.Members.Count}");
					f.Add("Emission");				v.Add($"{Chain.LastPayloadRound.Emission.ToHumanString()}");
					f.Add("Cached Blocks");			v.Add($"{Cache.Count()}");
					f.Add("Cached Rounds");			v.Add($"{Chain.LoadedRounds.Count()}");
					f.Add("Last Non-Empty Round");	v.Add($"{Chain.LastNonEmptyRound.Id}");
					f.Add("Last Payload Round");	v.Add($"{Chain.LastPayloadRound.Id}");
					f.Add("Generating (μs)");		v.Add((Statistics.Generating.Avarage.Ticks/10).ToString());
					f.Add("Consensing (μs)");		v.Add((Statistics.Consensing.Avarage.Ticks/10).ToString());
					//f.Add("Delegating (μs)");		v.Add((Statistics.Delegating.Avarage.Ticks/10).ToString());
					f.Add("Block Processing (μs)");	v.Add((Statistics.BlocksProcessing.Avarage.Ticks/10).ToString());
					f.Add("Tx Processing (μs)");	v.Add((Statistics.TransactionsProcessing.Avarage.Ticks/10).ToString());

					string formatbalance(Account a, bool confirmed)
					{
						return Chain.GetAccountInfo(a, false)?.Balance.ToHumanString();
					}

					foreach(var i in Vault.Accounts)
					{
						f.Add($"Account");	v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
					}
	
					if(Settings.Dev.UI)
					{
						f.Add("NAS Eth Account");		v.Add($"{Nas.Account?.Address}");

						foreach(var i in Chain.Funds)
						{
							f.Add($"Fundable");	v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
						}
						
						foreach(var i in Roundchain.Fathers)
						{
							f.Add($"Father"); v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
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
		
		public Header Header
		{
			get
			{
				Header h;

				lock(Lock)
				{
					h =	new Header
						{ 
							LastRound			= Chain == null ? -1 : Chain.LastNonEmptyRound.Id,
							LastConfirmedRound	= Chain == null ? -1 : Chain.LastConfirmedRound.Id,
						};
				}

				return h;
			}
		}

		public Core(Settings settings, string exedirectory, Log log)
		{
			Settings = settings;
			Cryptography.Current = settings.Cryptography;
			Log	 = Settings.Log ? log : null;

			Workflow = new Workflow(log);

			Directory.CreateDirectory(Settings.Profile);

			Log?.Report(this, $"Ultranet Node/Client {Assembly.GetEntryAssembly().GetName().Version}");
			Log?.Report(this, $"Runtime: {Environment.Version}");	
			Log?.Report(this, $"Profile: {Settings.Profile}");	
			Log?.Report(this, $"Protocol Versions: {string.Join(',', Versions)}");
			Log?.Report(this, $"Zone: {Settings.Zone.Name}");
			
			if(Settings.Dev.Any)
				Log?.ReportWarning(this, $"Dev: {Settings.Dev}");

			Vault = new Vault(Settings, Log);

			var cfamilies = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{
																new (nameof(Peers), new ()),
																new (nameof(Roundchain.Accounts), new ()),
																new (nameof(Roundchain.Authors), new ()),
																new (nameof(Roundchain.Products), new ()),
																new (nameof(Roundchain.Rounds), new ()),
																new (nameof(Roundchain.Funds), new ()),
															})
			cfamilies.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
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

			if(Settings.Chain.Enabled)
			{
				Log?.Report(this, "Chain started");
		
		  		try
		  		{
		 			new Uri(Settings.Nas.Provider);
		  		}
		  		catch(Exception)
		  		{
		  			Log.ReportError(this, $"Ethereum provider (Settings.xon -> Nas -> Provider) is not set or has incorrect format.");
		 			Log.ReportError(this, $"It's required to run the node in full mode.");
		 			Log.ReportError(this, $"This can be instance of some Ethereum client or third-party services like Infura.");
		 			Log.ReportError(this, $"Corresponding configuration file is located here: {Path.Join(Settings.Profile, Settings.FileName)}");
					return;
		  		}
		
				Chain = new Roundchain(Settings, Log, Nas, Vault, Database);
		
				if(Settings.Generator != null)
				{
					Generator = PrivateAccount.Parse(Settings.Generator);
					Declaration = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(Generator);
				}
		
				Chain.BlockAdded += b => {
											if(Generator != null)
												Declaration = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(Generator, null, null, null, r => r.Confirmed);
					
											ReachConsensus();
										 };
		
				if(Generator != null)
				{
					VerifingThread = new Thread(Verifing);
					VerifingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Verifing";
					VerifingThread.Start();
				}
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
												
												if(Chain != null)
												{
													if(Synchronization == Synchronization.Synchronized)
													{
														var conns = Connections.Where(i => i.Roles.HasFlag(Role.Chain)).GroupBy(i => i.LastConfirmedRound).ToArray(); /// Not cool, cause Peer.Established may change after this and 'conn' items will change
		
														if(conns.Any())
														{
															var max = conns.Aggregate((i, j) => i.Count() > j.Count() ? i : j);
							
															if(max.Key - Chain.LastConfirmedRound.Id > Roundchain.Pitch) /// we are late, force to resync
															{
																StartSynchronization();
															}
														}

														if(Generator != null)
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

// 			if(Abort != null && Abort())
// 			{
// 				throw new AbortException();
// 			}
		}

		public void Stop(MethodBase methodBase, Exception ex)
		{
			lock(Lock)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(methodBase.Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings.Profile, m + "." + Core.FailureExt), ex.ToString());
				Log?.ReportError(this, m, ex);
	
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

			Database?.Dispose();

			Log?.Report(this, "Stopped", message);
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
	 				p.LoadNode(new BinaryReader(new MemoryStream(i.Value())));
	 				Peers.Add(p);
				}
			}
			
			if(Peers.Any())
			{
				Log?.Report(this, "Peers loaded", $"n={Peers.Count}");
			}
			else
			{
				while(Running)
				{
					var initials = Nas.GetInitials(Settings.Zone);

					if(initials.Any())
					{
						RememberPeers(initials.Select(i => new Peer(i){LastSeen = DateTime.UtcNow}));

						Log?.Report(this, "Initial nodes retrieved", initials.Length.ToString());
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
	
					Database.Write(b);
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
				(Chain == null || Connections.Count(i => i.Roles.HasFlag(Role.Chain)) >= Settings.Chain.PeersMin))
			{
				if(Filebase != null && !IsClient)
				{
					var ps = Filebase.GetAll();
					
					if(ps.Any())
					{
						Task.Run(() =>	{
											DeclarePackage(ps, Workflow);
											Log?.Report(this, "Initial declaration completed");
										});
					}
				}

				if(Chain != null)
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
	
				Log?.Report(this, "Listening started", $"{Settings.IP}:{Settings.Port}");

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

			h.Roles					= (Chain != null ? Role.Chain : 0) | (Filebase != null ? Role.Seed : 0) | (Hub != null ? Role.Hub : 0);
			h.Versions				= Versions;
			h.Zone					= Settings.Zone.Name;
			h.IP					= ip;
			h.Nuid					= Nuid;
			h.Peers					= peers;
			h.LastRound				= Header.LastRound;
			h.LastConfirmedRound	= Header.LastConfirmedRound; 
			
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
					catch(SocketException) 
					{
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
					catch(Exception)// when(!Settings.Dev.ThrowOnCorrupted)
					{
						goto failed;
					}
	
					lock(Lock)
					{
						if(h.Nuid == Nuid)
						{
							Peers.Remove(peer);
							goto failed;
						}
													
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Log?.Report(this, "Detected IP", IP.ToString());
						}
	
						if(peer.Established/* && (peer.IP.GetAddressBytes()[3] + IP.GetAddressBytes()[3]) % 2 == 0*/)
						{
							client.Close();
							return;
						}
	
						RememberPeers(h.Peers);
	
						peer.OutStatus = EstablishingStatus.Succeeded;
						peer.Start(this, client, h, Listening, Lock, $"{Settings.IP.GetAddressBytes()[3]}");
	
						Log?.Report(this, "Connected to", $"{peer}");
	
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
			var p = Peers.Find(i => i.IP.Equals(ip));

			if(ip.Equals(IP) || IgnoredIPs.Any(j => j.Equals(ip)))
			{
				Peers.Remove(p);
				client.Close();
				return;
			}

			if(p != null)
			{
				p.InStatus = EstablishingStatus.Initiated;
			}

			var t = new Thread(a => incon());
			t.Name = Settings.IP.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
			t.Start();

			void incon(){
							try
							{
								Hello h = null;
	
								try
								{
									client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
									client.ReceiveTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;

									h = Peer.WaitHello(client);
								}
								catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
								{
									goto failed;
								}
				
								lock(Lock)
								{
									if(h.Nuid == Nuid)
									{
										goto failed;
									}
	
									if(IP.Equals(IPAddress.None))
									{
										IP = h.IP;
										Log?.Report(this, "Detected IP", IP.ToString());
									}
		
									if(p != null && p.Established)
									{
										goto failed;
									}
	
									try
									{
										Peer.SendHello(client, CreateHello(ip));
									}
									catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
									{
										goto failed;
									}
	
									if(p == null)
									{
										p = new Peer(ip);
										Peers.Add(p);
									}
									
									RememberPeers(h.Peers.Append(p));
	
									p.InStatus = EstablishingStatus.Succeeded;
									p.Start(this, client, h, Listening, Lock, $"{Settings.IP.GetAddressBytes()[3]}");
			
									Log?.Report(this, "Accepted from", $"{p}");
	
									return;
								}
	
							failed:
								lock(Lock)
									if(p != null)
										p.InStatus = EstablishingStatus.Failed;

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
						case PacketType.Blocks:
						{
							IEnumerable<Block> blocks;
										
							try
							{
								blocks = pk.Read((r, c) => Block.FromType(Chain, (BlockType)c));
							}
							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
							{
								peer.Disconnect();
								break;
							}
		
							lock(Lock)
							{
								ProcessIncoming(blocks, peer);
							}
	
							break;
						}

						//case PacketType.Rounds:
						//{
						//	Round[] rounds;
						//
						//	try
						//	{
						//		rounds = pk.Read(r => new Round(Chain));
						//	}
						//	catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
						//	{
						//		peer.Disconnect();
						//		break;
						//	}
						//
						//	break;
						//}

						//case PacketType.RoundsRequest:
						//{
						//	int from;
						//	int to;
						//
						//	try
						//	{
						//		from	= reader.Read7BitEncodedInt();
						//		to		= reader.Read7BitEncodedInt();
						//	}
						//	catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
						//	{
						//		peer.Disconnect();
						//		break;
						//	}
						//
						//	lock(Lock)
						//	{
						//		peer.Send(Packet.Create(PacketType.Rounds, rounds));
						//	}
						//		
						//	break;
						//}

 						case PacketType.Request:
 						{
							Request[] requests;

 							try
 							{
								requests = BinarySerializator.Deserialize(reader,	c => {
																							var o = UC.Net.Request.FromType(Chain, (DistributedCall)c); 
																							o.Peer = peer; 
																							return o;
																						},
																					Constractor
																					);

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
																			t => UC.Net.Response.FromType(Chain, (DistributedCall)t), 
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
							Log?.ReportError(this, $"Wrong packet type {pk.Type}");
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
				Log?.Report(this, "Syncing started");

				SynchronizingThread = new Thread(Synchronizing);
				SynchronizingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Synchronizing";
				SynchronizingThread.Start();
		
				Synchronization = Synchronization.Downloading;
			}
		}

		void Synchronizing()
		{
			int start = -1; 
			int end = -1; 
	
			var used = new HashSet<Peer>();
	
			var peer = Connect(Role.Chain, used, Workflow);
	
			while(true)
			{
				Workflow.ThrowIfAborted();

				if(Synchronization == Synchronization.Downloading || Synchronization == Synchronization.Synchronizing)
				{	
					var from = start != -1 ? start : (Chain.LastConfirmedRound.Id + 1);
					var to	 = end != -1 ? end : (from + Math.Min(peer.LastRound - from, Roundchain.Pitch));
				 	
					if(from <= to)
					{
						var rp = peer.Request<DownloadRoundsResponse>(new DownloadRoundsRequest{From = from, To = to});
	
						var rounds = rp.Read(Chain);
							
						lock(Lock)
						{
							bool confirmed = true;
		
							foreach(var r in rounds.OrderBy(i => i.Id))
							{
								if(confirmed && r.Confirmed)
								{
									foreach(var b in r.Blocks)
										b.Confirmed = true;
		
									Chain.Rounds.RemoveAll(i => i.Id == r.Id); /// remove old round with all its blocks
									Chain.Rounds.Add(r);
									Chain.Rounds = Chain.Rounds.OrderByDescending(i => i.Id).ToList();
									Chain.Seal(r);
										
									Cache.RemoveAll(i => i.RoundId <= r.Id);
								}
								else
								{
									if(confirmed && !r.Confirmed)
									{
										confirmed	= false;
										start		= rounds.Max(i => i.Id) + 1;
										end			= peer.LastRound; 
										Synchronization	= Synchronization.Synchronizing;
									}
	
									if(!confirmed && r.Confirmed)
									{
					 					peer = Connect(Role.Chain, used, Workflow); /// unacceptable case, choose other chain peer
										break;
									}
		
									ProcessIncoming(r.Blocks, null);
								}
							}
								
							Log?.Report(this, "Rounds received", $"{from}..{to}");
						}
					}
					else
						break;
				}
			}
	
			lock(Lock)
			{
				Chain.Add(Cache.OrderBy(i => i.RoundId));
				Cache.Clear();
	
				Synchronization = Synchronization.Synchronized;
				SynchronizingThread = null;
			}
	
			Log?.Report(this, "Syncing finished");
		}

		public void ProcessIncoming(IEnumerable<Block> blocks, Peer peer)
		{
			Statistics.BlocksProcessing.Begin();

			var accepted = blocks.Where(b => Cache.All(i => !i.Signature.SequenceEqual(b.Signature)) && Chain.Verify(b)).ToArray(); /// !ToArray cause will be added to Chain below

			if(!accepted.Any())
				return;

			if(Synchronization == Synchronization.Null || Synchronization == Synchronization.Synchronizing)
			{
				Cache.AddRange(accepted);
			}

			if(Synchronization == Synchronization.Synchronized)
			{
				var notolder = Chain.LastConfirmedRound.Id - Roundchain.Pitch;
				var notnewer = Chain.LastConfirmedRound.Id + Roundchain.Pitch * 2;

				var inrange = accepted.Where(b => notolder <= b.RoundId && b.RoundId <= notnewer);

				var joins = inrange.OfType<GeneratorJoinRequest>().Where(b => { 
																		var d = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(b.Generator);
														
																		if(d == null)
																			return false;

																		for(int i = b.RoundId; i > b.RoundId - Roundchain.Pitch; i--) /// not often than 1 request per [Pitch] rounds
																			if(Chain.GetRound(i).JoinRequests.Any(i => i.Generator == b.Generator))
																				return false;

																		if(Chain.GetRound(b.RoundId).JoinRequests.Count() < Roundchain.MembersMax) /// keep  maximum MembersMax requests per round
																			return true;

																		var min = Chain.GetRound(b.RoundId).JoinRequests.Aggregate((i, j) => i.Declaration.Bail < j.Declaration.Bail ? i : j);
														
																		return min.Declaration.Bail < d.Bail; /// if a number of members are Max then accept only those requests that have a bail greater than the existing request with minimal bail
																	});
				Chain.Add(joins);
					
				var votes = inrange.Where(b => b is UC.Net.Vote v && (Chain.Members.Any(j => j.Generator == b.Generator) || 
																	 (Chain.Rounds.Any(r => r.Members == null ? false : r.Members.Any(m => m.Generator == b.Generator)))));

				Chain.Add(votes);
			}

			if(Synchronization == Synchronization.Null || Synchronization == Synchronization.Synchronizing || Synchronization == Synchronization.Synchronized) /// Null and Synchronizing needed for Dev purposes
			{
				Broadcast(Packet.Create(PacketType.Blocks, accepted), peer);
			}

			Statistics.BlocksProcessing.End();

		}

		public Round GetNextAvailableRound()
		{
			var r = Chain.GetRound(Chain.LastVotedRound.Id + 1);

			while(r.Blocks.Any(i => i.Generator == Generator))
				r = Chain.GetRound(r.Id + 1);
	
			if(r.Id > Chain.LastVotedRound.Id + Roundchain.Pitch)
				return null;

			return r;
		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var votes = new List<Block>();

			if(Declaration != null)
			{
				var nar = GetNextAvailableRound();

				if(nar == null)
					return;

				var voters = Chain.VotersFor(nar);
						
				if(voters.All(i => i.Generator != Generator))
				{
					var jr = Chain.FindLastBlock(i => i is GeneratorJoinRequest && i.Generator == Generator);
	
					if(jr == null || (Chain.LastVotedRound.Id - jr.RoundId > Roundchain.Pitch * 2)) /// to be elected we need to wait [Pitch] rounds for voting and [Pitch] rounds to confirm votes
					{
						var b = new GeneratorJoinRequest(Chain)
									{
										RoundId		= nar.Id,
										IP			= IP
									};

						votes.Add(b);
					}
				}
				else
				{
					var txs = Chain.CollectValidTransactions(Transactions	.Where(i => i.Operations.All(i => i.Placing == PlacingStage.Pending) && i.RoundMax >= nar.Id)
																			.GroupBy(i => i.Signer)
																			.Select(i => i.First()), nar);

					var prev = Chain.FindRound(nar.Id - 1).Votes.FirstOrDefault(i => i.Generator == Generator);

					if(txs.Any()) /// any pending foreign transactions or any our pending operations
					{
						var p = Chain.FindRound(nar.ParentId);
		
						var rr = Chain.Refer(p);

						if(rr == null)
							return;
				
						var b = new Payload(Chain)
								{
									RoundId		= nar.Id,
									Try			= nar.Try,
									Reference	= rr,
									Time		= Clock.Now,
									TimeDelta	= prev == null ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
									Violators	= p.Forkers.ToList(),
									Joiners		= Chain.ProposeJoiners(nar).ToList(),
									Leavers		= Chain.ProposeLeavers(nar, Generator).ToList(),
									FundJoiners	= new(),
									FundLeavers	= new(),
									//Propositions		= msgs
								};
				
						foreach(var i in txs)
						{
							(b as Payload).AddNext(i);

							foreach(var o in i.Operations)
								o.Placing = PlacingStage.Placed;
						}
						
						votes.Add(b);
					}
					else
					{
						var r = Chain.Rounds.LastOrDefault(i => !i.Confirmed && !i.Blocks.Any(j => j.Generator == Generator));

						while(r != null)
						{

							if(	Chain.VotersFor(r).Any(i => i.Generator == Generator) &&			/// we must vote
								!r.Votes.Any(i => i.Generator == Generator) &&						/// no our block or vote yet
								r.Votes.OfType<Payload>().Any() 								/// has already some payloads from other members
								)
							{
								var p = Chain.FindRound(r.ParentId);
								var rr = Chain.Refer(p);
				
								if(rr != null)
								{

									var b = new Vote(Chain)
											{	
												RoundId		= r.Id,
												Try			= r.Try,
												Reference	= rr,
												Time		= Clock.Now,
												TimeDelta	= prev == null ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
												Violators	= p.Forkers.ToList(),
												Joiners		= Chain.ProposeJoiners(r).ToList(),
												Leavers		= Chain.ProposeLeavers(r, Generator).ToList(),
												FundJoiners	= new(),
												FundLeavers	= new(),
											};
							
									votes.Add(b);
								}
							}

							r = Chain.FindRound(r.Id + 1);
						}
					}
				}

				if(votes.Any())
				{
					foreach(var b in votes)
					{
						b.Sign(Generator as PrivateAccount);
						Chain.Add(b, b is Payload);
					}

					Broadcast(Packet.Create(PacketType.Blocks, votes));
													
					Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{i.Type}({i.RoundId})")));
				}
			}

			Statistics.Generating.End();
		}

		void Delegating()
		{
			Peer[]						peers;
			Operation[]					pendings;
			bool						ready;
			IEnumerable<Operation>		delegated;

			Log?.Report(this, "Delegating started");

			Dci m = null;

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
						if(m == this && Synchronization != Synchronization.Synchronized)
							continue;

						peers = Peers.ToArray();
						pendings = Operations.Where(i => i.Delegation == DelegationStage.Pending).ToArray();
						ready = pendings.Any() && !Operations.Any(i => i.Delegation == DelegationStage.Delegated && i.Placing == PlacingStage.Null);
					}

					if(ready) /// Any pending ops and no delegated cause we first need to recieve a valid block to keep tx id sequential correctly
					{
						var txs = new List<Transaction>();

						var rmax = m.GetNextRound().NextRoundId;

						lock(Lock)
						{
							foreach(var g in pendings.GroupBy(i => i.Signer))
							{
								int id = 1;

								if(!Vault.OperationIds.ContainsKey(g.Key))
								{
									Operation o = null;

									Monitor.Exit(Lock);

									try
									{
										o = m.GetLastOperation(g.Key, null).Operation;
									}
									catch(Exception) when(!Debugger.IsAttached)
									{
									}
									
									Monitor.Enter(Lock);
									Vault.OperationIds[g.Key] = o == null ? -1 : o.Id;
								}

								var t = new Transaction(Settings, g.Key as PrivateAccount);

								foreach(var o in g)
								{
									o.Id = Vault.OperationIds[g.Key] + id++;
									t.AddOperation(o);
								}

								t.Sign(m.Generator, Roundchain.GetValidityPeriod(rmax));
								txs.Add(t);
							}
						}
	
						var accepted = m.DelegateTransactions(txs).Accepted;
	
						lock(Lock)
							foreach(var o in txs.SelectMany(i => i.Operations))
							{
								if(accepted.Any(i => i.Account == o.Signer && i.Id == o.Id))
 									o.Delegation = DelegationStage.Delegated;

								//o.FlowReport?.StageChanged();
								o.FlowReport?.Log?.ReportWarning(this, $"Placing has been delegated to {m}");
							}
									
						Log?.Report(this, "Operation(s) delegated", $"{txs.Sum(i => i.Operations.Count(o => accepted.Any(i => i.Account == o.Signer && i.Id == o.Id)))} op(s) in {accepted.Count()} tx(s) -> {m.Generator} {(m is Peer p ? p.IP : "Self")}");

						Thread.Sleep(500); /// prevent any flooding
					}

					lock(Lock)
						delegated = Operations.Where(i => i.Delegation == DelegationStage.Delegated).ToArray();
	
					if(delegated.Any())
					{
						var rp = m.GetOperationStatus(delegated.Select(i => new OperationAddress{Account = i.Signer, Id = i.Id}).ToArray());
							
						if(rp != null)
						{
							lock(Lock)
							{
								foreach(var i in rp.Operations)
								{
									var o = delegated.First(d => d.Signer == i.Account && d.Id == i.Id);
																		
									if(o.Placing != i.Placing)
									{
										if(i.Placing == PlacingStage.Placed)
										{
											if(o.Placing == PlacingStage.Null)
												Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);
										}

										if(i.Placing == PlacingStage.Confirmed)
										{
											if(o.Placing == PlacingStage.Null)
												Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);

											o.Delegation = DelegationStage.Completed;
											Operations.Remove(o);
										}
	
										if(i.Placing == PlacingStage.FailedOrNotFound)
										{
											if(o.Placing == PlacingStage.Null)
												Vault.OperationIds[o.Signer] = Math.Max(o.Id, Vault.OperationIds[o.Signer]);

											o.Delegation = DelegationStage.Completed;
											Operations.Remove(o);
										}
								
										o.Placing = i.Placing;
										//o.FlowReport?.StageChanged();
									}
								}
							}
						}
					}

					Statistics.Delegating.End();
				}
				catch(Exception ex) when (ex is ConnectionFailedException || ex is DistributedCallException)
				{
					Log?.ReportWarning(this, "Delegation", $"Member={m}", ex);

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

		void ReachConsensus()
		{
			if(Synchronization != Synchronization.Synchronized)
				return;

			Statistics.Consensing.Begin();

			var r = Chain.Rounds.LastOrDefault(i => !i.Voted);
	
			while(r != null)
			{
				var p = Chain.FindRound(r.ParentId);
					
				if(!r.Voted)
				{
					if(Chain.QuorumReached(r))
					{
						r.Voted = true;
					}
					else if(Chain.QuorumFailed(r) || (!Settings.Dev.DisableTimeouts && DateTime.UtcNow - r.FirstArrivalTime > TimeSpan.FromSeconds(15)))
					{
						foreach(var i in Transactions.OfType<Transaction>().Where(i => i.Payload.RoundId == r.Id).SelectMany(i => i.Operations))
						{
							i.Placing = PlacingStage.Pending;
						}

						r.FirstArrivalTime = DateTime.MaxValue;
						r.Try++;
					}
				}
					
				if(r.Voted && p != null && !p.Confirmed)
				{
					var prevs = Chain.Rounds.Where(i => i.Id < p.Id).ToList();
					var sequential = prevs.Zip(prevs.Skip(1), (x, y) => x.Id == y.Id + 1).All(x => x);
					
					var c = r;
	
					if(prevs.All(i => i.Confirmed) && sequential)
					{
						do
						{
							Chain.Confirm(p);

							if(p.Confirmed)
							{
								Transactions.RemoveAll(t => t.RoundMax <= p.Id);
							}
							else
							{
								StartSynchronization();
								return;
							}
			
							p = Chain.FindRound(p.Id + 1);
							c = p != null ? Chain.FindRound(p.Id + Roundchain.Pitch) : null;
						}
						while(p != null && c != null && !p.Confirmed && c.Voted);
					}
				}
									
				r = Chain.FindRound(r.Id + 1);
			}

			Statistics.Consensing.End();
		}

		void Enqueue(Operation o)
		{
			if(Operations.Count <= OperationsQueueLimit)
			{
				o.Delegation = DelegationStage.Pending;
				Operations.Add(o);
			} 
			else
			{
				Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
		public Operation Enqueue(Operation operation, PlacingStage waitstage, Workflow workflow)
		{
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
			Log?.Report(this, "Verifing started");

			try
			{
				while(Running)
				{
					Thread.Sleep(1);

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
										Log?.ReportError(this, "Can't verify Emission operation", ex);
										break;
									}
									finally
									{
										Monitor.Enter(Lock);
									}
								}
								
								o.Placing = PlacingStage.Pending;
							}
						}
					}
				}
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		public List<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			if(!Chain.Members.Any(i => i.Generator == Generator))
				return new();

			Statistics.TransactionsProcessing.Begin();

			if(Generator == null) /// not ready to process external transactions
				return new();

			var accepted = txs.Where(i =>	!Transactions.Any(j => i.SignatureEquals(j)) && 
											i.RoundMax > Chain.LastConfirmedRound.Id && 
											i.Valid).ToList();
								
			foreach(var i in accepted)
				foreach(var o in i.Operations)
					o.Placing = PlacingStage.Accepted;

			Transactions.AddRange(accepted);

			Statistics.TransactionsProcessing.End();

			return accepted;
		}
		
 		public void ProcessIncoming(IEnumerable<Request> messages)
 		{

 		}

		void Broadcast(Packet packet, Peer skip = null)
		{
			if(packet != null)
				foreach(var i in Connections.Where(j => j != skip))
				{
					if(packet.Type == PacketType.Blocks)
					{
						if(i.ChainRank > 0)
							i.Send(packet);
					}
					else
						i.Send(packet);
				}
		}

		public Dci ConnectToMember(Workflow workflow)
		{
			if(Generator != null)
			{
				return this;
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
							RememberPeers(cr.Members);

							peer.ReachFailures = 0;
	
							Members = cr.Members.ToList();
								
							var c = Connections.FirstOrDefault(i => Members.Any(j => i.IP.Equals(j.IP)));

							if(c == null)
								continue;
			
							c.Generator = Members.Find(i => c.IP.Equals(i.IP)).Generator;

							Log?.Report(this, "Member chosen", c.ToString());
		
							return c;
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
						continue;
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

		public R Call<R>(Role role, Func<Peer, R> call, Workflow workflow)
		{
			var exclusions = new HashSet<Peer>();

			Peer peer;
				
			while(true)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = FindBestPeer(role, exclusions);
	
					if(peer == null)
						continue;
				}

				exclusions?.Add(peer);

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

			throw new DistributedCallException("No valid nodes found");
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
			return Chain != null ? Operation.CalculateFee(Chain.LastConfirmedRound.Factor, operations) : Coin.Zero;
		}

		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, PrivateAccount signer, PlacingStage awaitstage, Workflow workflow)
		{
			Emission l;

			if(Chain != null)
				lock(Lock)
					l = Chain.Accounts.FindLastOperation<Emission>(signer);
			else
				l = Connect(Role.Chain, null, workflow).GetLastOperation(signer, typeof(Emission).Name).Operation as Emission;
			
			var eid = l == null ? 0 : l.Eid + 1;

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
				var l = Chain.Accounts.FindLastOperation<Emission>(signer);
				var eid = l == null ? 0 : l.Eid + 1;

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

		public Download DownloadRelease(ReleaseAddress package, Workflow workflow)
		{
			lock(Lock)
			{
				var d = new Download(this, package, workflow);
	
				Downloads.Add(d);
		
				return d;
			}
		}

		public PackageStatus GetDownloadStatus(PackageAddress package)
		{
			lock(Lock)
			{
				var d = Downloads.Find(i => i.Package == package);

				if(d != null)
				{
					return new PackageStatus{Length = d.Length, CompletedLength = d.CompletedLength};
				}

				if(Filebase.Exists(package))
				{
					return new PackageStatus{Length = Filebase.GetLength(package), CompletedLength = Filebase.GetLength(package)};
				}

				return new PackageStatus();
			}
		}

		public void DeclarePackage(IEnumerable<PackageAddress> packages, Workflow workflow)
		{
			var hubs = new HashSet<Peer>();

			int success = 0;
			int failures = 0;

			while(success < 8 && success + failures < Peers.Count(i => i.GetRank(Role.Hub) > 0))
			{
				workflow.ThrowIfAborted();

				Peer h = null;

				try
				{
					h = Connect(Role.Hub, hubs, workflow);
					success++;
				}
				catch(ConnectionFailedException)
				{
					failures++;
					continue;
				}

				h.DeclarePackage(packages);

				hubs.Add(h);

				workflow?.Log?.Report(this, "Package declared", $"N={packages.Count()} Hub={h.IP}");
			}
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
