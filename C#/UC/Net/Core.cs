﻿using System;
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

	public enum Synchronization
	{
		Null, Downloading, Synchronizing, Synchronized
	}
 
	public class Core
	{
		public static readonly int[]					Versions = {1};
		public const string								FailureExt = "failure";
		//public const int								DefaultPort = 3080;
		public const int								Timeout = 15000;
		public const int								OperationsQueueLimit = 1000;
		public const string								SettingsFileName = "Settings.xon";

		const int										BalanceWidth = 24;

		public Log										Log;
		public Vault									Vault;
		public INas										Nas;
		public Roundchain								Chain;
		RocksDb											Database;
		public bool										IsNode => ListeningThread != null;
		public bool										IsClient => DelegatingThread != null;
		public object									Lock = new();
		public Settings									Settings;
		TimeProvider									TimeProvider;

		public PrivateAccount							Generator;
		CandidacyDeclaration							Declaration;
		public Guid										Session;
		public IPAddress								IP = IPAddress.None;

		public Statistics								PrevStatistics = new();
		public Statistics								Statistics = new();

		public List<Change>								Changes = new();
		public List<Operation>							Operations	= new();
		public List<Message>							Messages	= new();

		public List<Peer>								Peers		= new();
		public IEnumerable<Peer>						Connections	=> Peers.Where(i => i.Established);
		public List<IPAddress>							IgnoredIPs	= new();
		public List<Block>								Cache		= new();
		public List<Peer>								Members		= new();
		//List<ReleaseDeclaration>						ReleaseDeclarations = new();

		TcpListener										Listener;
		Thread											ListeningThread;
		Thread											DelegatingThread;
		Thread											VerifingThread;
		public Peer										RemoteMember;
		object											RemoteMemberLock = new object();

		JsonServer										ApiServer;
		HttpClient										HttpClient;

		public bool										Working => Running && (Abort == null || !Abort());
		bool											Running = true;
		Func<bool>										Abort;
		public Synchronization							_Synchronization = Synchronization.Null;
		public Synchronization							Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate								SynchronizationChanged;
		DateTime										SyncRequested;
		int												SyncStart = -1;
		int												SyncEnd = -1;

		IGasAsker										GasAsker; 
		IFeeAsker										FeeAsker;

		public ColumnFamilyHandle						PeersFamily => Database.GetColumnFamily(nameof(Peers));
		//public ColumnFamilyHandle						ReleasesFamily => Database.GetColumnFamily(nameof(Releases));

		readonly DbOptions								DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																					.SetCreateMissingColumnFamilies(true);

		public string[][] Info
		{
			get
			{
				List<string> f = new();
				List<string> v = new(); 
															
				f.Add("Zone");					v.Add(Settings.Zone);
				f.Add("Profile");				v.Add(Settings.Profile);
				f.Add("IP(Reported):Port");		v.Add($"{Settings.IP} ({IP}) : {Settings.Port}");
				f.Add($"Generator{(RemoteMember != null ? " (delegation)" : "")}");	v.Add($"{(Generator ?? RemoteMember?.Generator)}");
				f.Add("Operations");			v.Add($"{Operations.Count}");
				f.Add("    pending");			v.Add($"{Operations.Count(i => i.Stage == ProcessingStage.Pending)}");
				f.Add("    delegated");			v.Add($"{Operations.Count(i => i.Stage == ProcessingStage.Delegated)}");
				f.Add("    placed");			v.Add($"{Operations.Count(i => i.Stage == ProcessingStage.Placed)}");
				f.Add("Transactions");			v.Add($"{Changes.Count}");
				f.Add("    pending");			v.Add($"{Changes.Count(i => i.Stage == ProcessingStage.Pending)}");
				f.Add("    placed");			v.Add($"{Changes.Count(i => i.Stage == ProcessingStage.Placed)}");

				if(Chain != null)
				{
					f.Add("Synchronization");		v.Add($"{Synchronization}");
					f.Add("Peers in/out/min/known");v.Add($"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}");
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
						return GetAccountInfo(a, false)?.Balance.ToHumanString();
					}

					foreach(var i in Vault.Accounts)
					{
						f.Add($"Account");	v.Add($"{i.ToString().Insert(6, "-")} {formatbalance(i, true), BalanceWidth}");
					}
	
					if(Settings.Dev.UI)
					{
						f.Add("NAS Eth Account");		v.Add($"{Nas.Account?.Address}");

						foreach(var i in Chain.FindFundables(Chain.LastConfirmedRound))
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
		
		Header Header
		{
			get
			{
				Header h;

				lock(Lock)
				{
					h =	new Header
						{ 
							LastRound			= Chain.LastNonEmptyRound.Id,
							LastConfirmedRound	= Chain.LastConfirmedRound.Id,
						};
				}

				return h;
			}
		}

		public Core(Settings settings, string exedirectory, Log log, TimeProvider timeprovider, INas nas, IGasAsker gasasker, IFeeAsker feeasker)
		{
			Settings = settings;
			TimeProvider = timeprovider;
			GasAsker = gasasker;
			FeeAsker = feeasker;

			Directory.CreateDirectory(Settings.Profile);

			Log	 = Settings.Log ? log : null;
			Log?.Report(this, $"Ultranet Node/Client {Assembly.GetEntryAssembly().GetName().Version}");
			Log?.Report(this, $"Runtime: {Environment.Version}");	
			Log?.Report(this, $"Profile: {Settings.Profile}");	
			Log?.Report(this, $"Protocol Versions: {string.Join(',', Versions)}");
			Log?.Report(this, $"Zone: {Settings.Zone}");
			
			if(Settings.Dev.Any)
				Log?.ReportWarning(this, $"Dev: {Settings.Dev}");

			Vault		= new Vault(Settings, Log);
			Nas			= nas;
			HttpClient	= new HttpClient{Timeout = TimeSpan.FromSeconds(5)};
		}

		public override string ToString()
		{
			return $"{Settings.IP} {Synchronization}";
		}

		public void RunClient(Action<Settings, Vault> overridefinal = null, Func<bool> abort = null)
		{
			Abort		= abort;

			var descs = new ColumnFamilies.Descriptor[]	{
															new (nameof(Peers), new ()),
														};
			
			var cfamilies = new ColumnFamilies();
			
			foreach(var i in descs)
				cfamilies.Add(i);


			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Client"), cfamilies);

			overridefinal?.Invoke(Settings, Vault);

			LoadPeers();

			DelegatingThread = new Thread(Delegating);
			DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
			DelegatingThread.Start();

			Task.Run(() =>	{
								while(Working && RemoteMember == null) 
								{
									Thread.Sleep(100); 
								}
							}).Wait();

			if(RemoteMember == null && Abort != null && Abort())
			{
				throw new AbortException();
			}
		}

		public void RunServer()
		{
			if(!HttpListener.IsSupported)
			{
				Environment.ExitCode = -1;
				throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
			}

			ApiServer = new JsonServer(this);
		}

		public void RunNode()
		{
			Log?.Report(this, "Running");

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

			var descs = new ColumnFamilies.Descriptor[]	{
															new(nameof(Peers), new ()),

															new (nameof(Members), new ()),
															new (nameof(Roundchain.Accounts), new ()),
															new (nameof(Roundchain.Authors), new ()),
															new (nameof(Roundchain.Products), new ()),
															new (nameof(Roundchain.Rounds), new ()),
															new (nameof(Roundchain.Fundables), new ()),
														};
			
			var cfamilies = new ColumnFamilies();
			
			foreach(var i in descs)
				cfamilies.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Node"), cfamilies);
			Chain = new Roundchain(Settings, Log, Nas, Vault, Database);
			Session = Guid.NewGuid();

			if(Settings.Generator != null)
			{
				Generator = PrivateAccount.Parse(Settings.Generator);
				Declaration = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(Generator);
			}

			Chain.BlockAdded += b =>{
										if(Settings.Generator != null)
											Declaration = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(Generator);
			
										ReachConsensus();
									};

			ListeningThread = new Thread(Listening);
			ListeningThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Listening";
			ListeningThread.Start();

			DelegatingThread = new Thread(Delegating);
			DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
			DelegatingThread.Start();

			if(Generator != null)
			{
				VerifingThread = new Thread(Verifing);
				VerifingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Verifing";
				VerifingThread.Start();
			}


			void main()
			{
				Thread.CurrentThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Main";

				try
				{
					LoadPeers();
									
					var time = DateTime.MinValue;
	
					while(Working)
					{
						lock(Lock)
						{
							if(!Working)
								break;

							Connect();
							Synchronize();
	
							if(Synchronization == Synchronization.Synchronized && Generator != null)
							{
								Generate();
							}
						}
	
						if(Settings.Telemetry)
						{
							if(DateTime.UtcNow - time > TimeSpan.FromMilliseconds(1000))
							{
								time = DateTime.UtcNow;
							}
						}
	
						Thread.Sleep(1);
					}
	
					ListeningThread?.Join();
					DelegatingThread?.Join();
					ApiServer?.WaitStop();
				}
				catch(Exception ex) when (!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}

			var t = new Thread(main);
			t.Start();
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

			Log?.Report(this, "Stopped", "Cause=" + message);

			Running = false;
			Listener?.Stop();
			ApiServer?.Stop();

			lock(Lock)
			{
				foreach(var i in Peers.Where(i => i.Established))
					i.Disconnect();
			}

			Database?.Dispose();

			Log?.Report(this, null, "Stopped");
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
							RememberPeers(initials.Select(i => new Peer(i){ LastSeen = DateTime.UtcNow }));

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
			peers = peers.Where(i => !i.IP.Equals(IP) && !Peers.Any(j => j.IP.Equals(i.IP))).ToArray();
												
			foreach(var peer in peers)
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

				//using(var i = Database.NewIterator(PeersFamily))
				//{
				//	for(i.SeekToFirst(); i.Valid(); i.Next())
				//	{
				//		if(DateTime.UtcNow - DateTime.FromBinary(BitConverter.ToInt64(i.Value())) < TimeSpan.FromDays(365))
				//		{
				//			b.Delete(i.Key(), PeersFamily);
				//		}
				//	}
				//}

				Database.Write(b);
			}
		}

		void Connect()
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

			if(Synchronization == Synchronization.Null)
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
						Peer.SendHello(client, Versions, Settings.Zone, Session, peer.IP, peers, Header);
						h = Peer.WaitHello(client);
					}
					catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
					{
						goto failed;
					}
	
					lock(Lock)
					{
						if(h.Session == Session)
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
								catch(Exception ex) when(!Settings.Dev.ThrowOnCorrupted)
								{
									//Log.ReportWarning(this, "Inbound error", "WaitHello failed", ex);
									goto failed;
								}
				
								lock(Lock)
								{
									if(h.Session == Session)
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
										Peer.SendHello(client, Versions, Settings.Zone, Session, ip, Peers, Header);
									}
									catch(Exception ex) when(!Settings.Dev.ThrowOnCorrupted)
									{
										//Log.ReportWarning(this, "Inbound error", "SendHello failed", ex);
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
					var rq = peer.Read();

					if(rq == null)
					{
						lock(Lock)
							peer.Status = ConnectionStatus.Failed;
						return;
					}

					lock(Lock)
						if(!Working || !peer.Established)
							return;

					switch(rq.Type)
					{
						case PacketType.Blocks:
						{
							IEnumerable<Block> blocks;
										
							try
							{
								blocks = Read(rq.Data, (r, t) => Block.FromType(Chain, (BlockType)t));
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
								rounds = Read(rq.Data, r => new Round(Chain));
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
		
									bool seal = Chain.LastConfirmedRound.Id == from - 1;
	
									for(int i = from; i <= to; i++)
									{
										var r = rounds.FirstOrDefault(j => j.Id == i); 
	
										if(r == null && seal) /// not all reqested sealed rounds received? a peer must send the whole sealed sequence 
										{
											Synchronization = Synchronization.Downloading;
											SyncStart = i;
											SyncEnd = -1;
											break;
										}
																			
										if(seal && r.Confirmed)
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
											if(seal)
											{
												seal			= false;
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
								var reader = new BinaryReader(rq.Data); 

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
								peer.Send(Header, PacketType.Rounds, Write(rounds));
							}
								
							break;
						}

// 						case PacketType.Message:
// 						{
// 							IEnumerable<Message> messages;
// 										
// 							try
// 							{
// 								messages = Read(rq.Data, (r, t) => Message.FromType(Chain, (MessageType)t));
// 							}
// 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
// 							{
// 								peer.Disconnect();
// 								break;
// 							}
// 		
// 							lock(Lock)
// 							{
// 								ProcessIncoming(messages, peer);
// 							}
// 	
// 							break;
// 						}

						default:
							Log?.ReportError(this, $"Wrong packet type {rq.Type}");
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

				var joins = inrange.OfType<JoinRequest>().Where(b => { 
																		var d = Chain.Accounts.FindLastOperation<CandidacyDeclaration>(b.Member);
														
																		if(d == null)
																			return false;

																		for(int i = b.RoundId; i > b.RoundId - Roundchain.Pitch; i--) /// not often than 1 request per [Pitch] rounds
																			if(Chain.GetRound(i).JoinRequests.Any(i => i.Member == b.Member))
																				return false;

																		if(Chain.GetRound(b.RoundId).JoinRequests.Count() < Roundchain.MembersMax) /// keep  maximum MembersMax requests per round
																			return true;

																		var min = Chain.GetRound(b.RoundId).JoinRequests.Aggregate((i, j) => i.Declaration.Bail < j.Declaration.Bail ? i : j);
														
																		return min.Declaration.Bail < d.Bail; /// if a number of members are Max then accept only those requests that have a bail greater than the existing request with minimal bail
																	});
				Chain.Add(joins);
					
				var votes = inrange.Where(b => b is UC.Net.Vote v && (Chain.Members.Any(j => j.Generator == b.Member) || 
																	 (Chain.Rounds.Any(r => r.Members == null ? false : r.Members.Any(m => m.Generator == b.Member)))));

				Chain.Add(votes);

				//foreach(var o in Operations.Where(i => i.Stage == ProcessingStage.Delegated))  /// Mark all previously delegated operations as Placed if a block contaning them has arrived
				//{
				//	if(votes.OfType<Payload>().Any(b => b.Member == o.Transaction.Member && b.Transactions.Any(t => t.SignatureEquals(o.Transaction))))
				//		o.Stage = ProcessingStage.Placed;
				//}
			}

			if(Synchronization == Synchronization.Null || Synchronization == Synchronization.Synchronizing || Synchronization == Synchronization.Synchronized) /// Null and Synchronizing needed for Dev purposes
			{
				Broadcast(PacketType.Blocks, Write(accepted), peer);
			}

			Statistics.BlocksProcessing.End();

		}

		public Round GetNextAvailableRound()
		{
			var r = Chain.GetRound(Chain.LastVotedRound.Id + 1);

			while(r.Blocks.Any(i => i.Member == Generator))
				r = Chain.GetRound(r.Id + 1);
	
			if(r.Id > Chain.LastVotedRound.Id + Roundchain.Pitch)
				return null;

			return r;
		}

		List<Transaction> BuildTransactions(Account member, int rmax, Func<Account, int> nexttxid)
		{
			var l = new List<Transaction>();

			foreach(var g in Operations.Where(i => i.Stage == ProcessingStage.Pending).GroupBy(i => i.Signer))
			{
				var t = new Transaction(Settings, g.Key as PrivateAccount, nexttxid(g.Key));

				foreach(var o in g)
				{
					o.Transaction = t;
					t.Operations.Add(o);
				}

				t.Sign(member, rmax);
				l.Add(t);
			}

			return l;
		}

		List<Proposition> BuildPropositions(Account member, int rmax)
		{
			var l = new List<Proposition>();

			foreach(var g in Messages.Where(i => i.Stage == ProcessingStage.Pending).GroupBy(i => i.Signer))
			{
				var p = new Proposition(Settings, g.Key as PrivateAccount);

				foreach(var o in g)
				{
					o.Proposition = p;
					p.Messages.Add(o);
				}

				p.Sign(member, rmax);
				l.Add(p);
			}

			return l;
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
					var jr = Chain.FindLastBlock(i => i is JoinRequest && i.Member == Generator);
	
					if(jr == null || (Chain.LastVotedRound.Id - jr.RoundId > Roundchain.Pitch * 2)) /// to be elected we need to wait [Pitch] rounds for voting and [Pitch] rounds to confirm votes
					{
						var b = new JoinRequest(Chain)
									{
										RoundId		= nar.Id,
										IP			= IP
									};

						votes.Add(b);
					}
				}
				else
				{
					var txs = Chain.CollectValidTransactions(Changes.OfType<Transaction>()
																	.Where(i => i.Stage == ProcessingStage.Pending && i.RoundMax >= nar.Id)
																	.GroupBy(i => i.Signer)
																	.Select(i => i.First()), nar);

					var msgs = BuildPropositions(Generator, nar.Id).Union(Changes	.OfType<Proposition>()
																					.Where(i => i.Stage == ProcessingStage.Pending && i.RoundMax >= nar.Id)
																					.GroupBy(i => i.Signer)
																					.Select(i => i.First())).ToList();
					//var msg = Transactions.OfType<Message>();

					var prev = Chain.FindRound(nar.Id - 1).Votes.FirstOrDefault(i => i.Member == Generator);

					if(txs.Any()) /// any pending foreign transactions or any our pending operations
					{
						var p = Chain.FindRound(nar.ParentId);
		
						var rr = Chain.Refer(p);

						if(rr == null)
							return;
				
						var b = new Payload(Chain)
								{
									RoundId				= nar.Id,
									Try					= nar.Try,
									Reference			= rr,
									Time				= TimeProvider.Now,
									TimeDelta			= prev == null ? 0 : (long)(TimeProvider.Now - prev.Time).TotalMilliseconds,
									Violators			= p.Forkers.ToList(),
									Joiners				= Chain.ProposeJoiners(nar).ToList(),
									Leavers				= Chain.ProposeLeavers(nar, Generator).ToList(),
									FundableAssignments	= new(),
									FundableRevocations	= new(),
									Propositions		= msgs
								};
				
						foreach(var i in txs)
						{
							(b as Payload).AddNext(i);
							i.Stage = ProcessingStage.Placed;
						}
						
						foreach(var i in b.Propositions)
							i.Stage = ProcessingStage.Placed;

						votes.Add(b);
					}
					else
					{
						var r = Chain.Rounds.LastOrDefault(i => !i.Confirmed && !i.Blocks.Any(j => j.Member == Generator));

						while(r != null)
						{

							if(	Chain.VotersFor(r).Any(i => i.Generator == Generator) &&			/// we must vote
								!r.Votes.Any(i => i.Member == Generator) &&						/// no our block or vote yet
								r.Votes.OfType<Payload>().Any() 								/// has already some payloads from other members
								)
							{
								var p = Chain.FindRound(r.ParentId);
								var rr = Chain.Refer(p);
				
								if(rr != null)
								{

									var b = new Vote(Chain)
											{	
												RoundId				= r.Id,
												Try					= r.Try,
												Reference			= rr,
												Time				= TimeProvider.Now,
												TimeDelta			= prev == null ? 0 : (long)(TimeProvider.Now - prev.Time).TotalMilliseconds,
												Violators			= p.Forkers.ToList(),
												Joiners				= Chain.ProposeJoiners(r).ToList(),
												Leavers				= Chain.ProposeLeavers(r, Generator).ToList(),
												FundableAssignments	= new(),
												FundableRevocations	= new(),
												Propositions		= msgs
											};
							
									foreach(var i in b.Propositions)
										i.Stage = ProcessingStage.Placed;
							
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
						b.Sign(Generator);
						Chain.Add(b);
					}

					Broadcast(PacketType.Blocks, Write(votes));
													
					Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{i.Type}({i.RoundId})")));
				}
			}

			Statistics.Generating.End();
		}

		Peer GetRemoteMember()
		{
			lock(RemoteMemberLock)
			{
				lock(Lock)
				{
					if(RemoteMember != null && RemoteMember.Api.Failures <= 3)
						return RemoteMember;
				}

				if(Generator != null)
				{
					while(Working)
					{ 
						if(IP.Equals(IPAddress.None))
							Thread.Sleep(1);
						else
							break;
					}

					lock(Lock)
					{
						RemoteMember = new Peer(IP){ Generator = Generator };
						RemoteMember.Api = new JsonClient(HttpClient, $"http://{RemoteMember.IP}:{Zone.RpcPort(Settings.Zone)}", null);
						return RemoteMember;
					}
				}

/*				if(RemoteMember != null && RemoteMember.Api != null && RemoteMember.Api.Failures > 3)
				{
					RemoteMember.ApiReachFailures++;;
				}	*/

				RemoteMember = null;
				Peer peer;
				
				while(Working)
				{
					Thread.Sleep(1);
	
					lock(Lock)
					{
						peer = Peers.OrderByDescending(i => i.Established).ThenBy(i => i.ApiReachFailures).FirstOrDefault();
	
						if(peer == null)
							continue;
							
						if(peer.Api == null)
						{
							peer.Api = new JsonClient(HttpClient, $"http://{peer.IP}:{Zone.RpcPort(Settings.Zone)}", null);
						}
					}
	
					if(peer != null)
						try
						{
							var cr = peer.Api.Send(new GetMembersCall());
	
							lock(Lock)
							{
								if(cr.Members.Any())
								{
									RememberPeers(cr.Members);

									peer.ApiReachFailures = 0;
	
									Members = cr.Members.ToList();
								
									RemoteMember = Members.OrderBy(i => Guid.NewGuid()).First();
									RemoteMember.Api = new JsonClient(HttpClient, $"http://{RemoteMember.IP}:{Zone.RpcPort(Settings.Zone)}", null);
		
									Log?.Report(this, "Member chosen", RemoteMember.ToString());
		
									return RemoteMember;
								}
							}
						}
						catch(Exception ex) when (ex is AggregateException || ex is HttpRequestException || ex is RpcException || ex is OperationCanceledException)
						{
							peer.ApiReachFailures++;
						}
				}
	
				return null;
			}
		}

		void Delegating()
		{
			Peer[]						peers;
			Operation[]					pendings;
			Dictionary<Account, int>	accounts;
			bool						ready;
			IEnumerable<Transaction>	delegated;

			Log?.Report(this, "Delegating started");

			while(Working)
			{
				Thread.Sleep(1);

				var m = GetRemoteMember();

				if(m == null)
					continue;

				try
				{
					Statistics.Delegating.Begin();

					lock(Lock)
					{
						peers = Peers.ToArray();
						pendings = Operations.Where(i => i.Stage == ProcessingStage.Pending).ToArray();
						accounts = pendings.GroupBy(i => i.Signer).Select(i => i.Key).ToDictionary(k => k, v => 0);
						ready = pendings.Any() && Operations.Any(i => i.Stage != ProcessingStage.Delegated);
					}

					if(ready) /// Any pending ops and no delegated cause we first need to recieve a valid block to keep tx id sequential correctly
					{
						foreach(var a in accounts)
						{
							accounts[a.Key] = m.Api.Send(new LastTransactionIdCall {Account = a.Key}).Id + 1;
						}

						IEnumerable<Transaction> txs;

						var rmax = m.Api.Send(new NextRoundCall()).NextRoundId;

						lock(Lock)
							txs = BuildTransactions(m.Generator, Roundchain.GetValidityPeriod(rmax), a => accounts[a]);
	
						var accepted = m.Api.Send(new DelegateTransactionsCall {Data = Write(txs).ToArray()}).Accepted;
	
						lock(Lock)
							foreach(var t in txs.Where(i => accepted.Any(a => a.SequenceEqual(i.Signature))))
							{	
								t.Stage = ProcessingStage.Delegated;

								foreach(var o in t.Operations)
								{
									o.Stage = ProcessingStage.Delegated;
									o.FlowReport?.StageChanged();
									o.FlowReport?.Log.ReportWarning(this, $"Placing has been delegated to {m}");
								}
							}
									
						Log?.Report(this, "Operation(s) delegated", $"{txs.Where(i => accepted.Any(j => j.SequenceEqual(i.Signature))).Sum(i => i.Operations.Count)} op(s) in {accepted.Count()} tx(s) -> {m.Generator} {m.IP}");

						Thread.Sleep(1000); /// prevent any flooding
					}

					lock(Lock)
						delegated = Operations.Where(i => i.Stage == ProcessingStage.Delegated).GroupBy(i => i.Transaction).Select(i => i.Key).ToArray();
	
					if(delegated.Any())
					{
						var rp = m.Api.Send(new GetTransactionsStatusCall {Transactions = delegated.Select(i => new GetTransactionsStatusCall.Item {Account = i.Signer, Id = i.Id})});
							
						if(rp != null)
						{
							lock(Lock)
							{
								var ops = Operations.Where(o =>	o.Stage == ProcessingStage.Delegated && 
																rp.Transactions.Any(s =>s.Account == o.Transaction.Signer && 
																						s.Id == o.Transaction.Id &&
																						s.Confirmed)); /// confirmed operations

								if(ops.Any())
								{
									Log?.Report(this, "Operation(s) confirmed", $"{ops.Count()}");
	
									foreach(var o in ops.ToArray())
									{
										o.Transaction.Stage = ProcessingStage.Confirmed;
										o.Stage = ProcessingStage.Confirmed;
										o.FlowReport?.StageChanged();
										Operations.Remove(o);
									}
								}

								ops = Operations.Where(o => o.Stage == ProcessingStage.Delegated && rp.LastConfirmedRound > o.Transaction.RoundMax); /// outdated

								if(ops.Any())
								{
									Log?.Report(this, "Operation(s) outdated", $"{ops.Count()}");

									foreach(var o in ops.ToArray())
									{
										o.Transaction = null;
										o.Stage = ProcessingStage.Pending;
										o.FlowReport?.StageChanged();
										o.FlowReport?.Log.ReportWarning(this, "Operations was not placed. Redelegating.");
									}
								}
							}
						}
					}

					Statistics.Delegating.End();
				}
				catch(Exception ex) when (ex is AggregateException || ex is HttpRequestException || ex is RpcException || ex is OperationCanceledException)
				{
					m.Api.Failures++;
					Log?.ReportError(this, $"Failed to communicate with remote node {m}", ex);

					Thread.Sleep(1000); /// prevent any flooding
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
					else if(Chain.QuorumFailed(r) || DateTime.UtcNow - r.FirstArrivalTime > TimeSpan.FromSeconds(15))
					{
						//foreach(var i in Operations) /// mark all prevously placed operations 'pending' again
						//{
						//	if(Generator != null && i.Stage == ProcessingStage.Placed && i.Transaction.Payload.RoundId == r.Id) /// i.Stage == ProcessingStage.Placed must be verified first
						//	{
						//		i.Stage = ProcessingStage.Pending;
						//		i.FlowReport?.StageChanged();
						//		i.FlowReport?.Log.ReportWarning(this, "Quorum failed. Replacing/Redelegating required.");
						//		i.Transaction = null;
						//	}
						//}

						foreach(var i in Changes.OfType<Transaction>().Where(i => i.Payload.RoundId == r.Id))
						{
							i.Stage = ProcessingStage.Pending;
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
								Changes.RemoveAll(t => t.RoundMax <= p.Id);

								//foreach(var i in Operations.Where(o => o.Stage == ProcessingStage.Placed && p.AnyOperation(i => i.Transaction.SignatureEquals(o.Transaction))).ToArray())
								//{
								//	i.Stage = ProcessingStage.Confirmed;
								//	i.FlowReport?.StageChanged();
								//	Operations.Remove(i);
								//}

								//foreach(var i in Messages.Where(o => o.Stage == ProcessingStage.Placed && )
								//{
								//	i.Stage = ProcessingStage.Confirmed;
								//	//i.FlowReport?.StageChanged();
								//	Messages.Remove(i);
								//}
							
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
				o.Stage = ProcessingStage.Pending;
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
						foreach(var t in Changes.OfType<Transaction>().Where(i => i.Stage == ProcessingStage.Accepted).ToArray())
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
											Changes.Remove(t);
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
							}

							if(valid)
								t.Stage = ProcessingStage.Pending;
						}
					}
				}
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		public List<Change> ProcessIncoming(IEnumerable<Change> txs)
		{
			Statistics.TransactionsProcessing.Begin();

			if(Generator == null) /// not ready to process external transactions
				return new();

			var accepted = txs.Where(i =>	!Changes.Any(j => i.SignatureEquals(j)) && 
											i.RoundMax > Chain.LastConfirmedRound.Id && 
											i.Valid).ToList();
								
			foreach(var i in accepted)
				i.Stage = ProcessingStage.Accepted;

			Changes.AddRange(accepted);

			Statistics.TransactionsProcessing.End();

			return accepted;
		}
		
// 		public List<Message> ProcessIncoming(IEnumerable<Message> messages)
// 		{
// 			Statistics.TransactionsProcessing.Begin();
// 
// 			if(Generator == null) /// not ready to process external transactions
// 				return new();
// 
// 			var accepted = messages.Where(i =>	!Transactions.OfType<Message>().Any(j => i.SignatureEquals(j)) && 
// 												i.RoundMax > Chain.LastConfirmedRound.Id && 
// 												i.Valid).ToList();
// 								
// 			foreach(var i in accepted)
// 				i.Stage = ProcessingStage.Accepted;
// 
// 			Transactions.AddRange(accepted);
// 
// 			Statistics.TransactionsProcessing.End();
// 
// 			return accepted;
// 		}

		C[] Read<C>(Stream data, Func<BinaryReader, byte, C> construct) where C : IBinarySerializable
		{
			if(data != null)
			{
				var r = new BinaryReader(data);
	
				var n = r.Read7BitEncodedInt();
	
				var many = new C[n];
	
				for(int i=0; i<n; i++)
				{
					var o = construct(r, r.ReadByte());
					o.Read(r);
					many[i] = o;
				}
	
				return many;
			} 
			else
			{
				return new C[0];
			}
		}

		public T[] Read<T>(Stream data, Func<BinaryReader, T> construct) where T : IBinarySerializable
		{
			if(data != null)
			{
				var r = new BinaryReader(data);
	
				var n = r.Read7BitEncodedInt();
	
				var many = new T[n];
	
				for(int i=0; i<n; i++)
				{
					var o = construct(r);
					o.Read(r);
					many[i] = o;
				}
	
				return many;
			} 
			else
			{
				return new T[0];
			}
		}

		IEnumerable<O> ReadMany<O>(Stream data, Func<BinaryReader, O> read)
		{
			if(data != null)
			{
				var r = new BinaryReader(data);
	
				var n = r.Read7BitEncodedInt();
	
				for(int i=0; i<n; i++)
				{
					yield return read(r);
					
				}
			} 
		}

		MemoryStream Write<T>(IEnumerable<T> many) where T : IBinarySerializable
		{
			if(many.Count() > 0)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				w.Write7BitEncodedInt(many.Count());
	
				foreach(var i in many)
				{
					if(i is ITypedBinarySerializable t)
						w.Write(t.BinaryType);

					i.Write(w);
				}
	
				return s;
			}
			else
				return null;
		}

		MemoryStream Write<T>(IEnumerable<T> many, Action<BinaryWriter, T> write)
		{
			if(many.Count() > 0)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				w.Write7BitEncodedInt(many.Count());
	
				foreach(var i in many)
				{
					write(w, i);
				}
	
				return s;
			}
			else
				return null;
		}

		void Broadcast<T>(PacketType type, T o, Peer skip = null) where T : IBinarySerializable
		{
			Broadcast(type, Write(new T[] {o}), skip);
		}

		void Broadcast<T>(PacketType type, IEnumerable<T> o, Peer skip = null) where T : IBinarySerializable
		{
			Broadcast(type, Write(o), skip);
		}

		void Broadcast(PacketType type, MemoryStream data, Peer skip = null)
		{
			if(data != null)
				foreach(var i in Connections.Where(j => j != skip))
				{
					i.Send(Header, type, data);
				}
		}

		public Coin EstimateFee(Transaction t)
		{
			return Chain != null ? Chain.CalculateFee(Chain.LastConfirmedRound, t) : Coin.Zero;
		}

		public async Task<Emission> Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, PrivateAccount signer, IFlowControl flowcontrol = null, CancellationTokenSource cts = null)
		{
			Emission l;

			if(Chain != null)
				lock(Lock)
					l = Chain.Accounts.FindLastOperation<Emission>(signer);
			else
				l = await Task.Run<Emission>(() => GetRemoteMember().Api.Send(new LastOperationCall {Account = signer, Type = typeof(Emission).Name}) as Emission);

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

		public AccountInfo GetAccountInfo(Account account, bool confirmed, IFlowControl flowcontrol = null)
		{
			if(Chain != null)
			{
				lock(Lock)
					return Chain.GetAccountInfo(account, confirmed);
			}
			else
			{
				return GetRemoteMember().Api.Send(new AccountInfoCall {Account = account, Confirmed = confirmed}) as AccountInfo;
			}
		}

		public AuthorInfo GetAuthorInfo(string author, bool confirmed, IFlowControl flowcontrol = null)
		{
			if(Chain != null)
			{
				lock(Lock)
					return Chain.GetAuthorInfo(author, confirmed);
			}
			else
			{
				return GetRemoteMember().Api.Send(new AuthorInfoCall {Name = author, Confirmed = confirmed}) as AuthorInfo;
			}
		}
		
		public ReleaseAddress QueryRelease(ReleaseQuery query, bool confirmed)
		{
			if(Chain != null)
			{
				lock(Lock)
					return Chain.QueryRelease(query, confirmed);
			}
			else
			{
				return GetRemoteMember().Api.Send(new QueryReleaseCall { Query = query, Confirmed = confirmed}) as ReleaseAddress;
			}
		}
				
		public byte[] DownloadRelease(ReleaseDownloadRequest request)
		{
			throw new NotImplementedException();
		}
	}
}