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
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
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

		public int AccpetedVotes;
		public int RejectedVotes;

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
		None, Downloading, Synchronized
	}

	[Flags]
	public enum Role : uint
	{
		None,
		Base		= 0b00000001,
		Chain		= 0b00000011,
		//Analyzer	= 0b00000101,
		Seed		= 0b00000100,
	}

	public class Sun : RdcInterface
	{
		public Role						Roles => (Mcv != null ? Mcv.Roles : Role.None)|(ResourceHub != null ? Role.Seed : Role.None);

		public System.Version			Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]	Versions = {1};
		public const string				FailureExt = "failure";
		public const int				Timeout = 5000;
		public const int				OperationsQueueLimit = 1000;
		const int						BalanceWidth = 24;

		public Zone						Zone;
		public Settings					Settings;
		public Workflow					Workflow;
		public JsonApiServer			ApiServer;
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
		public bool						IsMember => Synchronization == Synchronization.Synchronized && Settings.Generators.Any(g => Mcv.LastConfirmedRound.Members.Any(i => i.Account == g));

		public Statistics				PrevStatistics = new();
		public Statistics				Statistics = new();

		public List<Transaction>		IncomingTransactions = new();
		internal List<Transaction>		OutgoingTransactions = new();
		//public List<Analysis>			Analyses = new();
		public List<OperationId>		ApprovedEmissions = new();
		public List<OperationId>		ApprovedDomainBids = new();

		public bool						MinimalPeersReached;
		public List<Peer>				Peers = new();
		public IEnumerable<Peer>		Connections	=> Peers.Where(i => i.Status == ConnectionStatus.OK);
		public IEnumerable<Peer>		Bases => Connections.Where(i => i.Permanent && i.BaseRank > 0);

		public List<IPAddress>			IgnoredIPs	= new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		Thread							TransactingThread;
		Thread							SynchronizingThread;
		AutoResetEvent					MainSignal = new AutoResetEvent(true);

		public Synchronization			_Synchronization = Synchronization.None;
		public Synchronization			Synchronization { protected set { _Synchronization = value; SynchronizationChanged?.Invoke(this); } get { return _Synchronization; } }
		public SunDelegate				SynchronizationChanged;
		
		Dictionary<AccountAddress, Transaction>		LastCandidacyDeclaration = new();

		public SunDelegate				Stopped;


		public class SyncRound
		{
			public List<Vote>					Votes = new();
			//public List<AnalyzerVoxRequest>		AnalyzerVoxes = new();
		}
		
		public Dictionary<int, SyncRound>	SyncTail	= new();

		public IGasAsker				GasAsker; 
		public IFeeAsker				FeeAsker;

		public SunDelegate				MainStarted;
		public SunDelegate				ApiStarted;

		public class Tag
		{
			public const string Peering = "Peering";
			public const string Error = "Error";
			public const string Establishing = "Establishing";
			public const string Synchronization = "Synchronization";
		}
		
		public Sun(Zone zone, Settings settings)
		{
			Zone = zone;
			Settings = settings;

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
																new (AnalysisTable.MetaColumnName,	new ()),
																new (AnalysisTable.MainColumnName,	new ()),
																new (AnalysisTable.MoreColumnName,	new ()),
																new (Mcv.ChainFamilyName,			new ()),
																new (ResourceHub.FamilyName,		new ()) })
				cfamilies.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Database"), cfamilies);
		}

		public override string ToString()
		{
			var gens = Mcv?.LastConfirmedRound != null ? Settings.Generators.Where(i => Mcv.LastConfirmedRound.Members.Any(j => j.Account == i)) : new AccountKey[0];
	
			return string.Join(" - ", new string[]{	$"{(Roles.HasFlag(Role.Base) ? "B" : null)}" +
													$"{(Roles.HasFlag(Role.Chain) ? "C" : null)}" +
													$"{(Roles.HasFlag(Role.Seed) ? "S" : null)}",
													Connections.Count() < Settings.PeersPermanentMin ? "Low Peers" : null,
													(Settings.IP != null ? $"{IP}" : null),
													Mcv != null ? $"{Synchronization} - {Mcv.LastConfirmedRound?.Id} - {Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"T-{OutgoingTransactions.Count}/{IncomingTransactions.Count}",
													}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
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

		public void RunApi(Workflow workflow)
		{
			if(!HttpListener.IsSupported)
			{
				Environment.ExitCode = -1;
				throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
			}

			lock(Lock)
			{
				workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "JsonServer.log"), FileMode.Create);

				ApiServer = new JsonApiServer(Settings.IP, Settings.JsonServerPort, Settings.Api.AccessKey, n => Type.GetType(GetType().Namespace + '.' + n), (o, w) => (o as SunApiCall).Execute(this, w), workflow);
			}
		
			ApiStarted?.Invoke(this);
		}

		public void Run(Xon xon, Workflow workflow)
		{
			if(xon.Has("api"))
				RunApi(workflow);
			
			/// workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Node.log"), FileMode.Create)

			if(xon.Has("node"))
				RunNode(workflow, (xon.Has("base") ? Role.Base : Role.None) | (xon.Has("chain") ? Role.Chain : Role.None));

			if(xon.Has("seed"))
				RunSeed();
		}

		public void RunNode(Workflow workflow, Role roles)
		{
			Workflow = workflow;

			Workflow.Log?.Report(this, $"Ultranet Node {Version}");
			Workflow.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(DevSettings.Any)
				Workflow.Log?.ReportWarning(this, $"Dev: {DevSettings.AsString}");

			Nuid = Guid.NewGuid();

			if(Settings.Generators.Any())
			{
				SeedHub = new SeedHub(this);
			}

			if(roles.HasFlag(Role.Base) || roles.HasFlag(Role.Chain))
			{
				Mcv = new Mcv(Zone, roles, Settings.Mcv, Database);

				Mcv.Log = Workflow.Log;
				Mcv.VoteAdded += b => MainSignal.Set();

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
																	foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId == r.Id))
																	{
																		i.Vote = null;
																		i.Placing = PlacingStage.Accepted;
																	}
																}
															};
				
				Mcv.Commited += r => {
										if(IsMember)
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
	 		
	 																			if(txt != null && txt.Text.Any(i => AccountAddress.Parse(i) == o.Transaction.Signer))
	 																			{
																					lock(Lock)
																					{	
																						ApprovedDomainBids.Add(ab.Id);
																					}
	 																			}
	 																		}
	 																		catch(AggregateException ex)
	 																		{
	 																			Workflow.Log?.ReportError(this, "Can't verify AuthorBid domain", ex);
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
										}

										ApprovedEmissions.RemoveAll(i => r.ConfirmedEmissions.Contains(i) || r.Id > i.Ri + Zone.ExternalVerificationDurationLimit);
										ApprovedDomainBids.RemoveAll(i => r.ConfirmedDomainBids.Contains(i) || r.Id > i.Ri + Zone.ExternalVerificationDurationLimit);
										IncomingTransactions.RemoveAll(t => t.Vote?.Round != null && t.Vote.Round.Id <= r.Id || t.Expiration <= r.Id);
										//Analyses.RemoveAll(i => r.ConfirmedAnalyses.Any(j => j.Resource == i.Release && j.Finished));
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
				}

				Workflow.Log?.Report(this, "MCV started");
			}

			LoadPeers();

			if(Settings.IP != null)
			{
				ListeningThread = new Thread(Listening);
				ListeningThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Listening";
				ListeningThread.Start();
			}

 			MainThread = new Thread(() =>	{ 
												try
												{
													while(Workflow.Active)
													{
														var r = WaitHandle.WaitAny(new[] {MainSignal, Workflow.Cancellation.WaitHandle}, 500);

														lock(Lock)
														{
															ProcessConnectivity();
												
															if(Settings.Generators.Any() && Synchronization == Synchronization.Synchronized)
															{
																Generate();
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
			MainThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Main";
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
			Workflow?.Abort();

			lock(Lock)
				foreach(var i in Peers.Where(i => i.Status != ConnectionStatus.Disconnected).ToArray())
					i.Disconnect();

			ApiServer?.Stop();
			
			Listener?.Stop();

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
			var needed = Settings.PeersPermanentMin - Peers.Count(i => i.Permanent && i.Status != ConnectionStatus.Disconnected);
		
			foreach(var p in Peers	.Where(m =>	m.Status == ConnectionStatus.Disconnected &&
												DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
									//.OrderByDescending(i => i.PeerRank)
									.OrderBy(i => i.Retries)
									.ThenBy(i => Settings.PeersInitialRandomization ? Guid.NewGuid() : Guid.Empty)
									.Take(needed))
			{
				OutboundConnect(p, true);
			}

			foreach(var p in Peers.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
			{
				OutboundConnect(p, false);
			}

			if(Roles.HasFlag(Role.Base))
			{
				needed = Settings.Mcv.PeersMin - Bases.Count();

				foreach(var p in Peers	.Where(m =>	m.BaseRank > 0 &&
													m.Status == ConnectionStatus.Disconnected &&
													DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
										//.OrderByDescending(i => i.PeerRank)
										.OrderBy(i => i.Retries)
										.ThenBy(i => Settings.PeersInitialRandomization ? Guid.NewGuid() : Guid.Empty)
										.Take(needed))
				{
					OutboundConnect(p, true);
				}
			}

			//foreach(var i in Peers.Where(i => i.Status == ConnectionStatus.Failed))
			//	i.Disconnect();

			if(!MinimalPeersReached && 
				Connections.Count(i => i.Permanent) >= Settings.PeersPermanentMin && 
				(!Roles.HasFlag(Role.Base) || Bases.Count() >= Settings.Mcv.PeersMin))
			{
				Workflow?.Log?.Report(this, $"{Tag.Peering}", "Minimal peers reached");

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
				Workflow?.Log?.Report(this, $"{Tag.Peering}", $"Listening starting {Settings.IP}:{Zone.Port}");

				Listener = new TcpListener(Settings.IP, Zone.Port);
				Listener.Start();
	
				while(Workflow.Active)
				{
					var c = Listener.AcceptTcpClient();

					if(Workflow.Aborted)
					{
						c.Close();
						return;
					}
	
					lock(Lock)
					{
						if(Connections.Count(i => i.Inbound) <= Settings.PeersInboundMax)
							InboundConnect(c);
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
			catch(OperationCanceledException)
			{
			}
			catch(Exception ex) when (!Debugger.IsAttached)
			{
				Stop(MethodBase.GetCurrentMethod(), ex);
			}
		}

		Hello CreateHello(IPAddress ip, bool permanent)
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
			h.Permanent		= permanent;
			//h.Generators	= Members;
			
			return h;
		}

		void OutboundConnect(Peer peer, bool permanent)
		{
			peer.Status = ConnectionStatus.Initiated;
			peer.Permanent = permanent;
			peer.LastTry = DateTime.UtcNow;
			peer.Retries++;

			void f()
			{
				try
				{
					var client = Settings.IP != null ? new TcpClient(new IPEndPoint(Settings.IP, 0)) : new TcpClient();

					try
					{
						client.SendTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;
						//client.ReceiveTimeout = Timeout;
						client.Connect(peer.IP, Zone.Port);
					}
					catch(SocketException ex) 
					{
						Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"To {peer.IP}. {ex.Message}" );
						goto failed;
					}
	
					Hello h = null;
									
					try
					{
						client.SendTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;
						client.ReceiveTimeout = DevSettings.DisableTimeouts ? 0 : Timeout;

						Peer.SendHello(client, CreateHello(peer.IP, permanent));
						h = Peer.WaitHello(client);
					}
					catch(Exception ex)// when(!Settings.Dev.ThrowOnCorrupted)
					{
						Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"To {peer.IP}. {ex.Message}" );
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
							goto failed;
						}

						if(h.Zone != Zone.Name)
						{
							goto failed;
						}

						if(h.Nuid == Nuid)
						{
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"To {peer.IP}. It's me" );
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
							goto failed;
						}
													
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing}", $"Reported IP {IP}");
						}
	
						if(peer.Status == ConnectionStatus.OK)
						{
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"To {peer.IP}. Already established" );
							client.Close();
							return;
						}
	
						RefreshPeers(h.Peers.Append(peer));
	
						peer.Start(this, client, h, $"{Settings.IP?.GetAddressBytes()[3]}", false);
						
						Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing}", $"Connected to {peer}");
	
						return;
					}
	
					failed:
					{
						lock(Lock)
							peer.Disconnect();;
									
						client.Close();
					}
				}
				catch(Exception ex) when(!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
			
			var t = new Thread(f);
			t.Name = Settings.IP?.GetAddressBytes()[3] + " -> out -> " + peer.IP.GetAddressBytes()[3];
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

				peer.Disconnect();
				
				while(peer.Status != ConnectionStatus.Disconnected) 
					Thread.Sleep(1);
								
				peer.Status = ConnectionStatus.Initiated;
			}

			var t = new Thread(a => incon());
			t.Name = Settings.IP?.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
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
						Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"From {ip}. WaitHello -> {ex.Message}");
						goto failed;
					}
				
					lock(Lock)
					{
						if(Workflow.Aborted)
							return;

						if(h.Permanent)
						{
							if(Connections.Count(i => i.Inbound && i.Permanent) + 1 > Settings.PeersPermanentInboundMax)
							{
								goto failed;
							}
						}

						if(!h.Versions.Any(i => Versions.Contains(i)))
						{
							goto failed;
						}

						if(h.Zone != Zone.Name)
						{
							goto failed;
						}

						if(h.Nuid == Nuid)
						{
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"From {ip}. It's me");
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
							goto failed;
						}

						if(peer != null && peer.Status == ConnectionStatus.OK)
						{
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"From {ip}. Already established" );
							goto failed;
						}
	
						if(IP.Equals(IPAddress.None))
						{
							IP = h.IP;
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"Reported IP {IP}");
						}
		
						try
						{
							Peer.SendHello(client, CreateHello(ip, false));
						}
						catch(Exception ex) when(!DevSettings.ThrowOnCorrupted)
						{
							Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing} {Tag.Error}", $"From {ip}. SendHello -> {ex.Message}");
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
						peer.Permanent = h.Permanent;
						peer.Start(this, client, h, $"{Settings.IP?.GetAddressBytes()[3]}", true);
						Workflow.Log?.Report(this, $"{Tag.Peering} {Tag.Establishing}", $"Connected from {peer}");
			
						//Workflow.Log?.Report(this, "Accepted from", $"{peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
						return;
					}
	
				failed:
					if(peer != null)
						lock(Lock)
							peer.Disconnect();;

					client.Close();
				}
				catch(Exception ex) when(!Debugger.IsAttached)
				{
					Stop(MethodBase.GetCurrentMethod(), ex);
				}
			}
		}

		public void Synchronize()
		{
			if(Settings.IP != null && Settings.IP.Equals(Zone.Father0IP) && Settings.Generators.Contains(Zone.Father0) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound || DevSettings.SkipSynchronization)
			{
				Synchronization = Synchronization.Synchronized;
				return;
			}

			if(Synchronization != Synchronization.Downloading)
			{
				Workflow.Log?.Report(this, $"{Tag.Synchronization}", "Started");

				SynchronizingThread = new Thread(Synchronizing);
				SynchronizingThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Synchronizing";
				SynchronizingThread.Start();
		
				Synchronization = Synchronization.Downloading;
			}
		}

		void Synchronizing()
		{
			var used = new HashSet<Peer>();
	
			StampResponse stamp = null;
			Peer peer = null;

			lock(Lock)
				SyncTail.Clear();

			while(Workflow.Active)
			{
				try
				{
					WaitHandle.WaitAny(new[] {Workflow.Cancellation.WaitHandle}, 500);

					peer = Connect(Mcv.Roles.HasFlag(Role.Chain) ? Role.Chain : Role.Base, used, Workflow);

					if(Mcv.Roles.HasFlag(Role.Base) && !Mcv.Roles.HasFlag(Role.Chain))
					{
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
																			Tables.Analyses => stamp.Analyses.Where(i =>{
																															var c = Mcv.Analyses.SuperClusters.ContainsKey(i.Id);
																															return !c || !Mcv.Analyses.SuperClusters[i.Id].SequenceEqual(i.Hash);
																														}),
																			_ => throw new SynchronizationException("Unknown table recieved after GetTableStamp")
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
										
									lock(Lock)
									{
										using(var b = new WriteBatch())
										{
											c.Save(b);
			
											if(!c.Hash.SequenceEqual(i.Hash))
											{
												throw new SynchronizationException("Cluster hash mismatch");
											}
										
											Mcv.Engine.Write(b);
										}
									}
		
									Workflow.Log?.Report(this, $"{Tag.Synchronization}", $"Cluster downloaded {t.GetType().Name}, {c.Id}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						download<AccountEntry, AccountAddress>(Mcv.Accounts);
						download<AuthorEntry, string>(Mcv.Authors);
						download<AnalysisEntry, byte[]>(Mcv.Analyses);
		
						var r = new Round(Mcv) {Confirmed = true};
						r.ReadBaseState(new BinaryReader(new MemoryStream(stamp.BaseState)));
		
						lock(Lock)
						{
							Mcv.BaseState = stamp.BaseState;
							Mcv.LastConfirmedRound = r;
							Mcv.LastCommittedRound = r;
			
							Mcv.Hashify();
			
							if(peer.GetStamp().BaseHash.SequenceEqual(Mcv.BaseHash))
	 							Mcv.LoadedRounds[r.Id] = r;
							else
								throw new SynchronizationException("BaseHash mismatch");
						}
					}
		
					//int finalfrom = -1; 
					int from = -1;
					int to = -1;

					//lock(Lock)
					//	Mcv.Tail.RemoveAll(i => i.Id > Mcv.LastConfirmedRound.Id);

					while(Workflow.Active)
					{
						lock(Lock)
							if(Mcv.Roles.HasFlag(Role.Chain))
								from = Mcv.LastConfirmedRound.Id + 1;
							else
								from = Math.Max(stamp.FirstTailRound, Mcv.LastConfirmedRound == null ? -1 : (Mcv.LastConfirmedRound.Id + 1));
		
						to = from + Mcv.P;
		
						var rp = peer.Request<DownloadRoundsResponse>(new DownloadRoundsRequest{From = from, To = to});

						lock(Lock)
						{
							var rounds = rp.Read(Mcv);
														
							for(int rid = from; rounds.Any() && rid <= rounds.Max(i => i.Id); rid++)
							{
								var r = rounds.FirstOrDefault(i => i.Id == rid);

								if(r == null)
									break;
								
								Workflow.Log?.Report(this, $"{Tag.Synchronization}", $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.IP}");
									
								if(Mcv.LastConfirmedRound.Id + 1 != rid)
								 	throw new IntegrityException();
	
								if(Enumerable.Range(rid, Mcv.P + 1).All(i => SyncTail.ContainsKey(i)) && (Mcv.Roles.HasFlag(Role.Chain) || Mcv.FindRound(r.VotersRound) != null))
								{
									var p =	SyncTail[rid];
									var c =	SyncTail[rid + Mcv.P];

									try
									{
										foreach(var v in p.Votes)
											ProcessIncoming(v, true);
		
										foreach(var v in c.Votes)
											ProcessIncoming(v, true);
									}
									catch(ConfirmationException)
									{
									}

									if(Mcv.LastConfirmedRound.Id == rid && Mcv.LastConfirmedRound.Hash.SequenceEqual(r.Hash))
									{
										//Mcv.Commit(Mcv.LastConfirmedRound);
										
										foreach(var i in SyncTail.OrderBy(i => i.Key).Where(i => i.Key > rid))
										{
											foreach(var v in i.Value.Votes)
												ProcessIncoming(v, true);
										}
										
										Synchronization = Synchronization.Synchronized;
										SyncTail.Clear();
										SynchronizingThread = null;
						
										MainSignal.Set();

										Workflow.Log?.Report(this, $"{Tag.Synchronization}", "Finished");
										return;
									}
								}

								Mcv.Tail.RemoveAll(i => i.Id >= rid);
								Mcv.Tail.Insert(0, r);
			
								var h = r.Hash;

								r.Hashify();

								if(!r.Hash.SequenceEqual(h))
									throw new SynchronizationException("!r.Hash.SequenceEqual(h)");

								r.Confirmed = false;
								Mcv.Execute(r, r.ConfirmedTransactions);
								Mcv.Confirm(r);

								if(r.Members.Count == 0)
									throw new SynchronizationException("Incorrect round (Members.Count == 0)");

								Mcv.Commit(r);
								
								foreach(var i in SyncTail.Keys)
									if(i <= rid)
										SyncTail.Remove(i);
							}
						}
					}
				}
				catch(ConfirmationException ex)
				{
					Workflow.Log?.ReportError(this, ex.Message);

					lock(Lock)
					{	
						//foreach(var i in SyncTail.Keys)
						//	if(i <= ex.Round.Id)
						//		SyncTail.Remove(i);

						Mcv.Tail.RemoveAll(i => i.Id >= ex.Round.Id);
						//Mcv.LastConfirmedRound = Mcv.Tail.First();
					}
				}
				catch(SynchronizationException ex)
				{
					Workflow.Log?.ReportError(this, ex.Message);

					used.Add(peer);

					lock(Lock)
					{	
						//SyncTail.Clear();
						Mcv.Clear();
					}
				}
				catch(RdcNodeException ex)
				{
					used.Add(peer);
				}
				catch(RdcEntityException)
				{
				}
				catch(OperationCanceledException)
				{
					return;
				}
			}
		}

		public bool ProcessIncoming(Vote v, bool assynchronized)
		{
			if(!v.Valid)
				return false;

			if(!assynchronized && (Synchronization == Synchronization.None || Synchronization == Synchronization.Downloading))
			{
 				//var min = SyncTail.Any() ? SyncTail.Max(i => i.Key) - Mcv.Pitch * 100 : 0; /// keep latest Pitch * 3 rounds only
 
				if(SyncTail.TryGetValue(v.RoundId, out var r) && r.Votes.Any(j => j.Signature.SequenceEqual(v.Signature)))
					return false;

				if(!SyncTail.TryGetValue(v.RoundId, out r))
				{
					r = SyncTail[v.RoundId] = new();
				}

				v.Created = DateTime.UtcNow;
				r.Votes.Add(v);

				//foreach(var i in SyncTail.Keys)
				//{
				//	if(i < min)
				//	{
				//		SyncTail.Remove(i);
				//	}
				//}
			}
			else if(assynchronized || Synchronization == Synchronization.Synchronized)
			{
				if(v.RoundId <= Mcv.LastConfirmedRound.Id || Mcv.LastConfirmedRound.Id + Mcv.P * 2 < v.RoundId)
					return false;

				//if(v.RoundId <= Mcv.LastVotedRound.Id - Mcv.Pitch / 2)
				//	return false;

				var r = Mcv.GetRound(v.RoundId);

				if(!Mcv.VotersOf(r).Any(i => i.Account == v.Generator))
					return false;

				if(r.Votes.Any(i => i.Signature.SequenceEqual(v.Signature)))
					return false;
								
				if(r.Parent != null && r.Parent.Members.Count > 0)
				{
					if(v.Transactions.Length > r.Parent.TransactionsPerVoteAllowableOverflow)
						return false;

					if(v.Transactions.Sum(i => i.Operations.Length) > r.Parent.OperationsPerVoteLimit)
						return false;

					if(v.Transactions.Any(t => Mcv.VotersOf(r).NearestBy(m => m.Account, t.Signer).Account != v.Generator))
						return false;
				}

				if(v.Transactions.Any(i => !i.Valid(Mcv))) /// do it only after adding to the chainbase
				{
					//r.Votes.Remove(v);
					return false;
				}

				var pc = Mcv.Add(v);

				if(pc)
				{
				}
			}

			return true;
		}

		public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			foreach(var i in txs.Where(i =>	!IncomingTransactions.Any(j => i.EqualBySignature(j)) &&
											i.Fee >= i.Operations.Length * Mcv.LastConfirmedRound.ConfirmedExeunitMinFee &&
											i.Expiration > Mcv.LastConfirmedRound.Id &&
											i.Valid(Mcv)).OrderByDescending(i => i.Nid))
			{
				var m = Mcv.LastConfirmedRound.Members.Where(i => i.CastingSince <= Mcv.LastConfirmedRound.Id + Mcv.P).NearestBy(m => m.Account, i.Signer).Account;

				if(!Settings.Generators.Contains(m))
					continue;

				var r = new Round(Mcv);
				r.Id = (Mcv.Tail.FirstOrDefault(r => !r.Confirmed && r.Votes.Any(v => v.Generator == m)) ?? Mcv.LastConfirmedRound).Id + 1;
				
				var prev = r.Previous.VotesOfTry.FirstOrDefault(j => j.Generator == m);
				r.ConfirmedTime = new Time(Mcv.LastConfirmedRound.ConfirmedTime.Ticks + (prev == null || prev.RoundId <= Mcv.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Created).TotalMilliseconds));
				r.Analyzers = Mcv.LastConfirmedRound.Analyzers.ToList();

				Mcv.Execute(r, new [] {i});

				if(i.Successful)
				{
					i.Placing = PlacingStage.Accepted;
					IncomingTransactions.Add(i);

					Workflow.Log?.Report(this, "Transaction Accepted", i.ToString());

					yield return i;
				}
			}

			MainSignal.Set();
		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var votes = new List<Vote>();


			foreach(var g in Settings.Generators)
			{
				var m = Mcv.LastConfirmedRound.Members.Find(i => i.Account == g);

				if(m == null)
				{
					var a = Mcv.Accounts.Find(g, Mcv.LastConfirmedRound.Id);

					if(a != null && a.Bail + a.Balance > Settings.Bail && a.CandidacyDeclarationRid <= Mcv.LastConfirmedRound.Id && (!LastCandidacyDeclaration.TryGetValue(g, out var d) || d.Placing > PlacingStage.Placed))
					{
						var o = new CandidacyDeclaration{	Bail = Settings.Bail,
															BaseRdcIPs		= new IPAddress[] {Settings.IP},
															SeedHubRdcIPs	= new IPAddress[] {Settings.IP}};

						var t = new Transaction(Zone);
						t.Signer = g;
 						t.__ExpectedPlacing = PlacingStage.Confirmed;
			
						t.AddOperation(o);

			 			Enqueue(t);

						LastCandidacyDeclaration[g] = t;
					}
				}
				else
				{
					//if(Mcv.VotersOf(Mcv.LastConfirmedRound).Any(i => i.Account == g) && !Mcv.LastConfirmedRound.VotesOfTry.Any(i => i.Generator == g))
					//	votes = votes;

					if(LastCandidacyDeclaration.TryGetValue(g, out var lcd))
						OutgoingTransactions.Remove(lcd);

					var r = Mcv.GetRound(Mcv.LastConfirmedRound.Id + 1 + Mcv.P);

					if(r.Id < m.CastingSince)
						continue;

					if(r.VotesOfTry.Any(i => i.Generator == g))
						continue;

					var txs = new List<Transaction>();

					foreach(var i in IncomingTransactions.Where(i => i.Placing == PlacingStage.Accepted).OrderByDescending(i => i.Fee).ToArray())
					{
						var nearest = Mcv.VotersOf(r).NearestBy(m => m.Account, i.Signer).Account;

						if(!Settings.Generators.Contains(nearest))
						{
							i.Placing = PlacingStage.FailedOrNotFound;
							IncomingTransactions.Remove(i);
							continue;
						}

						if(r.Id > i.Expiration)
						{
							i.Placing = PlacingStage.FailedOrNotFound;
							IncomingTransactions.Remove(i);
							continue;
						}

						if(nearest == g)
							txs.Add(i);
					}

					Vote createvote(Round r)
					{
						var prev = r.Previous?.VotesOfTry.FirstOrDefault(i => i.Generator == g);
						
						return new Vote(Mcv) {	RoundId			= r.Id,
												Try				= r.Try,
												ParentHash		= r.Parent.Hash ?? Mcv.Summarize(r.Parent),
												Created			= Clock.Now,
												TimeDelta		= prev == null || prev.RoundId <= Mcv.LastGenesisRound ? 0 : (long)(Clock.Now - prev.Created).TotalMilliseconds,
												Violators		= Mcv.ProposeViolators(r).ToArray(),
												MemberLeavers	= Mcv.ProposeMemberLeavers(r, g).ToArray(),
												AnalyzerJoiners	= Settings.ProposedAnalyzerJoiners.Where(i => !Mcv.LastConfirmedRound.Analyzers.Any(j => j.Account == i)).ToArray(),
												AnalyzerLeavers	= Settings.ProposedAnalyzerLeavers.Where(i => Mcv.LastConfirmedRound.Analyzers.Any(j => j.Account == i)).ToArray(),
												FundJoiners		= Settings.ProposedFundJoiners.Where(i => !Mcv.LastConfirmedRound.Funds.Contains(i)).ToArray(),
												FundLeavers		= Settings.ProposedFundLeavers.Where(i => Mcv.LastConfirmedRound.Funds.Contains(i)).ToArray(),
												Emissions		= ApprovedEmissions.ToArray(),
												DomainBids		= ApprovedDomainBids.ToArray() };
					}
	
					if(txs.Any() || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any())) /// any pending foreign transactions or any our pending operations OR some unconfirmed payload 
					{
						var v = createvote(r);
	
						if(txs.Any())
						{
							foreach(var i in txs.OrderBy(i => i.Nid))
							{
								if(v.Transactions.Sum(i => i.Operations.Length) + i.Operations.Length > r.Parent.OperationsPerVoteLimit)
									break;

								if(v.Transactions.Length + 1 > r.Parent.TransactionsPerVoteAllowableOverflow)
									break;

								//if(Mcv.VotersOf(r).NearestBy(m => m.Account, i.Signer).Account != i.Member)
								//{
								//	IncomingTransactions.Remove(i);
								//	continue;
								//}

								v.AddTransaction(i);

								i.Placing = PlacingStage.Placed;
								Workflow.Log?.Report(this, "Transaction Placed", i.ToString());
							}
						}
						
						v.Sign(g);
						votes.Add(v);
					}

 					while(r.Previous != null && !r.Previous.Confirmed && Mcv.VotersOf(r.Previous).Any(i => i.Account == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
 					{
 						r = r.Previous;
 
 						var b = createvote(r);
 								
 						b.Sign(g);
 						votes.Add(b);
 					}

					if(IncomingTransactions.Any(i => i.Placing == PlacingStage.Accepted) || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						MainSignal.Set();
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

					for(int i = votes.Min(i => i.RoundId); i <= Mcv.LastNonEmptyRound.Id; i++)
					{
						var r = Mcv.FindRound(i);
						r.Analyzers = Mcv.LastConfirmedRound.Analyzers.ToList();

						if(r != null && !r.Confirmed)
						{
							var txs = r.OrderedTransactions.Where(i => Settings.Generators.Contains(i.Vote.Generator));
							
							Mcv.Execute(r, txs);
						}
					}
				}
				catch(ConfirmationException ex)
				{
					ProcessConfirmationException(ex);
				}

				foreach(var i in votes)
				{
					Broadcast(i);
				}
													
				 Workflow.Log?.Report(this, "Block(s) generated", string.Join(", ", votes.Select(i => $"{i.Generator.Bytes.ToHexPrefix()}-{i.RoundId}")));
			}

			Statistics.Generating.End();
		}

		public void ProcessConfirmationException(ConfirmationException ex)
		{
			Workflow.Log?.ReportError(this, ex.Message);
			Mcv.Tail.RemoveAll(i => i.Id >= ex.Round.Id);

			//foreach(var i in IncomingTransactions.Where(i => i.Vote != null && i.Vote.RoundId >= ex.Round.Id && (i.Placing == PlacingStage.Placed || i.Placing == PlacingStage.Confirmed)).ToArray())
			//{
			//	i.Placing = PlacingStage.Accepted;
			//}

			Synchronize();
		}

		void Transacting()
		{
			IEnumerable<Transaction>	accepted;

			Workflow.Log?.Report(this, "Delegating started");

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

				//Thread.Sleep(100);
				
				var cr = Call(i => i.GetMembers(), Workflow);

				if(!cr.Members.Any() || cr.Members.Any(i => !i.BaseRdcIPs.Any() || !i.SeedHubRdcIPs.Any()))
					continue;

				var members = cr.Members;

				RdcInterface getrdi(AccountAddress account)
				{
					var m = members.NearestBy(i => i.Account, account);

					if(m.BaseRdcIPs.Contains(Settings.IP))
						return this;

					var p = GetPeer(m.BaseRdcIPs.Random());
					Connect(p, Workflow);

					return p;
				}

				Statistics.Transacting.Begin();
				
				lock(Lock)
				{
					foreach(var g in OutgoingTransactions.GroupBy(i => i.Signer).ToArray())
					{
						if(!g.Any(i => i.Placing >= PlacingStage.Accepted) && g.Any(i => i.Placing == PlacingStage.None))
						{
							var m = members.NearestBy(i => i.Account, g.Key);

							AllocateTransactionResponse at = null;
							RdcInterface rdi; 

							try
							{
								Monitor.Exit(Lock);

								rdi = getrdi(g.Key);
								at = rdi.Request<AllocateTransactionResponse>(new AllocateTransactionRequest {Account = g.Key});
							}
							catch(RdcNodeException)
							{
								Thread.Sleep(1000);
								continue;
							}
							finally
							{
								Monitor.Enter(Lock);
							}
									
							int nid = at.NextTransactionId;
							var txs = new List<Transaction>();

							foreach(var t in g.Where(i => i.Placing == PlacingStage.None))
							{
								t.Nid = nid++;
								t.Rdc = rdi;
								t.Expiration = at.LastConfirmedRid + Mcv.TransactionPlacingLifetime;
								t.Fee = t.Operations.Length * at.ExeunitMinFee;
	
								t.Sign(Vault.GetKey(t.Signer), at.PowHash);
								txs.Add(t);
							}

							IEnumerable<byte[]> atxs = null;

							try
							{
								Monitor.Exit(Lock);
								atxs = rdi.SendTransactions(txs).Accepted;
							}
							catch(RdcNodeException)
							{
								Thread.Sleep(1000);
								continue;
							}
							finally
							{
								Monitor.Enter(Lock);
							}

							foreach(var t in txs)
							{ 
								if(atxs.Any(s => s.SequenceEqual(t.Signature)))
								{
									t.Placing = PlacingStage.Accepted;

									Workflow.Log?.Report(this, "Operation(s) accepted", $"N={t.Operations.Length} -> {m}, {t.Rdc}");
								}
								else
								{
									if(t.__ExpectedPlacing == PlacingStage.FailedOrNotFound)
									{
										t.Placing = PlacingStage.FailedOrNotFound;
										OutgoingTransactions.Remove(t);
									} 
									else
									{
										t.Placing = PlacingStage.None;
									}
								}
							}
						}
					}

					accepted = OutgoingTransactions.Where(i => i.Placing == PlacingStage.Accepted || i.Placing == PlacingStage.Placed).ToArray();

					if(accepted.Any())
					{
						foreach(var g in accepted.GroupBy(i => i.Rdc).ToArray())
						{
							TransactionStatusResponse ts;

							try
							{
								Monitor.Exit(Lock);
								ts = g.Key.GetTransactionStatus(g.Select(i => new TransactionsAddress {Account = i.Signer, Nid = i.Nid}));
							}
							catch(RdcNodeException)
							{
								Thread.Sleep(1000);
								continue;
							}
							finally
							{
								Monitor.Enter(Lock);
							}

							foreach(var i in ts.Transactions)
							{
								var t = accepted.First(d => d.Signer == i.Account && d.Nid == i.Nid);
																		
								if(t.Placing != i.Placing)
								{
									t.Placing = i.Placing;

									if(t.Placing == PlacingStage.FailedOrNotFound)
									{
										if(t.__ExpectedPlacing == PlacingStage.Confirmed)
											t.Placing = PlacingStage.None;
									}
									else if(t.Placing == PlacingStage.Confirmed)
									{
										if(t.__ExpectedPlacing == PlacingStage.FailedOrNotFound)
											Debugger.Break();
										else
											OutgoingTransactions.Remove(t);
									}
								}
							}
						}
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

					TransactingThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Transacting";
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
		
		public Transaction Enqueue(Operation operation, AccountAddress signer, PlacingStage await, Workflow workflow)
		{
			return Enqueue(new Operation[] {operation}, signer, await, workflow)[0];
		}

 		public Transaction[] Enqueue(IEnumerable<Operation> operations, AccountAddress signer, PlacingStage await, Workflow workflow)
 		{
			var p = new List<Transaction>();

			while(operations.Any())
			{
				var t = new Transaction(Zone);
				t.Signer = signer;
 				t.__ExpectedPlacing = await;
			
				foreach(var i in operations.Take(Zone.OperationsPerTransactionLimit))
				{
					t.AddOperation(i);
				}

 				lock(Lock)
				{	
 					if(FeeAsker.Ask(this, signer, null))
 					{
 				 		Enqueue(t);
 					}
				}
 
				Await(t, await, workflow);

				p.Add(t);

				operations = operations.Skip(Zone.OperationsPerTransactionLimit);
			}

			return p.ToArray();
 		}

		void Await(Transaction t, PlacingStage s, Workflow workflow)
		{
			while(workflow.Active)
			{ 
				switch(s)
				{
					case PlacingStage.None :				return;
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
				catch(RdcNodeException)
				{
				}
			}

			throw new OperationCanceledException();
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
					catch(RdcNodeException)
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

			throw new OperationCanceledException();
		}

		public void Connect(Peer peer, Workflow workflow)
		{
			lock(Lock)
			{
				if(peer.Status == ConnectionStatus.OK)
					return;
				else if(peer.Status == ConnectionStatus.Disconnected)
					OutboundConnect(peer, false);
				else if(peer.Status == ConnectionStatus.Disconnecting)
				{	
					while(peer.Status != ConnectionStatus.Disconnected)
						Thread.Sleep(0);
					
					OutboundConnect(peer, false);
				}
			}

			var t = DateTime.Now;

			while(workflow.Active)
			{
				lock(Lock)
					if(peer.Status == ConnectionStatus.OK)
						return;
					else if(peer.Status == ConnectionStatus.Disconnecting || peer.Status == ConnectionStatus.Disconnected)
						throw new RdcNodeException(RdcNodeError.Connectivity);

				if(!DevSettings.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new RdcNodeException(RdcNodeError.Timeout);
				
				Thread.Sleep(1);
			}
		}


		public R Call<R>(Func<RdcInterface, R> call, Workflow workflow, IEnumerable<Peer> exclusions = null)
		{
			var tried = exclusions != null ? new HashSet<Peer>(exclusions) : new HashSet<Peer>();

			Peer p;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					if(Synchronization == Synchronization.Synchronized)
					{
						return call(this);
					}

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

		//public double EstimateFee(IEnumerable<Operation> operations)
		//{
		//	return Mcv != null ? operations.Sum(i => (double)i.CalculateTransactionFee(TransactionPerByteMinFee).ToDecimal()) : double.NaN;
		//}

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

		public override RdcResponse Request(RdcRequest rq)
  		{
			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;
				rq = BinarySerializator.Deserialize<RdcRequest>(new(s), Constract);
			}

 			return rq.Execute(this);
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
					i.Send(new VoteRequest {Raw = vote.RawForBroadcast });
				}
				catch(RdcNodeException)
				{
				}
			}
		}

		//public QueryResourceResponse QueryResource(string query, Workflow workflow) => Call(c => c.QueryResource(query), workflow);
		//public ResourceResponse FindResource(ResourceAddress query, Workflow workflow) => Call(c => c.FindResource(query), workflow);
	}
}
