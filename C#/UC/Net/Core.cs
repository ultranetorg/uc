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

	public class Download
	{
		public PackageAddress				Package;
		public Dictionary<Peer, List<Peer>> HubsSeeders = new();
		public Task							Task;
		public byte[]						Hash;
		public long							TotalLength;
		public long							CurrentLength;
		public bool							Succeeded => TotalLength > 0 && CurrentLength == TotalLength;
	}

	public enum Synchronization
	{
		Null, Downloading, Synchronizing, Synchronized
	}

	[Flags]
	public enum Role : uint
	{
		Chain	= 0b00000001,
		Seeder	= 0b00000010,
		Hub		= 0b00000100,
	}
 
	public class Core : Nci
	{
		public static readonly int[]					Versions = {1};
		public const string								FailureExt = "failure";
		public const int								Timeout = 15000;
		public const int								OperationsQueueLimit = 1000;
		public const string								SettingsFileName = "Settings.xon";
		const int										BalanceWidth = 24;

		public Log										Log;
		public Vault									Vault;
		public INas										Nas;
		public Roundchain								Chain;
		public Filebase									Filebase;
		public Hub										Hub;
		RocksDb											Database;
		public bool										IsNodee => ListeningThread != null && DelegatingThread != null;
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

		public List<Peer>								Peers		= new();
		public IEnumerable<Peer>						Connections	=> Peers.Where(i => i.Established);
		public List<IPAddress>							IgnoredIPs	= new();
		public List<Block>								Cache		= new();
		public List<Peer>								Members		= new();
		//Dictionary<PackageAddress, List<IPAddress>>		PackagePeers = new();
		List<Download>									Downloads = new();

		TcpListener										Listener;
		Thread											ListeningThread;
		Thread											DelegatingThread;
		Thread											VerifingThread;
		object											RemoteMemberLock = new object();

		JsonServer										ApiServer;

		public bool										Working => Running && (Abort == null || !Abort());
		bool											Running = true;
		Func<bool>										Abort;
		public Synchronization							_Synchronization = Synchronization.Null;
		public Synchronization							Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate								SynchronizationChanged;
		DateTime										SyncRequested;
		int												SyncStart = -1;
		int												SyncEnd = -1;

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
															
				f.Add("Zone");					v.Add(Settings.Zone);
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

			Directory.CreateDirectory(Settings.Profile);

			Log	 = Settings.Log ? log : null;
			Log?.Report(this, $"Ultranet Node/Client {Assembly.GetEntryAssembly().GetName().Version}");
			Log?.Report(this, $"Runtime: {Environment.Version}");	
			Log?.Report(this, $"Profile: {Settings.Profile}");	
			Log?.Report(this, $"Protocol Versions: {string.Join(',', Versions)}");
			Log?.Report(this, $"Zone: {Settings.Zone}");
			
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

		public void RunNode(Func<bool> abort = null)
		{
			Abort = abort;
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
		 			Log.ReportError(this, $"Corresponding configuration file is located here: {Path.Join(Settings.Profile, SettingsFileName)}");
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

			ListeningThread = new Thread(Listening);
			ListeningThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Listening";
			ListeningThread.Start();

			DelegatingThread = new Thread(Delegating);
			DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
			DelegatingThread.Start();

 			var t = new Thread(	() =>
 								{ 
									Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

									try
									{
										while(Working)
										{
											lock(Lock)
											{
												if(!Working)
													break;

												MaintainPeerConnections();
												
												if(Chain != null)
												{
													Synchronize();

													if(Synchronization == Synchronization.Synchronized && Generator != null)
													{
														Generate();
													}
												}
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

			if(Abort != null && Abort())
			{
				throw new AbortException();
			}
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

			Log?.Report(this, "Stopped", "Cause=" + message);
		}

		Peer GetPeer(IPAddress ip)
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
				while(Working)
				{
					try
					{
						var initials = Nas.GetInitials(Settings.Zone);

						if(initials.Any())
						{
							RememberPeers(initials.Select(i => new Peer(i){LastSeen = DateTime.UtcNow}));

							Log?.Report(this, "Initial nodes retrieved", initials.Count.ToString());
							break;
						}
						else
							throw new RequirementException($"No initial peers found for zone '{Settings.Zone}'");

					}
					catch(Exception ex) when (ex is not RequirementException)
					{
						Log.ReportError(this, "Can't retrieve initial peers. Retrying in 5 sec...", ex);
						Thread.Sleep(5000);
					}
				}
			}
		}

		void RememberPeers(IEnumerable<Peer> peers)
		{
			var news = peers.Where(i => !i.IP.Equals(IP) && !Peers.Any(j => j.IP.Equals(i.IP))).ToArray();
												
			foreach(var peer in news)
				Peers.Add(peer);

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

		void MaintainPeerConnections()
		{
			var needed = Settings.PeersMin - Peers.Count(i => i.Established || i.InStatus == EstablishingStatus.Initiated || i.OutStatus == EstablishingStatus.Initiated);
		
			foreach(var p in Peers.Where(m =>	(m.InStatus == EstablishingStatus.Null || m.InStatus == EstablishingStatus.Failed) &&
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

			if(Chain != null && Synchronization == Synchronization.Null)
			{
				if(Connections.Count() >= Settings.PeersMin)
				{
					StartSynchronization();
				}
			}
		}

		void Listening()
		{
			try
			{
				Listener = new TcpListener(Settings.IP, Settings.Port);
				Listener.Start();
	
				Log?.Report(this, "Listening started", $"{Settings.IP}:{Settings.Port}");

				while(Working)
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

			h.Roles					= (Chain != null ? Role.Chain : 0) | (Filebase != null ? Role.Seeder : 0) | (Hub != null ? Role.Hub : 0);
			h.Versions				= Versions;
			h.Zone					= Settings.Zone;
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
								client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
								//client.ReceiveTimeout = Timeout;
	
								Hello h = null;
	
								try
								{
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
						if(!Working || !peer.Established)
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

						case PacketType.Rounds:
						{
							Round[] rounds;
	
							try
							{
								rounds = pk.Read(r => new Round(Chain));
							}
							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
							{
								peer.Disconnect();
								break;
							}

							lock(Lock)
							{
								if(rounds.Any())
								{
									var from = rounds.Min(i => i.Id);
									var to = rounds.Max(i => i.Id);
		
									bool confirmed = true;
	
									for(int i = from; i <= to; i++)
									{
										var r = rounds.FirstOrDefault(j => j.Id == i); 
	
										if(r == null && confirmed) /// not all reqested sealed rounds received? a peer must send the whole sealed sequence 
										{
											Synchronization = Synchronization.Downloading;
											SyncStart = i;
											SyncEnd = -1;
											break;
										}
																			
										if(r.Confirmed)
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
											if(confirmed)
											{
												confirmed		= false;
												Synchronization	= Synchronization.Synchronizing;
												SyncEnd			= peer.LastRound;
											}
	
											if(r != null)
											{
												ProcessIncoming(r.Blocks, null);
											}
										}
																	
										SyncStart = i + 1;
									}
							
									Log?.Report(this, "Rounds received", $"{from}..{to}");
								}
								else
								{
									Synchronization	= Synchronization.Synchronizing;
									SyncEnd			= peer.LastRound;
								}

								SyncRequested = DateTime.MinValue;
							}

							break;
						}

						case PacketType.RoundsRequest:
						{
							int from;
							int to;
		
							try
							{
								from	= reader.Read7BitEncodedInt();
								to		= reader.Read7BitEncodedInt();
							}
							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
							{
								peer.Disconnect();
								break;
							}
	
							lock(Lock)
							{
								var rounds = Enumerable.Range(from, to - from + 1).Select(i => Chain.FindRound(i)).Where(i => i != null).ToList();
								peer.Send(Packet.Create(PacketType.Rounds, rounds));
							}
								
							break;
						}

 						case PacketType.Request:
 						{
							Request[] requests;

 							try
 							{
								requests = BinarySerializator.Deserialize(reader,	c => {
																							var o = UC.Net.Request.FromType(Chain, (NciCall)c); 
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
																			t => UC.Net.Response.FromType(Chain, (NciCall)t), 
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
										rq.RecievedResponse = rp;
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
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		void StartSynchronization()
		{
		 	lock(Lock)
		 	{
			 	if(Synchronization != Synchronization.Downloading && Synchronization != Synchronization.Synchronizing)
			 	{
				 	Log?.Report(this, "Syncing started");

					SyncStart = -1;
					SyncEnd	  = -1;
		
					Synchronization = Synchronization.Downloading;
			 	}
		 	}
		}

		void Synchronize()
		{
			if(Synchronization == Synchronization.Synchronized)
			{
				var conns = Connections.GroupBy(i => i.LastConfirmedRound).ToList(); /// Not cool, cause Peer.Established may change after this and 'conn' items will change
	
				if(conns.Any())
				{
					var max = conns.Aggregate((i, j) => i.Count() > j.Count() ? i : j);
						
					if(max.Key - Chain.LastConfirmedRound.Id > Roundchain.Pitch) /// we are late, force to resync
					{
						StartSynchronization();
					}
				}
			}
	
			if(Synchronization == Synchronization.Downloading || Synchronization == Synchronization.Synchronizing)
			{
			 	var peer = Connections.Aggregate((i, j) => i.LastRound > j.LastRound ? i : j);
			 	
				var from = SyncStart != -1 ? SyncStart	: (Chain.LastConfirmedRound.Id + 1);
			 	var to	 = SyncEnd != -1 ?	 SyncEnd	: (from + Math.Min(peer.LastRound - from, Roundchain.Pitch));
		 	
			 	if(from <= to)
			 	{
					if(DateTime.UtcNow - SyncRequested > TimeSpan.FromSeconds(15))
					{
						peer.RequestRounds(Header, from, to); 
							
						SyncRequested = DateTime.UtcNow;
					}
			 	}
				else
				{
					Synchronization = Synchronization.Synchronized;
	
					Chain.Add(Cache.OrderBy(i => i.RoundId));
					Cache.Clear();
	
				 	Log?.Report(this, "Syncing finished");
				}
			}
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

			Nci m = null;

			while(Working)
			{
				Thread.Sleep(1);

				if(m == null)
					m = ConnectToMember();

				try
				{
					Statistics.Delegating.Begin();

					lock(Lock)
					{
						peers = Peers.ToArray();
						pendings = Operations.Where(i => i.Delegation == DelegationStage.Pending).ToArray();
						ready = pendings.Any() && !Operations.Any(i => i.Delegation == DelegationStage.Delegated && i.Placing == PlacingStage.Null);
					}

					if(ready) /// Any pending ops and no delegated cause we first need to recieve a valid block to keep tx id sequential correctly
					{
						var txs= new List<Transaction>();

						var rmax = m.Send(new NextRoundRequest()).NextRoundId;

						lock(Lock)
						{
							foreach(var g in pendings.GroupBy(i => i.Signer))
							{
								int id = 1;

								if(!Vault.OperationIds.ContainsKey(g.Key))
								{
									Monitor.Exit(Lock);
									var o = m.Send(new LastOperationRequest {Account = g.Key}).Operation;
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
	
						var accepted = m.Send(new DelegateTransactionsRequest {Transactions = txs }).Accepted;
	
						lock(Lock)
							foreach(var o in txs.SelectMany(i => i.Operations))
							{
								if(accepted.Any(i => i.Account == o.Signer && i.Id == o.Id))
 									o.Delegation = DelegationStage.Delegated;

								o.FlowReport?.StageChanged();
								o.FlowReport?.Log?.ReportWarning(this, $"Placing has been delegated to {m}");
							}
									
						Log?.Report(this, "Operation(s) delegated", $"{txs.Sum(i => i.Operations.Count(o => accepted.Any(i => i.Account == o.Signer && i.Id == o.Id)))} op(s) in {accepted.Count()} tx(s) -> {m.Generator} {(m is Peer p ? p.IP : "Self")}");

						Thread.Sleep(500); /// prevent any flooding
					}

					lock(Lock)
						delegated = Operations.Where(i => i.Delegation == DelegationStage.Delegated).ToArray();
	
					if(delegated.Any())
					{
						var rp = m.Send(new GetOperationStatusRequest {Operations = delegated.Select(i => new OperationAddress {Account = i.Signer, Id = i.Id}).ToArray()});
							
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
										o.FlowReport?.StageChanged();
									}
								}
							}
						}
					}

					Statistics.Delegating.End();
				}
				catch(Exception ex) when (ex is AggregateException || ex is HttpRequestException || ex is RpcException)
				{
					Log?.ReportError(this, $"Failed to communicate with remote node {m}", ex);

					if(m.Failures < 3)
						m.Failures++;
					else
						m = null;

					Thread.Sleep(1000); /// prevent any flooding
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

		public void ReachConsensus()
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
		
		public Operation Enqueue(Operation operation, IFlowControl flowcontrol = null)
		{
			if(FeeAsker.Ask(this, operation.Signer as PrivateAccount, operation))
			{
				lock(Lock)
				 	Enqueue(operation);

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
				while(Working)
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
						if((i.Role & Role.Chain) != 0)
							i.Send(packet);
					}
					else
						i.Send(packet);
				}
		}

		public Nci ConnectToMember()
		{
			lock(RemoteMemberLock)
			{
				if(Generator != null)
				{
					return this;
				}

				Peer peer;
				
				while(Working)
				{
					Thread.Sleep(1);
	
					lock(Lock)
					{
						peer = Peers.OrderByDescending(i => i.Established).ThenBy(i => i.ReachFailures).FirstOrDefault();
	
						if(peer == null)
							continue;
					}
	
					try
					{
						var cr = peer.Send(new GetMembersRequest());
	
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
					catch(Exception ex) when (ex is AggregateException || ex is HttpRequestException || ex is RpcException || ex is OperationCanceledException)
					{
						peer.ReachFailures++;
					}
				}
	
				throw new OperationCanceledException("Looking for a member to connect has been aborted because of overral stopping");
			}
		}

		public Peer Connect(Role role, HashSet<Peer> exclusions, CancellationToken cancellation)
		{
			Peer peer;
				
			while(Working && !cancellation.IsCancellationRequested)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					peer = Peers.Where(i => i.Role.HasFlag(role) && (exclusions == null || !exclusions.Contains(i))).OrderByDescending(i => i.Established).ThenBy(i => i.ReachFailures).FirstOrDefault();
	
					if(peer == null)
						continue;
				}

				exclusions?.Add(peer);

				try
				{
					Connect(peer, cancellation);
	
					return peer;
				}
				catch(OperationCanceledException ex)
				{
					if(ex.CancellationToken.IsCancellationRequested)
						throw;
				}
			}

			throw new OperationCanceledException("Looking for a node to connect has been aborted");
		}

//  		public Nci[] ConnectMany(Role role, int min, int desired, IEnumerable<Peer> exclusions, CancellationToken cancellation)
//  		{
//  			Peer[] peers;
//  			
//  			while(Working)
//  			{
//  				Thread.Sleep(1);
// 
// 				var tasks = new List<Task>();
//  	
//  				lock(Lock)
//  				{
//  					peers = Peers.Where(i => exclusions == null || !exclusions.Contains(i)).OrderByDescending(i => i.Established && i.Role.HasFlag(role)).ThenBy(i => i.ReachFailures).Take(desired).ToArray();
//  					
//  					if(!peers.Any())
//  						continue;
//  
//  					foreach(var i in peers)
//  					{
//  						var t = Task.Run(() =>
//  										 {
// 												try
// 												{
// 		 											Connect(i, cancellation);
// 												}
// 												catch(ConnectionFailedException)
// 												{
// 												}
// 												catch(OperationCanceledException)
// 												{
// 												}
//  										 });
//  
//  						tasks.Add(t);
//  					}
//  				}
//  
//  				Task.WaitAll(tasks.ToArray());
//  
//  				if(peers.Count(i => i.Established) >= min)
//  					return peers.Where(i => i.Established).ToArray();
//  			}
//  
//  			throw new OperationCanceledException("Looking for a node to connect has been aborted because of overall stopping");
//  		}

		void Connect(Peer peer, CancellationToken cancellation)
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

			while(Working && !cancellation.IsCancellationRequested)
			{
				lock(Lock)
					if(peer.Established)
						return;
					else if(peer.OutStatus == EstablishingStatus.Failed)
						throw new ConnectionFailedException("Connecting to a node has failed");

				Thread.Sleep(1);
			}

			throw new OperationCanceledException("Connecting to a node has been aborted", cancellation);
		}


		public Coin EstimateFee(IEnumerable<Operation> operations)
		{
			return Chain != null ? Operation.CalculateFee(Chain.LastConfirmedRound.Factor, operations) : Coin.Zero;
		}

		public async Task<Emission> Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, PrivateAccount signer, IFlowControl flowcontrol = null, CancellationTokenSource cts = null)
		{
			Emission l;

			if(Chain != null)
				lock(Lock)
					l = Chain.Accounts.FindLastOperation<Emission>(signer);
			else
				l = await Task.Run<Emission>(() => Connect(Role.Chain, null, cts != null ? cts.Token : CancellationToken.None).Send(new LastOperationRequest(){
																																							Account = signer, 
																																							Class = typeof(Emission).Name
																																						}).Operation as Emission);
			var eid = l == null ? 0 : l.Eid + 1;

			await Nas.Emit(a, wei, signer, GasAsker, eid, flowcontrol, cts);		
						
			var o = new Emission(signer, wei, eid);

			flowcontrol?.SetOperation(o);
						
			if(FeeAsker.Ask(this, signer, o))
			{
				lock(Lock)
					Enqueue(o);
	
				flowcontrol?.Log?.Report(this, "State changed", $"{o} is queued for placing and confirmation");
						
				return o;
			}

			return null;
		}

		public Emission FinishTransfer(PrivateAccount signer, IFlowControl flowcontrol = null)
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

		public void Downloading()
		{
		}

		public Download DownloadPackage(PackageAddress package, CancellationToken cancellation)
		{
			var d = new Download{Package = package};

			Downloads.Add(d);

			d.Task = Task.Run(	() =>
								{
									var c = Connect(Role.Chain, null, cancellation);
									var qrrp = c.QueryRelease(package, VersionQuery.Exact, "", true);

									d.TotalLength = qrrp.Manifests.First().GetInt64(package.Distribution switch {	Distribution.Complete => ReleaseManifest.CompleteSizeField, 
																													Distribution.Incremental => ReleaseManifest.IncrementalSizeField, 
																													_ =>  throw new RequirementException("Wrong Distribution value") });
									
									d.Hash = qrrp.Manifests.First().Get<byte[]>(package.Distribution switch {	Distribution.Complete => ReleaseManifest.CompleteHashField, 
																												Distribution.Incremental => ReleaseManifest.IncrementalHashField, 
																												_ =>  throw new RequirementException("Wrong Distribution value") });

									lock(Lock)
									{
										if(Filebase.GetLength(package) == d.TotalLength && Filebase.GetHash(package).SequenceEqual(d.Hash))
										{	
											d.CurrentLength = d.TotalLength;
											return;
										}
									}

									var hubs = new HashSet<Peer>();

									using(var cts = new CancellationTokenSource())
									{
										while(Working)
										{
											var lcts = CancellationTokenSource.CreateLinkedTokenSource(cancellation, cts.Token);
																						
											Peer h;
											LocatePackageResponse lp;

											try
											{
												if(!Settings.Dev.DisableTimeouts)
													lcts.CancelAfter(5 * 1000);
												
												h = Connect(Role.Hub, hubs, lcts.Token);

												lp = h.LocatePackage(package, 16);

												if(!d.HubsSeeders.ContainsKey(h))
													d.HubsSeeders[h] = new();
											}
											catch(ConnectionFailedException)
											{
												continue;
											}
											catch(RpcException)
											{
												continue;
											}
											catch(OperationCanceledException)
											{
												if(cancellation.IsCancellationRequested)
													throw;
												else
													continue;	
											}
						
											foreach(var s in lp.Seeders.Select(i => GetPeer(i)))
											{
												try
												{
													if(!Settings.Dev.DisableTimeouts)
														lcts.CancelAfter(5 * 1000);
													
													Connect(s, lcts.Token);
													
													d.CurrentLength = Filebase.GetLength(package);

													while(d.CurrentLength < d.TotalLength)
													{
														byte[] data = null;

														for(int j=0; j<3; j++)
														{
															try
															{
																data = s.DownloadPackage(package, d.CurrentLength, Math.Min(65536, d.TotalLength - d.CurrentLength)).Data;
																break;
															}
															catch(RpcException)
															{
															}
														}
	
														lock(Lock)
															if(data != null)
															{
																Filebase.Append(package, data);
																d.CurrentLength += data.LongLength;

																if(!d.HubsSeeders[h].Contains(s)) /// this hub gave a good seeder
																	d.HubsSeeders[h].Add(s);
															}
															else
																break;
													}
				
													if(d.CurrentLength == d.TotalLength && Filebase.GetHash(package).SequenceEqual(d.Hash))
													{
														if(d.HubsSeeders[h].Any())
															h.HubHits++;

														RememberPeers(new[]{h});

														return; /// Success
													}
												}
												catch(ConnectionFailedException)
												{
													continue;
												}
												catch(RpcException)
												{
													continue;
												}
												catch(OperationCanceledException)
												{
													if(cancellation.IsCancellationRequested)
														throw;
												}
											}

											h.HubMisses++;

											RememberPeers(new[]{h});
										}
									}
								},
								cancellation);

			return d;
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
	}
}
