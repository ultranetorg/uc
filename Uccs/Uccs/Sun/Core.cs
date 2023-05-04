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
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;
using static Uccs.Net.Download;

namespace Uccs.Net
{
	public delegate void VoidDelegate();
 	public delegate void CoreDelegate(Core d);

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
		Chain	= 0b00000001,
		Base	= 0b00000010,
		Seed	= 0b00000100,
		Hub		= 0b00001000,
	}

	public class ReleaseStatus
	{
		public bool				ExistsRecursively { get; set; }
		public Manifest			Manifest { get; set; }
		public DownloadReport	Download { get; set; }
	}

	public class Core : RdcInterface
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
		public Database					Database;
		public Filebase					Filebase;
		public Seedbase					Seedbase;
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

		public List<Transaction>		Transactions = new();
		public List<Operation>			Operations	= new();

		bool							MinimalPeersReached;
		public List<Peer>				Peers		= new();
		public IEnumerable<Peer>		Connections	=> Peers.Where(i => i.Established);
		public Peer[]					BaseConnections = new Peer[0];
		public List<IPAddress>			IgnoredIPs	= new();
		public List<Member>				Members		= new();
		public List<Download>			Downloads	= new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		Thread							TransactingThread;
		Thread							VerifingThread;
		Thread							SynchronizingThread;
		Thread							DeclaringThread;

		//public bool						Running { get; protected set; } = true;
		public Synchronization			_Synchronization = Synchronization.Null;
		public Synchronization			Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public CoreDelegate				SynchronizationChanged;
		public Dictionary<int, List<BlockPiece>> SyncBlockCache	= new();

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
				f.Add(new ("Operations",				$"{Operations.Count}"));
				f.Add(new ("    Pending Delegation",	$"{Operations.Count(i => i.Placing == PlacingStage.PendingDelegation)}"));
				f.Add(new ("    Accepted",				$"{Operations.Count(i => i.Placing == PlacingStage.Accepted)}"));
				f.Add(new ("    Pending Placement",		$"{Operations.Count(i => i.Placing == PlacingStage.Verified)}"));
				f.Add(new ("    Placed",				$"{Operations.Count(i => i.Placing == PlacingStage.Placed)}"));
				f.Add(new ("    Confirmed",				$"{Operations.Count(i => i.Placing == PlacingStage.Confirmed)}"));
				f.Add(new ("Peers in/out/min/known",	$"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}"));
				
				if(Database != null)
				{
					f.Add(new ("Synchronization",		$"{Synchronization}"));
					f.Add(new ("Size",					$"{Database.Size}"));
					f.Add(new ("Members",				$"{Database.LastConfirmedRound?.Members.Count}"));
					f.Add(new ("Emission",				$"{(Database.LastPayloadRound != null ? Database.LastPayloadRound.Emission.ToHumanString() : null)}"));
					f.Add(new ("Cached Block Pieces",	$"{SyncBlockCache.Sum(i => i.Value.Count)}"));
					f.Add(new ("Cached Rounds",			$"{Database.LoadedRounds.Count()}"));
					f.Add(new ("Last Non-Empty Round",	$"{(Database.LastNonEmptyRound != null ? Database.LastNonEmptyRound.Id : null)}"));
					f.Add(new ("Last Payload Round",	$"{(Database.LastPayloadRound != null ? Database.LastPayloadRound.Id : null)}"));
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
							return Database.Accounts.Find(a, Database.LastConfirmedRound.Id)?.Balance.ToHumanString();
						}
	
						foreach(var i in Vault.Accounts)
						{
							f.Add(new ($"Account", $"{i.ToString().Insert(6, "-")} {formatbalance(i), BalanceWidth}"));
						}
	
						if(Settings.Dev.UI)
						{
							foreach(var i in Database.LastConfirmedRound.Funds)
							{
								f.Add(new ($"Fundable", $"{i.ToString().Insert(6, "-")} {formatbalance(i), BalanceWidth}"));
							}
						}
					}
				}
				else
				{
					f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in Vault.Accounts)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				Statistics.Reset();
		
				return f;
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


		public Core(Zone zone, Settings settings, Log log)
		{
			Zone = zone;
			Settings = settings;
			//Cryptography.Current = settings.Cryptography;

			Workflow = new Workflow(log);

			Directory.CreateDirectory(Settings.Profile);

			Workflow?.Log?.Report(this, $"Ultranet Node/Client {Version}");
			Workflow?.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow?.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow?.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow?.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(Settings.Dev != null && Settings.Dev.Any)
				Workflow?.Log?.ReportWarning(this, $"Dev: {Settings.Dev}");

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
																new (ProductTable.MetaColumnName,	new ()),
																new (ProductTable.MainColumnName,	new ()),
																new (ProductTable.MoreColumnName,	new ()),
																new (PlatformTable.MetaColumnName,	new ()),
																new (PlatformTable.MainColumnName,	new ()),
																new (PlatformTable.MoreColumnName,	new ()),
																new (ReleaseTable.MetaColumnName,	new ()),
																new (ReleaseTable.MainColumnName,	new ()),
																new (ReleaseTable.MoreColumnName,	new ()),
																new (Database.ChainFamilyName,		new ()),
															})
				cfamilies.Add(i);

			DatabaseEngine = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
		}

		public override string ToString()
		{
			var gens = Database?.LastConfirmedRound != null ? Settings.Generators.Where(i => Database.LastConfirmedRound.Members.Any(j => j.Generator == i)) : new AccountKey[0];
	
			return	$"{(Settings.Roles.HasFlag(Role.Base) ? "B" : "")}{(Settings.Roles.HasFlag(Role.Chain) ? "C" : "")}{(Settings.Roles.HasFlag(Role.Hub) ? "H" : "")}{(Settings.Roles.HasFlag(Role.Seed) ? "S" : "")}" +
					$"{(Connections.Count() < Settings.PeersMin ? " - Low Peers" : "")}" +
					$"{(!IP.Equals(IPAddress.None) ? " - " + IP : "")}" +
					$" - {Synchronization}" +
					(Database?.LastConfirmedRound != null ? $" - {gens.Count()}/{Database.LastConfirmedRound.Members.Count()} members" : "");
		}

		public object Constract(Type t)
		{
			if(t == typeof(Transaction)) return new Transaction(Zone);
			if(t == typeof(BlockPiece)) return new BlockPiece(Zone);
			if(t == typeof(Manifest)) return new Manifest(Zone);

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

			if(Settings.Roles.HasFlag(Role.Seed))
			{
				Filebase = new Filebase(Zone, System.IO.Path.Join(Settings.Profile, typeof(Filebase).Name), Settings.ProductsPath);
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

			if(Settings.Roles.HasFlag(Role.Hub))
			{
				Seedbase = new Seedbase(this);
			}

			if(Settings.Roles.HasFlag(Role.Seed))
			{
				Filebase = new Filebase(Zone, System.IO.Path.Join(Settings.Profile, typeof(Filebase).Name), System.IO.Path.Join(Settings.Profile, "Products"));
			}

			if(Settings.Roles.HasFlag(Role.Base) || Settings.Roles.HasFlag(Role.Chain))
			{
				Database = new Database(Zone, Settings.Roles, Settings.Database, Settings.Dev, Workflow?.Log, DatabaseEngine);
		
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
			MainThread.Start();

			MainStarted?.Invoke(this);
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
				RememberPeers(Zone.Initials.Select(i => new Peer(i){LastSeen = DateTime.UtcNow}));
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

			foreach(var i in Peers.Where(i => i.Tcp != null && i.Status == ConnectionStatus.Failed))
				i.Disconnect();

			if(!MinimalPeersReached && 
				Connections.Count() >= Settings.PeersMin && 
				(Database == null || Connections.Count(i => i.Roles.HasFlag(Database.Roles)) >= Settings.Database.PeersMin))
			{
				//if(DelegatingThread == null)
				//{
				//	DelegatingThread = new Thread(() => { 
				//											try
				//											{
				//												Delegating();
				//											}
				//											catch(OperationCanceledException)
				//											{
				//											}
				//											catch(Exception ex) when (!Debugger.IsAttached)
				//											{
				//												Stop(MethodBase.GetCurrentMethod(), ex);
				//											}
				//										});
				//
				//	DelegatingThread.Name = $"{Settings.IP.GetAddressBytes()[3]} Delegating";
				//	DelegatingThread.Start();
				//}


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
				Workflow?.Log?.Report(this, "Listening starting", $"{Settings.IP}:{Zone.Port}");

				Listener = new TcpListener(Settings.IP, Zone.Port);
				Listener.Start();
	
				while(!Workflow.IsAborted)
				{
					var client = Listener.AcceptTcpClient();
	
					lock(Lock)
					{
						if(!Workflow.IsAborted && Connections.Count() < Settings.PeersInMax)
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

			h.Roles		= Settings.Roles;
			h.Versions	= Versions;
			h.Zone		= Zone.Name;
			h.IP		= ip;
			h.Nuid		= Nuid;
			h.Peers		= peers;
			//h.LastRound				= Header.LastNonEmptyRound;
			//h.LastConfirmedRound	= Header.LastConfirmedRound; 
			
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
						client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Timeout;
						//client.ReceiveTimeout = Timeout;
						client.Connect(peer.IP, Zone.Port);
					}
					catch(SocketException ex) 
					{
						//Workflow.Log?.Report(this, "Establishing failed", $"To {peer.IP}; Connect; {ex.Message}" );
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
						//Workflow.Log?.Report(this, "Establishing failed", $"To {peer.IP}; Send/Wait Hello; {ex.Message}" );
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
							//Workflow.Log?.Report(this, "Establishing failed", "It's me");
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
							//Workflow.Log?.Report(this, "Establishing failed", $"From {peer.IP}; Already established" );
							client.Close();
							return;
						}
	
						RememberPeers(h.Peers);
	
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
							//Workflow.Log?.Report(this, "Establishing failed", "It's me");
							Peers.Remove(peer);
							client.Close();
							return;
						}

						if(peer != null && peer.Established)
						{
							//Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; Already established" );
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
							//Workflow.Log?.Report(this, "Establishing failed", $"From {ip}; SendHello; {ex.Message}");
							goto failed;
						}
	
						if(peer == null)
						{
							peer = new Peer(ip);
							Peers.Add(peer);
						}
									
						RememberPeers(h.Peers.Append(peer));
	
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

					Database.LoadedRounds.Clear();

					var peer = Connect(Database.Roles.HasFlag(Role.Chain) ? Role.Chain : Role.Base, used, Workflow);

					if(Database.Roles.HasFlag(Role.Base) && !Database.Roles.HasFlag(Role.Chain))
					{
						Database.Tail.Clear();
		
						stamp = peer.GetStamp();
		
						void download<E, K>(Table<E, K> t) where E : ITableEntry<K>
						{
							var ts = peer.GetTableStamp(t.Type, (t.Type switch
																		{ 
																			Tables.Accounts	=> stamp.Accounts.Where(i => {
																															var c = Database.Accounts.SuperClusters.ContainsKey(i.Id);
																															return !c || !Database.Accounts.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														 }),
																			Tables.Authors	=> stamp.Authors.Where(i =>	{
																															var c = Database.Authors.SuperClusters.ContainsKey(i.Id);
																															return !c || !Database.Authors.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														}),
																			Tables.Products	=> stamp.Products.Where(i => {
																															var c = Database.Products.SuperClusters.ContainsKey(i.Id);
																															return !c || !Database.Products.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														 }),
																			Tables.Platforms=> stamp.Platforms.Where(i => {
																															var c = Database.Platforms.SuperClusters.ContainsKey(i.Id);
																															return !c || !Database.Platforms.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														  }),
																			Tables.Releases	=> stamp.Releases.Where(i => {
																															var c = Database.Releases.SuperClusters.ContainsKey(i.Id);
																															return !c || !Database.Releases.SuperClusters[i.Id].SequenceEqual(i.Hash);
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
									
										Database.Engine.Write(b);
									}
		
									Workflow.Log?.Report(this, "Cluster downloaded", $"{t.GetType().Name} {c.Id}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						download<AccountEntry,	AccountAddress>(Database.Accounts);
						download<AuthorEntry, string>(Database.Authors);
						download<ProductEntry, ProductAddress>(Database.Products);
						download<PlatformEntry, PlatformAddress>(Database.Platforms);
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
						r.Funds		= rd.ReadList<AccountAddress>();
		
						Database.BaseState	= stamp.BaseState;
						//Database.Members	= r.Members;
						//Database.Funds		= r.Funds;
		
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
						Thread.Sleep(1);
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
		
						var to = from + Database.Pitch;
		
						var rp = peer.Request<DownloadRoundsResponse>(new DownloadRoundsRequest{From = from, To = to});
						 	
						lock(Lock)
							if(from <= rp.LastNonEmptyRound)
							{
								foreach(var i in SyncBlockCache.Keys)
								{
									if(i < rp.LastConfirmedRound + 1 - Database.Pitch)
									{
										SyncBlockCache.Remove(i);
									}
								}

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
				
										Database.Tail.RemoveAll(i => i.Id == r.Id); /// remove old round with all its blocks
										Database.Tail.Add(r);
										Database.Tail = Database.Tail.OrderByDescending(i => i.Id).ToList();
		
										r.Confirmed = false;
										Database.Confirm(r, true);
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
			
										if(r.Confirmed)
										{
							 				throw new SynchronizationException();
										}
				
										if(r.BlockPieces.Any())
										{
											var rq = new BlocksPiecesRequest();
											rq.Pieces = r.BlockPieces;
											rq.Execute(this);
										}

										ProcessIncoming(r.Blocks);
									}
								}
										
								Workflow.Log?.Report(this, "Rounds received", $"{rounds.Min(i => i.Id)}..{rounds.Max(i => i.Id)}");
							}
							else if(Database.BaseHash.SequenceEqual(rp.BaseHash))
							{
								Synchronization = Synchronization.Synchronized;

								var rq = new BlocksPiecesRequest();
								rq.Pieces = SyncBlockCache.OrderBy(i => i.Key).SelectMany(i => i.Value).ToArray();
								rq.Execute(this);
									
								SyncBlockCache.Clear();
	
								SynchronizingThread = null;
	
								Workflow.Log?.Report(this, "Syncing finished");

								return;
							}
							else
								throw new SynchronizationException();
					}
				}
				catch(RdcException)
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

		public void ProcessIncoming(IEnumerable<Block> blocks)
		{
 			var verified = blocks.Where(b =>{
												//if(LastConfirmedRound != null && b.RoundId <= LastConfirmedRound.Id)
												//	return false;

												var r = Database.FindRound(b.RoundId);
	
												if(r != null && !r.Confirmed && r.Blocks.Any(i => i.Signature.SequenceEqual(b.Signature)))
													return false;

												return b.Valid && Zone.Cryptography.Valid(b.Signature, b.Hash, b.Generator);

											}).ToArray(); /// !ToArray cause will be added to Chain below

			if(Synchronization == Synchronization.Downloading || Synchronization == Synchronization.Synchronizing)
			{
				Database.Add(verified);
			}

			if(Synchronization == Synchronization.Synchronized)
			{
				//var joins = verified.OfType<JoinMembersRequest>().Where(b => { 
				//																for(int i = b.RoundId; i > b.RoundId - Database.Pitch * 2; i--) /// not more than 1 request per [2 x Pitch] rounds
				//																	if(Database.FindRound(i) is Round r && r.JoinRequests.Any(j => j.Generator == b.Generator))
				//																		return false;
				//
				//																return true;
				//															}).ToArray();
				//Database.Add(joins);
				//	
				//var votes = verified.OfType<Vote>().Where(i => i.RoundId > Database.LastConfirmedRound.Id && Database.VoterOf(i.RoundId).Any(j => j.Generator == i.Generator)).ToArray();
				
				Database.Add(verified);
			}
		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var blocks = new List<Block>();

			foreach(var g in Settings.Generators)
			{
				if(!Database.VoterOf(Database.LastConfirmedRound.Id + 1 + Database.Pitch).Any(i => i.Generator == g))
				{
					var jr = Database.FindLastBlock(i => i is JoinMembersRequest jr && jr.Generator == g, Database.LastConfirmedRound.Id - Database.Pitch) as JoinMembersRequest;

					//var jr = Database.JoinRequests.Where(i => i.Generator == g).MaxBy(i => i.RoundId);
		
					if(jr == null || jr.RoundId + Database.Pitch <= Database.LastConfirmedRound.Id)
					{
						var b = new JoinMembersRequest(Database)
								{	
									RoundId	= Database.LastConfirmedRound.Id + Database.Pitch,
									IPs     = new [] {IP}
								};
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

					// i.Operations.Any() required because in Database.Confirm operations and transactions may be deleted
					var txs = Database.CollectValidTransactions(Transactions.Where(i => r.Id <= i.RoundMax && i.Operations.Any() && i.Operations.All(i => i.Placing == PlacingStage.Verified))
																			.GroupBy(i => i.Signer)
																			.Select(i => i.First()), r).ToArray();

					var prev = r.Previous.Votes.FirstOrDefault(i => i.Generator == g);
					var p = r.Parent;
	
					if(txs.Any()) /// any pending foreign transactions or any our pending operations
					{
					
						var b = new Payload(Database)
								{
									RoundId		= r.Id,
									Try			= r.Try,
									Consensus	= Database.ProposeConsensus(p),
									Time		= Clock.Now,
									TimeDelta	= prev == null || prev.RoundId <= Database.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Time).TotalMilliseconds,
									Violators	= p.Forkers.ToList(),
									Joiners		= Database.ProposeJoiners(r).ToList(),
									Leavers		= Database.ProposeLeavers(r, g).ToList(),
									FundJoiners	= new(),
									FundLeavers	= new(),
								};
					
						var s = new MemoryStream(); 
						var w = new BinaryWriter(s);
						b.Sign(g);
						b.Write(w);

						foreach(var i in txs)
						{
							i.WriteAsPartOfBlock(w);

							if(s.Position > Block.SizeMax)
								break;

							b.AddNext(i);
	
							foreach(var o in i.Operations)
								o.Placing = PlacingStage.Placed;

							//Transactions.Remove(i); /// required because in Database.Confirm operations and transactions may be deleted
						}
							
						b.Sign(g);
						blocks.Add(b);
					}
					else if(Database.Tail.Any(i => Database.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
					{
						var b = new Vote(Database)
								{	
									RoundId		= r.Id,
									Try			= r.Try,
									Consensus	= Database.ProposeConsensus(p),
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

					while(Database.VoterOf(r.Previous.Id).Any(i => i.Generator == g) && !r.Previous.Votes.Any(i => i.Generator == g))
					{
						r = r.Previous;

						prev = r.Previous.Votes.FirstOrDefault(i => i.Generator == g);

						var b = new Vote(Database)
								{	
									RoundId		= r.Id,
									Try			= r.Try,
									Consensus	= Database.ProposeConsensus(p),
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
					Database.Add(b);
				}

				var pieces = new List<BlockPiece>();

				var npieces = 1;
				var ppp = 3; // peers per peice

				foreach(var b in blocks)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					//w.Write(b.TypeCode);
					b.Write(w);

					s.Position = 0;
					var r = new BinaryReader(s);

					//var guid = new byte[BlockPiece.GuidLength];
					//Cryptography.Random.NextBytes(guid);

					for(int i = 0; i < npieces; i++)
					{
						var p = new BlockPiece(Zone){	Type = b.Type,
														Try = b is Vote v ? v.Try : 0,
														RoundId = b.RoundId,
														Total = npieces,
														Index = i,
														Data = r.ReadBytes((int)s.Length/npieces + (i < npieces-1 ? 0 : (int)s.Length % npieces))};

						p.Sign(b.Generator as AccountKey);

						pieces.Add(p);
						Database.GetRound(b.RoundId).BlockPieces.Add(p);
					}
				}

				
				if(BaseConnections.Count(i => i.Established) < Settings.Database.PeersMin)
				{
					BaseConnections = Connect(Role.Base, Settings.Database.PeersMin, Workflow);
				}

				IEnumerator<Peer> start() => BaseConnections.AsEnumerable().GetEnumerator();

 				var c = start();
 
 				foreach(var i in pieces)
 				{
					i.Peers = new();   

					for(int j = 0; j < ppp; j++)
					{
 						if(!c.MoveNext())
 						{
 							c = start(); /// go to the beginning
 							c.MoveNext();
 						}

						i.Peers.Add(c.Current);
					}
 				}

				/// LESS RELIABLE
				foreach(var i in pieces.SelectMany(i => i.Peers).Distinct())
				{
					i.Request<object>(new BlocksPiecesRequest{Pieces = pieces.Where(x => x.Peers.Contains(i)).ToArray()});
				}

				/// ALL FOR ALL
				///foreach(var i in Connections.Where(i => i.BaseRank > 0))
				///{
				///	i.Request<object>(new UploadBlocksPiecesRequest{Pieces = pieces});
				///}
													
				 Workflow.Log?.Report(this, "Block(s) generated", string.Join(", ", blocks.Select(i => $"{i.Type}-{Hex.ToHexString(i.Generator.Prefix)}-{i.RoundId}")));
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
				if(Database.QuorumReached(r) && r.Parent != null)
				{
					r.Voted = true;

					/// Check our peices that are not come back from other peer, means first peer went offline, if any - force broadcast them
					var notcomebacks = r.Parent.BlockPieces.Where(i => i.Peers != null && !i.Broadcasted).ToArray();
					
					if(notcomebacks.Any())
					{
						foreach(var i in Connections.Where(i => i.BaseRank > 0).OrderBy(i => Guid.NewGuid()))
						{
							i.Request<object>(new BlocksPiecesRequest{Pieces = notcomebacks});
						}
					}

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
				else if(Database.QuorumFailed(r) || (!Settings.Dev.DisableTimeouts && DateTime.UtcNow - r.FirstArrivalTime > TimeSpan.FromMinutes(5)))
				{
					foreach(var i in Transactions.Where(i => i.Payload != null && i.Payload.RoundId >= r.Id).SelectMany(i => i.Operations))
					{
						i.Placing = PlacingStage.Verified;
					}

					r.FirstArrivalTime = DateTime.MaxValue;
					r.Try++;
				}
			}

			Statistics.Consensing.End();
		}

		void Transacting()
		{
			Operation[]					pendings;
			bool						ready;
			IEnumerable<Operation>		accepted;

			Workflow.Log?.Report(this, "Delegating started");

			MemberRdi m = null;

			while(!Workflow.IsAborted)
			{
				Thread.Sleep(1);

				lock(Lock)
				{
					if(!Operations.Any())
					{
						TransactingThread = null;
						return;
					}
				}
				
				if(m == null)
				{	
					m = ConnectToMember(Workflow);
				}

				try
				{
					Statistics.Transacting.Begin();

					lock(Lock)
					{
						if(m.Rdi == this && Synchronization != Synchronization.Synchronized)
							continue;

						pendings = Operations.Where(i => i.Placing == PlacingStage.PendingDelegation).ToArray();
						ready = pendings.Any() && !Operations.Any(i => i.Placing >= PlacingStage.Accepted);
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
										Vault.OperationIds[g.Key] = m.GetAccountInfo(g.Key, false).Account.LastOperationId;
									}
									catch(RdcException ex) when(ex.Error == RdcError.AccountNotFound)
									{
										Vault.OperationIds[g.Key] = -1;
									}
									catch(Exception) when(!Debugger.IsAttached)
									{
									}
									
									Monitor.Enter(Lock);
								}

								var t = new Transaction(Zone);

								foreach(var o in g)
								{
									o.Id = ++Vault.OperationIds[g.Key];
									t.AddOperation(o);
								}

								t.Sign(Vault.GetKey(g.Key), m.Generator, Database.GetValidityPeriod(rmax));
								txs.Add(t);
							}
						}
	
						var atxs = m.SendTransactions(txs).Accepted.Select(i => txs.Find(t => t.Signature.SequenceEqual(i)));
	
						lock(Lock)
							foreach(var o in atxs.SelectMany(i => i.Operations))
							{
								o.Placing = PlacingStage.Accepted;
							}
						
						if(atxs.Any())
						{
							if(atxs.Sum(i => i.Operations.Count) <= 1)
							{
								var msgs = new List<string>{};
		
								foreach(var i in atxs.SelectMany(i => i.Operations))
								{
									msgs.Add(i.ToString());
								}
		
								Workflow.Log?.Report(this, "Operations sent", msgs);
							} 
							else
							{
								Workflow.Log?.Report(this, "Operation sent", $"{atxs.First().Operations.First()} -> {m.Generator} {(m.Rdi is Peer p ? p.IP : "Self")}");
							}
						}

					}

					lock(Lock)
						accepted = Operations.Where(i => i.Placing >= PlacingStage.Accepted).ToArray();
	
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
									o.Placing = i.Placing;

									if(i.Placing == PlacingStage.Confirmed || i.Placing == PlacingStage.FailedOrNotFound)
									{
										Operations.Remove(o);
									}
								}
							}
						}
					}

					Statistics.Transacting.End();
				}
				catch(Exception ex) when (ex is ConnectionFailedException || ex is RdcException)
				{
					Workflow.Log?.ReportWarning(this, "Delegation", $"Member={m}", ex);

					if(m.Failures < 3)
						m.Failures++;
					else
						m = null;

					Thread.Sleep(500); /// prevent any flooding
				}
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

			while(true)
			{ 
				Thread.Sleep(1);
				workflow.ThrowIfAborted();

				switch(waitstage)
				{
					case PlacingStage.Null :				return;
					case PlacingStage.Accepted :			if(operations.All(o => o.Placing >= PlacingStage.Accepted))			return; else break;
					case PlacingStage.Placed :				if(operations.All(o => o.Placing >= PlacingStage.Placed))			return; else break;
					case PlacingStage.Confirmed :			if(operations.All(o => o.Placing == PlacingStage.Confirmed))		return; else break;
					case PlacingStage.FailedOrNotFound :	if(operations.All(o => o.Placing == PlacingStage.FailedOrNotFound)) return; else break;
				}
			}
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

		void Declaring()
		{
			Workflow.Log?.Report(this, "Declaring started");

			try
			{
				while(!Workflow.IsAborted)
				{
					Workflow.Wait(100);
					Statistics.Declaring.Begin();

					FilebaseRelease[] rs;
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

				Statistics.Declaring.End();

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
			if(!Settings.Generators.Any(g => Database.LastConfirmedRound.Members.Any(m => g == m.Generator))) /// not ready to process external transactions
				return new();

			var accepted = txs.Where(i =>	!Transactions.Any(j => i.EqualBySignature(j)) &&
											i.RoundMax > Database.LastConfirmedRound.Id &&
											Settings.Generators.Any(g => g == i.Generator) &&
											i.Valid).ToList();
								
			foreach(var i in accepted)
				foreach(var o in i.Operations)
					o.Placing = PlacingStage.Accepted;

			Transactions.AddRange(accepted);

			return accepted;
		}

		public MemberRdi ConnectToMember(Workflow workflow)
		{
			if(Settings.Generators.Any())
			{
				return new MemberRdi(Settings.Generators.First(), this);
			}

			Peer peer;
				
			while(true)
			{
				Thread.Sleep(100);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					peer = Peers.OrderByDescending(i => i.Established).ThenBy(i => i.ReachFailures).FirstOrDefault();

					if(peer == null)
					{
						Thread.Sleep(1000);
						continue;
					}
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
								var c = Connections.FirstOrDefault(j => i.IPs.Any(ip => j.IP.Equals(ip)));

								if(c != null)
								{
									Workflow.Log?.Report(this, "Member chosen", $"{i} {c}");
									return new MemberRdi(i.Generator, c);
								}
							}

							var m = Members.Random();
							var ip = m.IPs.Random();
							var p = GetPeer(ip);

							Connect(p, workflow);
		
							Workflow.Log?.Report(this, "Member chosen", $"{m} {p}");

							return new MemberRdi(m.Generator, p);
						}
					}
				}
				catch(Exception ex) when (ex is ConnectionFailedException || ex is AggregateException || ex is RdcException)
				{
					peer.ReachFailures++;
				}
			}
	
			throw new ConnectionFailedException("Aborted, timeout or overall abort");
		}

		public Peer FindBestPeer(Role role, HashSet<Peer> exclusions)
		{
			if(BitOperations.PopCount((uint)role) > 1)
				throw new ArgumentException();

			return Peers.Where(i => i.GetRank(role) > 0 && (exclusions == null || !exclusions.Contains(i))).OrderByDescending(i => i.Established)
																											.ThenBy(i => i.GetRank(role))
																											.ThenByDescending(i => i.ReachFailures)
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
					peer = FindBestPeer(role, exclusions);
	
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
				
			while(!workflow.IsAborted)
			{
				Thread.Sleep(1);
				workflow.ThrowIfAborted();
	
				lock(Lock)
				{
					var p = FindBestPeer(role, peers);
	
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
// 				catch(RdcException)
// 				{
// 				}
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
				catch(RdcException)
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
				catch(RdcException)
				{
				}
			}

			throw new RdcException(RdcError.AllNodesFailed);
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
				Thread.Sleep(1);
				workflow.ThrowIfAborted();

				lock(Lock)
					if(peer.Established)
						return;
					else if(peer.OutStatus == EstablishingStatus.Failed)
						throw new ConnectionFailedException("Failed");

				if(!Settings.Dev.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new ConnectionFailedException("Timed out");
			}
		}


		public Coin EstimateFee(IEnumerable<Operation> operations)
		{
			return Database != null ? Operation.CalculateFee(Database.LastConfirmedRound.Factor, operations) : Coin.Zero;
		}

		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, AccountKey signer, PlacingStage awaitstage, Workflow workflow)
		{
			var	l = Call(Role.Base, p =>{
											try
											{
												return p.GetAccountInfo(signer, true);
											}
											catch(RdcException ex) when (ex.Error == RdcError.AccountNotFound)
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
													return p.GetAccountInfo(signer, true);
												}
												catch(RdcException ex) when (ex.Error == RdcError.AccountNotFound)
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

		public Download DownloadRelease(ReleaseAddress release, Workflow workflow)
		{
			lock(Lock)
			{
				if(Filebase.FindRelease(release) != null)
					return null;

				var d = Downloads.Find(j => j.Release == release);
				
				if(d == null)
				{
					d = new Download(this, release, workflow);
					Downloads.Add(d);
				}

				return d;
			}
		}

		public void GetRelease(ReleaseAddress release, Workflow workflow)
		{
			Task.Run(() =>	{ 
								try
								{
									lock(Lock)
									{
										if(!Filebase.ExistsRecursively(release))
										{
											var d = DownloadRelease(release, workflow);
		
											while(!workflow.IsAborted && Downloads.Contains(d))
											{
												Monitor.Exit(Lock);
												{
													Thread.Sleep(100);
												}
												Monitor.Enter(Lock);
											}
										}
					
										Filebase.Unpack(release);
									}
								}
								catch(OperationCanceledException)
								{
								}
							});
		}

		public ReleaseStatus GetReleaseStatus(ReleaseAddress release, int limit)
		{
			var s = new ReleaseStatus();

			lock(Lock)
				s.ExistsRecursively = Filebase.ExistsRecursively(release);

			if(s.ExistsRecursively)
			{
				lock(Lock)
					s.Manifest = Filebase.FindRelease(release).Manifest;
			}
			else
			{
				Download d ;
				
				lock(Lock)
					d = Downloads.Find(i => i.Release == release);

				if(d != null)
				{
					lock(d.Lock)
					{
						s.Download = new () {	Distributive			= d.Distributive,
												Length					= d.Length, 
												CompletedLength			= d.CompletedLength,
												DependenciesRecursive	= d.DependenciesRecursive.Select(i => new DownloadReport.Dependency {Release = i.Release, Exists = Filebase.FindRelease(i.Release) != null}).ToArray(),
												Hubs					= d.Hubs.Take(limit).Select(i => new DownloadReport.Hub { IP = i.Peer.IP, Seeds = i.Seeds.Take(limit), Status = i.Status }).ToArray(),
												Seeds					= d.Seeds.Take(limit).Select(i => new DownloadReport.Seed { IP = i.Key, Status = i.Value }).ToArray() };
					}
				}
			}

			return s;
		}

		public void PackRelease(ReleaseAddress release, string channel, IEnumerable<string> sources, string dependsdirectory, bool confirmed, Workflow workflow)
		{
			var qlatest = Call(Role.Base, p => p.QueryRelease(release.Realization, release.Version, VersionQuery.Latest, channel, confirmed), workflow);
			var previos = qlatest.Releases.FirstOrDefault()?.Address.Version;

			Filebase.AddRelease(release, previos, sources, dependsdirectory, workflow);
		}

		public override Rp Request<Rp>(RdcRequest rq) where Rp : class
  		{
			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;
				rq = BinarySerializator.Deserialize(new(s), rq.GetType(), Constract) as RdcRequest;
			}

 			return rq.Execute(this) as Rp;
 		}
	}
}
