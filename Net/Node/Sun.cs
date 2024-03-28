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
		Seed		= 0b00000100,
	}

	public class Sun : RdcInterface
	{
		public Role						Roles => (Mcv != null ? Mcv.Roles : Role.None)|(ResourceHub != null ? Role.Seed : Role.None);

		public System.Version			Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]	Versions = {4};
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

		public Guid						Netid;
		public IPAddress				IP = IPAddress.None;
		public bool						IsNodeOrUserRun => MainThread != null;
		public bool						IsClient => ListeningThread == null;
		public Round					NextVoteRound => Mcv.GetRound(Mcv.LastConfirmedRound.Id + 1 + Mcv.P);
		public List<Member>				NextVoteMembers => Mcv.VotersOf(NextVoteRound);

		public Statistics				PrevStatistics = new();
		public Statistics				Statistics = new();

		public List<TcpClient>			IncomingConnections = new();
		public List<Transaction>		IncomingTransactions = new();
		internal List<Transaction>		OutgoingTransactions = new();
		//public List<Analysis>			Analyses = new();
		public List<OperationId>		ApprovedEmissions = new();
		public List<OperationId>		ApprovedDomainBids = new();

		public bool						MinimalPeersReached;
		public List<Peer>				Peers = new();
		public IEnumerable<Peer>		Connections	=> Peers.Where(i => i.Status == ConnectionStatus.OK);
		public IEnumerable<Peer>		Bases => Connections.Where(i => i.Permanent && i.BaseRank > 0);

		public List<IPAddress>			IgnoredIPs = new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		Thread							TransactingThread;
		Thread							SynchronizingThread;
		AutoResetEvent					MainWakeup = new AutoResetEvent(true);
		AutoResetEvent					TransactingWakeup = new AutoResetEvent(true);

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
			public const string P = "Peering";
			public const string E = "Establishing";
			public const string S = "Synchronization";
			public const string ERR = "Error";
		}
		
		public Sun(Zone zone, Settings settings, Workflow workflow)
		{
			Zone = zone;
			Settings = settings;
			Directory.CreateDirectory(Settings.Profile);
			
			Workflow = workflow ?? new Workflow("Sun", new Log());

			if(Workflow.Log != null)
			{
				Workflow.Log.Reported += m => File.AppendAllText(Path.Combine(Settings.Profile, "Sun.log"), m.ToString() + Environment.NewLine);
			}

			Vault = new Vault(Zone, Settings);

			var fs = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[] {	new (nameof(Peers),					new ()),
																new (ResourceHub.ReleaseFamilyName,	new ()),
																new (ResourceHub.ResourceFamilyName,new ()) })
				fs.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Node"), fs);
		}

		public override string ToString()
		{
			var gens = Mcv?.LastConfirmedRound != null ? Settings.Generators.Where(i => Mcv.LastConfirmedRound.Members.Any(j => j.Account == i)) : new AccountKey[0];
	
			return string.Join(" - ", new string[]{	(Settings.IP != null ? $"{IP}" : null),
													$"{(Roles.HasFlag(Role.Base) ? "B" : null)}" +
													$"{(Roles.HasFlag(Role.Chain) ? "C" : null)}" +
													$"{(Roles.HasFlag(Role.Seed) ? "S" : null)}",
													Connections.Count() < Settings.PeersPermanentMin ? "Low Peers" : null,
													Mcv != null ? $"{Synchronization} - {Mcv.LastConfirmedRound?.Id} - {Mcv.LastConfirmedRound?.Hash.ToHexPrefix()}" : null,
													$"{OutgoingTransactions.Count}ot/{IncomingTransactions.Count}it"}
						.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		public object Constract(Type t, byte b)
		{
			if(t == typeof(Transaction))	return new Transaction {Zone = Zone};
			if(t == typeof(Vote))			return new Vote(Mcv);
			if(t == typeof(Manifest))		return new Manifest();
			if(t == typeof(RdcRequest))		return RdcRequest.FromType((RdcClass)b); 
			if(t == typeof(RdcResponse))	return RdcResponse.FromType((RdcClass)b); 
			if(t == typeof(Operation))		return Operation.FromType((OperationClass)b); 
			if(t == typeof(SunException))	return SunException.FromType((ExceptionClass)b); 
			if(t == typeof(ReleaseAddress))	return ReleaseAddress.FromType(b); 

			return null;
		}

		public Thread CreateThread(Action action)
		{
			return new Thread(() => { 
										try
										{
											action();
										}
										catch(OperationCanceledException)
										{
										}
										catch(Exception ex) when (!Debugger.IsAttached)
										{
											if(Workflow.Active)
												Abort(ex);
										}
									});
		}

		public void Run(Xon xon)
		{
			if(xon.Has("api"))
				RunApi();
			
			/// workflow.Log.Stream = new FileStream(Path.Combine(Settings.Profile, "Node.log"), FileMode.Create)

			if(xon.Has("peer"))
				RunNode((xon.Has("base") ? Role.Base : Role.None) | (xon.Has("chain") ? Role.Chain : Role.None));

			if(xon.Has("seed"))
				RunSeed();
		}
		
		public void RunApi()
		{
			if(!HttpListener.IsSupported)
			{
				Environment.ExitCode = -1;
				throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
			}

			if(ApiServer != null)
				throw new NodeException(NodeError.AlreadyRunning);

			lock(Lock)
			{
				ApiServer = new SunJsonApiServer(this, Workflow);
			}
		
			ApiStarted?.Invoke(this);
		}

		public void RunNode(Role roles)
		{
			if(Netid != Guid.Empty)
				throw new NodeException(NodeError.AlreadyRunning);

			Workflow.Log?.Report(this, $"Ultranet Node {Version}");
			Workflow.Log?.Report(this, $"Runtime: {Environment.Version}");	
			Workflow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Workflow.Log?.Report(this, $"Zone: {Zone.Name}");
			Workflow.Log?.Report(this, $"Profile: {Settings.Profile}");	
			
			if(SunGlobals.Any)
				Workflow.Log?.ReportWarning(this, $"Dev: {SunGlobals.AsString}");

			Netid = Guid.NewGuid();

			if(Settings.Generators.Any())
			{
				SeedHub = new SeedHub(this);
			}

			if(roles.HasFlag(Role.Base) || roles.HasFlag(Role.Chain))
			{
				Mcv = new Mcv(Zone, roles, Settings.Mcv, Path.Join(Settings.Profile, nameof(Mcv)));

				Mcv.Log = Workflow.Log;
				Mcv.VoteAdded += b => MainWakeup.Set();

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
										if(Mcv.LastConfirmedRound.Members.Any(i => Settings.Generators.Contains(i.Account)))
										{
											var ops = r.ConsensusTransactions.SelectMany(t => t.Operations).ToArray();
												
											foreach(var o in ops)
											{
												if(o is AuthorBid ab && ab.Tld.Any())
												{
	 												if(!SunGlobals.SkipDomainVerification)
	 												{
														Task.Run(() =>	{
	 																		try
	 																		{
	 																			var result = Dns.QueryAsync(ab.Author + '.' + ab.Tld, QueryType.TXT, QueryClass.IN, Workflow.Cancellation);
	 															
	 																			var txt = result.Result.Answers.TxtRecords().FirstOrDefault(r => r.DomainName == ab.Author + '.' + ab.Tld + '.');
	 		
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

										ApprovedEmissions.RemoveAll(i => r.ConsensusEmissions.Contains(i) || r.Id > i.Ri + Zone.ExternalVerificationDurationLimit);
										ApprovedDomainBids.RemoveAll(i => r.ConsensusDomainBids.Contains(i) || r.Id > i.Ri + Zone.ExternalVerificationDurationLimit);
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
				ListeningThread = CreateThread(Listening);
				ListeningThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Listening";
				ListeningThread.Start();
			}

 			MainThread = CreateThread(() =>	{ 
												while(Workflow.Active)
												{
													var r = WaitHandle.WaitAny(new[] {MainWakeup, Workflow.Cancellation.WaitHandle}, 500);

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
 											});

			MainThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Main";
			MainThread.Start();
			MainStarted?.Invoke(this);
		}

		public void RunSeed()
		{
			if(ResourceHub != null || PackageHub != null)
				throw new NodeException(NodeError.AlreadyRunning);

			ResourceHub = new ResourceHub(this, Zone, System.IO.Path.Join(Settings.Profile, "Releases"));
			PackageHub = new PackageHub(this, Settings.Packages);
		}

		public void Abort(Exception ex)
		{
			lock(Lock)
			{
				File.WriteAllText(Path.Join(Settings.Profile, "Abort." + Sun.FailureExt), ex.ToString());
				Workflow.Log?.ReportError(this, "Abort", ex);
	
				Stop("Due to exception");
			}
		}

		public void Stop(string message)
		{
			Workflow.Abort();

			ApiServer?.Stop();
			Listener?.Stop();


			lock(Lock)
			{
				foreach(var i in IncomingConnections)
					i.Close();

				foreach(var i in Peers.Where(i => i.Status != ConnectionStatus.Disconnected).ToArray())
					i.Disconnect();
			}

			MainThread?.Join();
			ListeningThread?.Join();
			TransactingThread?.Join();
			SynchronizingThread?.Join();

			Mcv?.Database.Dispose();
			Database?.Dispose();

			Workflow.Log?.Report(this, "Stopped", message);

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
				Workflow.Log?.Report(this, "PEE loaded", $"n={Peers.Count}");
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

		DateTime __X = DateTime.Now;

		void ProcessConnectivity()
		{
// 			if(DateTime.Now - __X > TimeSpan.FromSeconds(1))
// 			{
// 				__X = DateTime.Now;
// 				Workflow.Log?.Report(this, __X.ToString());
// 			}

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
				Workflow.Log?.Report(this, $"{Tag.P}", "Minimal peers reached");

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
				Workflow.Log?.Report(this, $"{Tag.P}", $"Listening starting {Settings.IP}:{Zone.Port}");

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
			h.Nuid			= Netid;
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

			TcpClient tcp = null;
			
			void f()
			{

				try
				{
					tcp = Settings.IP != null ? new TcpClient(new IPEndPoint(Settings.IP, 0)) : new TcpClient();

					tcp.SendTimeout = SunGlobals.DisableTimeouts ? 0 : Timeout;
					//client.ReceiveTimeout = Timeout;
					tcp.Connect(peer.IP, Zone.Port);
				}
				catch(SocketException ex) 
				{
					Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"To {peer.IP}. {ex.Message}" );
					goto failed;
				}
	
				Hello h = null;
									
				try
				{
					tcp.SendTimeout = SunGlobals.DisableTimeouts ? 0 : Timeout;
					tcp.ReceiveTimeout = SunGlobals.DisableTimeouts ? 0 : Timeout;

					Peer.SendHello(tcp, CreateHello(peer.IP, permanent));
					h = Peer.WaitHello(tcp);
				}
				catch(Exception ex)// when(!Settings.Dev.ThrowOnCorrupted)
				{
					Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"To {peer.IP}. {ex.Message}" );
					goto failed;
				}
	
				lock(Lock)
				{
					if(Workflow.Aborted)
					{
						tcp.Close();
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

					if(h.Nuid == Netid)
					{
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"To {peer.IP}. It's me" );
						IgnoredIPs.Add(peer.IP);
						Peers.Remove(peer);
						goto failed;
					}
													
					if(IP.Equals(IPAddress.None))
					{
						IP = h.IP;
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E}", $"Reported IP {IP}");
					}
	
					if(peer.Status == ConnectionStatus.OK)
					{
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"To {peer.IP}. Already established" );
						tcp.Close();
						return;
					}
	
					RefreshPeers(h.Peers.Append(peer));
	
					peer.Start(this, tcp, h, $"{Settings.IP?.GetAddressBytes()[3]}", false);
						
					Workflow.Log?.Report(this, $"{Tag.P} {Tag.E}", $"Connected to {peer}");
	
					return;
				}
	
				failed:
				{
					lock(Lock)
						peer.Disconnect();;
									
					tcp?.Close();
				}
			}
			
			var t = CreateThread(f);
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
				
				Monitor.Exit(Lock);

				while(Workflow.Active && peer.Status != ConnectionStatus.Disconnected) 
					Thread.Sleep(1);

				Monitor.Enter(Lock);
								
				peer.Status = ConnectionStatus.Initiated;
			}

			IncomingConnections.Add(client);

			var t = CreateThread(incon);
			t.Name = Settings.IP?.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
			t.Start();

			void incon()
			{
				Hello h = null;
	
				try
				{
					client.SendTimeout = SunGlobals.DisableTimeouts ? 0 : Timeout;
					client.ReceiveTimeout = SunGlobals.DisableTimeouts ? 0 : Timeout;

					h = Peer.WaitHello(client);
				}
				catch(Exception ex) when(!SunGlobals.ThrowOnCorrupted)
				{
					Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"From {ip}. WaitHello -> {ex.Message}");
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

					if(h.Nuid == Netid)
					{
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"From {ip}. It's me");
						if(peer != null)
						{	
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
						}
						goto failed;
					}

					if(peer != null && peer.Status == ConnectionStatus.OK)
					{
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"From {ip}. Already established" );
						goto failed;
					}
	
					if(IP.Equals(IPAddress.None))
					{
						IP = h.IP;
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"Reported IP {IP}");
					}
		
					try
					{
						Peer.SendHello(client, CreateHello(ip, false));
					}
					catch(Exception ex) when(!SunGlobals.ThrowOnCorrupted)
					{
						Workflow.Log?.Report(this, $"{Tag.P} {Tag.E} {Tag.ERR}", $"From {ip}. SendHello -> {ex.Message}");
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
					Workflow.Log?.Report(this, $"{Tag.P} {Tag.E}", $"Connected from {peer}");
			
					IncomingConnections.Remove(client);
					//Workflow.Log?.Report(this, "Accepted from", $"{peer}, in/out/min/inmax/total={Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Settings.PeersInMax}/{Peers.Count}");
	
					return;
				}
	
			failed:
				if(peer != null)
					lock(Lock)
						peer.Disconnect();;

				client.Close();

			}
		}

		public void Synchronize()
		{
			if(Settings.IP != null && Settings.IP.Equals(Zone.Father0IP) && Settings.Generators.Contains(Zone.Father0) && Mcv.LastNonEmptyRound.Id == Mcv.LastGenesisRound || SunGlobals.SkipSynchronization)
			{
				Synchronization = Synchronization.Synchronized;
				return;
			}

			if(Synchronization != Synchronization.Downloading)
			{
				Workflow.Log?.Report(this, $"{Tag.S}", "Started");

				SynchronizingThread = CreateThread(Synchronizing);
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
		
						void download<E, K>(Table<E, K> t) where E : class, ITableEntry<K>
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
																			_ => throw new SynchronizationException("Unknown table recieved after GetTableStamp")
																		}
																).Select(i => i.Id).ToArray());
		
							foreach(var i in ts.Clusters)
							{
								var c = t.Clusters.Find(j => j.Id.SequenceEqual(i.Id));
		
								if(c == null || c.Hash == null || !c.Hash.SequenceEqual(i.Hash))
								{
									if(c == null)
									{
										c = new Table<E, K>.Cluster(t, i.Id);
										t.Clusters.Add(c);
									}
		
									var d = peer.DownloadTable(t.Type, i.Id, 0, i.Length);
											
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
										
											Mcv.Database.Write(b);
										}
									}
		
									Workflow.Log?.Report(this, $"{Tag.S}", $"Cluster downloaded {t.GetType().Name}, {c.Id.ToHex()}");
								}
							}
		
							t.CalculateSuperClusters();
						}
		
						download(Mcv.Accounts);
						download(Mcv.Authors);
		
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
								
								Workflow.Log?.Report(this, $"{Tag.S}", $"Round received {r.Id} - {r.Hash.ToHex()} from {peer.IP}");
									
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
						
										MainWakeup.Set();

										Workflow.Log?.Report(this, $"{Tag.S}", "Finished");
										return;
									}
								}

								Mcv.Tail.RemoveAll(i => i.Id >= rid);
								Mcv.Tail.Insert(0, r);
			
								var h = r.Hash;

								r.Hashify();

								if(!r.Hash.SequenceEqual(h))
								{
									#if DEBUG
										SunGlobals.CompareBase();
									#endif
									
									throw new SynchronizationException("!r.Hash.SequenceEqual(h)");
								}

								r.Confirmed = false;
								Mcv.Confirm(r);

								if(r.Members.Count == 0)
									throw new SynchronizationException("Incorrect round (Members.Count == 0)");

								Mcv.Commit(r);
								
								foreach(var i in SyncTail.Keys)
									if(i <= rid)
										SyncTail.Remove(i);
							}

							Thread.Sleep(1);
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

		public bool TryExecute(Transaction transaction)
		{
			var m = NextVoteMembers.NearestBy(m => m.Account, transaction.Signer).Account;

			if(!Settings.Generators.Contains(m))
				return false;

			var p = Mcv.Tail.FirstOrDefault(r => !r.Confirmed && r.Votes.Any(v => v.Generator == m)) ?? Mcv.LastConfirmedRound;

			var r = new Round(Mcv) {Id						= p.Id + 1,
									ConsensusTime			= Time.Now(Clock), 
									ConsensusExeunitFee	= p.ConsensusExeunitFee,
									Members					= p.Members,
									Funds					= p.Funds};
	
			Mcv.Execute(r, new [] {transaction});

			return transaction.Successful;
		}

		public IEnumerable<Transaction> ProcessIncoming(IEnumerable<Transaction> txs)
		{
			foreach(var i in txs.Where(i =>	!IncomingTransactions.Any(j => j.Signer == i.Signer && j.Nid == i.Nid) &&
											i.Fee >= i.Operations.Length * Mcv.LastConfirmedRound.ConsensusExeunitFee &&
											i.Expiration > Mcv.LastConfirmedRound.Id &&
											i.Valid(Mcv)).OrderByDescending(i => i.Nid))
			{
				if(TryExecute(i))
				{
					i.Placing = PlacingStage.Accepted;
					IncomingTransactions.Add(i);

					Workflow.Log?.Report(this, "Transaction Accepted", i.ToString());

					yield return i;
				}
			}

			MainWakeup.Set();
		}

		void Generate()
		{
			Statistics.Generating.Begin();

			var votes = new List<Vote>();


			foreach(var g in Settings.Generators)
			{
				var m = NextVoteMembers.Find(i => i.Account == g);

				if(m == null)
				{
					m = Mcv.LastConfirmedRound.Members.Find(i => i.Account == g);

					var a = Mcv.Accounts.Find(g, Mcv.LastConfirmedRound.Id);

					if(m == null && a != null && a.Bail + a.Balance > Settings.Bail &&	//a.CandidacyDeclarationRid <= Mcv.LastConfirmedRound.Id && 
																						(!LastCandidacyDeclaration.TryGetValue(g, out var d) || d.Placing > PlacingStage.Placed))
					{
						var o = new CandidacyDeclaration{	Bail = Settings.Bail,
															BaseRdcIPs		= new IPAddress[] {Settings.IP},
															SeedHubRdcIPs	= new IPAddress[] {Settings.IP}};

						var t = new Transaction();
						t.Zone = Zone;
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

					var r = NextVoteRound;

					if(r.Id < m.CastingSince)
						continue;

					if(r.VotesOfTry.Any(i => i.Generator == g))
						continue;

					Vote createvote(Round r)
					{
						var prev = r.Previous?.VotesOfTry.FirstOrDefault(i => i.Generator == g);
						
						return new Vote(Mcv) {	RoundId			= r.Id,
												Try				= r.Try,
												ParentHash		= r.Parent.Hash ?? Mcv.Summarize(r.Parent),
												Created			= Clock.Now,
												Time			= Time.Now(Clock),
												Violators		= Mcv.ProposeViolators(r).ToArray(),
												MemberLeavers	= Mcv.ProposeMemberLeavers(r, g).ToArray(),
												FundJoiners		= Settings.ProposedFundJoiners.Where(i => !Mcv.LastConfirmedRound.Funds.Contains(i)).ToArray(),
												FundLeavers		= Settings.ProposedFundLeavers.Where(i => Mcv.LastConfirmedRound.Funds.Contains(i)).ToArray(),
												Emissions		= ApprovedEmissions.ToArray(),
												DomainBids		= ApprovedDomainBids.ToArray() };
					}

					var txs = IncomingTransactions.Where(i => i.Placing == PlacingStage.Accepted).ToArray();
	
					if(txs.Any() || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any())) /// any pending foreign transactions or any our pending operations OR some unconfirmed payload 
					{
						var v = createvote(r);
						var deferred = new List<Transaction>();
						
						bool add(Transaction t, bool isdeferred)
						{ 	
							if(v.Transactions.Sum(i => i.Operations.Length) + t.Operations.Length > r.Parent.OperationsPerVoteLimit)
								return false;
	
							if(v.Transactions.Length + 1 > r.Parent.TransactionsPerVoteAllowableOverflow)
								return false;
	
							if(r.Id > t.Expiration)
							{
								t.Placing = PlacingStage.FailedOrNotFound;
								IncomingTransactions.Remove(t);
								return true;
							}

							var nearest = Mcv.VotersOf(r).NearestBy(m => m.Account, t.Signer).Account;

							if(nearest != g)
								return true;
	
							if(!Settings.Generators.Contains(nearest))
							{
								t.Placing = PlacingStage.FailedOrNotFound;
								IncomingTransactions.Remove(t);
								return true;
							}
	
							if(!isdeferred)
							{
								if(txs.Any(i => i.Signer == t.Signer && i.Placing == PlacingStage.Accepted && i.Nid < t.Nid)) /// any older tx left?
								{
									deferred.Add(t);
									return true;
								}
							}
							else
								deferred.Remove(t);

							t.Placing = PlacingStage.Placed;
							v.AddTransaction(t);
	
							var next = deferred.Find(i => i.Signer == t.Signer && i.Nid + 1 == t.Nid);

							if(next != null)
							{
								if(add(next, true) == false)
									return false;
							}
	
							Workflow.Log?.Report(this, "Transaction Placed", t.ToString());

							return true;
						}

						foreach(var t in txs.OrderByDescending(i => i.Fee).ToArray())
						{
							if(add(t, false) == false)
								break;
						}

						if(v.Transactions.Any() || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
						{
							v.Sign(Vault.GetKey(g));
							votes.Add(v);
						}
					}

 					while(r.Previous != null && !r.Previous.Confirmed && Mcv.VotersOf(r.Previous).Any(i => i.Account == g) && !r.Previous.VotesOfTry.Any(i => i.Generator == g))
 					{
 						r = r.Previous;
 
 						var b = createvote(r);
 								
 						b.Sign(Vault.GetKey(g));
 						votes.Add(b);
 					}

					if(IncomingTransactions.Any(i => i.Placing == PlacingStage.Accepted) || Mcv.Tail.Any(i => Mcv.LastConfirmedRound.Id < i.Id && i.Payloads.Any()))
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

					for(int i = Mcv.LastConfirmedRound.Id + 1; i <= Mcv.LastNonEmptyRound.Id; i++) /// better to start from votes.Min(i => i.Id) or last excuted
					{
						var r = Mcv.GetRound(i);
						
						if(r.Hash == null)
						{
							r.ConsensusTime = r.Previous.ConsensusTime;
							r.ConsensusExeunitFee = r.Previous.ConsensusExeunitFee;
						}

						if(!r.Confirmed)
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
			//IEnumerable<Transaction>	accepted;

			Workflow.Log?.Report(this, "Transacting started");

			while(Workflow.Active)
			{
				if(!OutgoingTransactions.Any())
					WaitHandle.WaitAny(new[] {TransactingWakeup, Workflow.Cancellation.WaitHandle});

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

							//AllocateTransactionResponse at = null;
							RdcInterface rdi; 

							try
							{
								Monitor.Exit(Lock);

								rdi = getrdi(g.Key);
							}
							catch(NodeException)
							{
								Thread.Sleep(1000);
								continue;
							}
							finally
							{
								Monitor.Enter(Lock);
							}

							int nid = -1;
							var txs = new List<Transaction>();

							foreach(var t in g.Where(i => i.Placing == PlacingStage.None))
							{
								try
								{
									Monitor.Exit(Lock);

									t.Rdi = rdi;
									t.Fee = 0;
									t.Nid = 0;
									t.Expiration = 0;

									t.Sign(Vault.GetKey(t.Signer), Zone.Cryptography.ZeroHash);

									var at = rdi.Request<AllocateTransactionResponse>(new AllocateTransactionRequest {Transaction = t});
								
									if(nid == -1)
										nid = at.NextTransactionId;
									else
										nid++;

									t.Fee		 = at.MinFee;
									t.Nid		 = nid;
									t.Expiration = at.LastConfirmedRid + Mcv.TransactionPlacingLifetime;

									t.Sign(Vault.GetKey(t.Signer), at.PowHash);
									txs.Add(t);
								}
								catch(NodeException)
								{
									Thread.Sleep(1000);
									continue;
								}
								catch(EntityException)
								{
									if(t.__ExpectedPlacing == PlacingStage.FailedOrNotFound)
									{
										t.Placing = PlacingStage.FailedOrNotFound;
										OutgoingTransactions.Remove(t);
									} 
									else
										Thread.Sleep(1000);

									continue;
								}
								finally
								{
									Monitor.Enter(Lock);
								}
							}

							IEnumerable<byte[]> atxs = null;

							try
							{
								Monitor.Exit(Lock);
								atxs = rdi.SendTransactions(txs).Accepted;
							}
							catch(NodeException)
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

									Workflow.Log?.Report(this, "Operation(s) accepted", $"N={t.Operations.Length} -> {m}, {t.Rdi}");
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

					var accepted = OutgoingTransactions.Where(i => i.Placing == PlacingStage.Accepted || i.Placing == PlacingStage.Placed).ToArray();

					if(accepted.Any())
					{
						foreach(var g in accepted.GroupBy(i => i.Rdi).ToArray())
						{
							TransactionStatusResponse ts;

							try
							{
								Monitor.Exit(Lock);
								ts = g.Key.GetTransactionStatus(g.Select(i => new TransactionsAddress {Account = i.Signer, Nid = i.Nid}));
							}
							catch(NodeException)
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
										else
											OutgoingTransactions.Remove(t);
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
					TransactingThread = CreateThread(Transacting);

					TransactingThread.Name = $"{Settings.IP?.GetAddressBytes()[3]} Transacting";
					TransactingThread.Start();
				}

				//o.Placing = PlacingStage.PendingDelegation;
				OutgoingTransactions.Add(t);
				TransactingWakeup.Set();
			} 
			else
			{
				Workflow.Log?.ReportError(this, "Too many pending/unconfirmed operations");
			}
		}
		
		public Transaction Enqueue(Operation operation, AccountAddress signer, PlacingStage await, Workflow workflow)
		{
			return Enqueue(new Operation[] {operation}, signer, await, workflow)[0];
		}

 		public Transaction[] Enqueue(IEnumerable<Operation> operations, AccountAddress signer, PlacingStage await, Workflow workflow)
 		{
			var p = new List<Transaction>();

			while(operations.Any())
			{
				var t = new Transaction();
				t.Zone = Zone;
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

			}
				Thread.Sleep(100);

			end:
				workflow.Log?.Report(this, $"Transaction is {t.Placing}", t.ToString());
		}

		public Peer ChooseBestPeer(Role role, HashSet<Peer> exclusions)
		{
			return Peers.Where(i => i.GetRank(role) > 0 && (exclusions == null || !exclusions.Contains(i)))
						.OrderByDescending(i => i.Status == ConnectionStatus.OK)
						//.ThenBy(i => i.GetRank(role))
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
				catch(NodeException)
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
					catch(NodeException)
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
					{
						workflow.ThrowIfAborted();
						Thread.Sleep(0);
					}
					
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
						throw new NodeException(NodeError.Connectivity);

				if(!SunGlobals.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new NodeException(NodeError.Timeout);
				
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
 				catch(NodeException)
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
								catch(EntityException ex) when (ex.Error == EntityError.NotFound)
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
									catch(EntityException ex) when (ex.Error == EntityError.NotFound)
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
				catch(NodeException)
				{
				}
			}
		}

		//public QueryResourceResponse QueryResource(string query, Workflow workflow) => Call(c => c.QueryResource(query), workflow);
		//public ResourceResponse FindResource(ResourceAddress query, Workflow workflow) => Call(c => c.FindResource(query), workflow);
	}
}
