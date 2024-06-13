using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void VoidDelegate();
 	public delegate void SunDelegate(Node d);

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
	public enum Role : long
	{
		None,
		Base		= 0b00000001,
		Chain		= 0b00000010,
	}

	public class Node : IPeer
	{
		public System.Version			Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]	Versions = {4};
		public const string				FailureExt = "failure";
		public const int				Timeout = 5000;

		public Zone						Zone;
		public NodeSettings				Settings;
		public Flow						Flow;
		public Vault					Vault;
		public List<Mcv>				Mcvs = [];
		public object					Lock = new();
		//public Clock					Clock;
		public Mcv						FindMcv(Guid id) => Mcvs.Find(i => i.Guid == id);
		public T						Find<T>() where T : Mcv => Mcvs.Find(i => i.GetType() == typeof(T)) as T;

		public RocksDb					Database;
		public ColumnFamilyHandle		PeersFamily => Database.GetColumnFamily(nameof(Peers));
		readonly DbOptions				DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																			.SetCreateMissingColumnFamilies(true);

		public Guid						PeerId;
		public IPAddress				IP = IPAddress.None;
		public List<IPAddress>			IgnoredIPs = new();
		public bool						IsPeering => MainThread != null;
		public bool						IsListener => ListeningThread != null;
		public List<Peer>				Peers = new();
		public IEnumerable<Peer>		Connections(Mcv mcv) => Peers.Where(i => (mcv == null || i.Ranks.ContainsKey(mcv.Guid)) && i.Status == ConnectionStatus.OK);
		public IEnumerable<Peer>		Bases(Mcv mcv) => Connections(mcv).Where(i => i.Permanent && i.GetRank(mcv.Guid, (long)Role.Base) > 0);

		public Statistics				PrevStatistics = new();
		public Statistics				Statistics = new();

		public List<TcpClient>			IncomingConnections = new();

		TcpListener						Listener;
		public Thread					MainThread;
		Thread							ListeningThread;
		public AutoResetEvent			MainWakeup = new AutoResetEvent(true);
		
		public SunDelegate				Stopped;
		
		//public IGasAsker				GasAsker; 
		//public IFeeAsker				FeeAsker;

		public SunDelegate				MainStarted;
		public SunDelegate				ApiStarted;

		public static List<Node>		All = new();
		
		public string					Name;

		public Node(string name, NodeSettings settings, Zone zone, Vault vault,  Flow workflow)
		{
			Name = name;
			Settings = settings;
			Zone = zone;
			Vault = vault;
			Directory.CreateDirectory(Settings.Profile);

			Flow = workflow ?? new Flow(Name, new Log());

			if(Flow.Log != null)
			{
				Flow.Log.Reported += m => File.AppendAllText(Path.Combine(Settings.Profile, Name + ".log"), m.ToString() + Environment.NewLine);
			}

			var fs = new ColumnFamilies();

			foreach(var i in new ColumnFamilies.Descriptor[] { new(nameof(Peers), new()) })
				fs.Add(i);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, nameof(Node)), fs);

			if(PeerId != Guid.Empty)
				throw new NodeException(NodeError.AlreadyRunning);

			Flow.Log?.Report(this, $"Ultranet Node {Version}");
			Flow.Log?.Report(this, $"Runtime: {Environment.Version}");
			Flow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Flow.Log?.Report(this, $"Zone: {Zone.Name}");
			Flow.Log?.Report(this, $"Profile: {Settings.Profile}");

			if(NodeGlobals.Any)
				Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");
		}

		public void RunPeer()
		{
			PeerId = Guid.NewGuid();

			LoadPeers();

			if(Settings.IP != null)
			{
				ListeningThread = CreateThread(Listening);
				ListeningThread.Name = $"{Name} Listening";
				ListeningThread.Start();
			}

			MainThread = CreateThread(() =>	{
												while(Flow.Active)
												{
													var r = WaitHandle.WaitAny([MainWakeup, Flow.Cancellation.WaitHandle], 500);

													lock(Lock)
													{
														ProcessConnectivity();
													}
												}
											});

			MainThread.Name = $"{Name} Main";
			MainThread.Start();
			MainStarted?.Invoke(this);
		}

		public override string ToString()
		{
			return string.Join(",", new string[] {Settings.IP != null ? IP.ToString() : null}.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

		public object Constract(Type t, byte b)
		{
			if(t == typeof(Transaction))	return new Transaction {Zone = Zone};
			if(t == typeof(Manifest))		return new Manifest();
			if(t == typeof(PeerRequest))	return PeerRequest.FromType((PeerCallClass)b); 
			if(t == typeof(PeerResponse))	return PeerResponse.FromType((PeerCallClass)b); 
			if(t == typeof(Operation))		return Operation.FromType((OperationClass)b); 
			if(t == typeof(NetException))	return NetException.FromType((ExceptionClass)b); 
			if(t == typeof(Urr))			return Urr.FromType(b); 

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
											if(Flow.Active)
												Abort(ex);
										}
									});
		}

		public void Abort(Exception ex)
		{
			lock(Lock)
			{
				File.WriteAllText(Path.Join(Settings.Profile, "Abort." + FailureExt), ex.ToString());
				Flow.Log?.ReportError(this, "Abort", ex);
	
				Stop();
			}
		}

		public void Stop()
		{
			Flow.Abort();

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

			Database?.Dispose();

			Flow.Log?.Report(this, "Stopped");

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
					p.Recent = false;
	 				p.LoadNode(new BinaryReader(new MemoryStream(i.Value())));
	 				Peers.Add(p);
				}
			}
			
			if(Peers.Any())
			{
				Flow.Log?.Report(this, "PEE loaded", $"n={Peers.Count}");
			}
			else
			{
				Peers = Zone.Initials.Select(i => new Peer(i) {Recent = false, LastSeen = DateTime.MinValue}).ToList();

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
						i.Recent = true;
						
						Peers.Add(i);
						toupdate.Add(i);
					}
					else
					{
						p.Recent = true;
						
						//var old = p.Ranks.ToDictionary(i => i.Key, i => i.Value.ToDictionary());

						bool changed = false;

						foreach(var g in i.Ranks)
						{	
							if(!p.Ranks.TryGetValue(g.Key, out var ranks))
							{
								p.Ranks[g.Key] = ranks = new ();
								changed = true;
							}

							foreach(var r in g.Value)
								if(!ranks.TryGetValue(r.Key, out var rank) || rank == 0)
								{
									ranks[r.Key] = 1;
									changed = true;
								}
						}

						if(changed)
						{
							toupdate.Add(p);
						}
					}
				}
	
				UpdatePeers(toupdate);

				return toupdate;
			}
		}

		public void Connect(Mcv m)
		{
			Mcvs.Add(m);

			foreach(var c in Connections(null))
			{
				c.Post(new PeersBroadcastRequest {Peers = [new Peer {	IP = Settings.IP,
																		Ranks = Mcvs.ToDictionary(i => i.Guid,
																								  i => Enumerable.Range(0, 64).Select(j => 1L << j).Where(j => j.IsSet(i.Settings.Roles)).ToDictionary(	i => i,
																																																		i => (byte)1))}] });
			}
		}

		public void Disconnect(Mcv m)
		{
			Mcvs.Remove(m);
		}

		void ProcessConnectivity()
		{
			var broad = false;

			var needed = Settings.Peering.PermanentMin - Peers.Count(i => i.Permanent && i.Status != ConnectionStatus.Disconnected);
		
			foreach(var p in Peers	.Where(m =>	m.Status == ConnectionStatus.Disconnected &&
												DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
									.OrderBy(i => i.Retries)
									.ThenBy(i => Settings.Peering.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
									.Take(needed))
			{
				OutboundConnect(p, true);
			}

			foreach(var p in Peers.Where(i => i.Forced && i.Status == ConnectionStatus.Disconnected))
			{
				OutboundConnect(p, false);
			}

			foreach(var i in Mcvs)
			{
				needed = i.Settings.Peering.PermanentBaseMin - Bases(i).Count();

				foreach(var p in Peers	.Where(m =>	m.GetRank(i.Guid, (long)Role.Base) > 0 &&
													m.Status == ConnectionStatus.Disconnected &&
													DateTime.UtcNow - m.LastTry > TimeSpan.FromSeconds(5))
										.OrderBy(i => i.Retries)
										.ThenBy(i => Settings.Peering.InitialRandomization ? Guid.NewGuid() : Guid.Empty)
										.Take(needed))
				{
					OutboundConnect(p, true);
				}

				if(i.ProcessConnectivity())
				{
					broad = true;
				}
			}

			if(broad)
			{
				foreach(var c in Connections(null))
				{
					Post(new PeersBroadcastRequest {Peers = Connections(null).Where(i => i != c).ToArray()});
				}
			}
		}

		void Listening()
		{
			try
			{
				Flow.Log?.Report(this, $"Listening starting {Settings.IP}:{Zone.Port}");

				Listener = new TcpListener(Settings.IP, Zone.Port);
				Listener.Start();
	
				while(Flow.Active)
				{
					var c = Listener.AcceptTcpClient();

					if(Flow.Aborted)
					{
						c.Close();
						return;
					}
	
					lock(Lock)
					{
						if(Peers.Count(i => i.Status == ConnectionStatus.OK && i.Inbound) <= Settings.Peering.InboundMax)
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
			lock(Lock)
			{
				var h = new Hello();

				h.Roles			= Mcvs.ToDictionary(i => i.Guid, i => i.Settings.Roles);
				h.Versions		= Versions;
				h.Zone			= Zone.Name;
				h.IP			= ip;
				h.Name			= Name;
				h.PeerId		= PeerId;
				h.Peers			= Peers.Where(i => i.Recent).ToArray();
				h.Permanent		= permanent;
				//h.Generators	= Members;
			
				return h;
			}
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

					tcp.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;
					//client.ReceiveTimeout = Timeout;
					tcp.Connect(peer.IP, Zone.Port);
				}
				catch(SocketException ex) 
				{
					Flow.Log?.ReportError(this, $"To {peer.IP}. {ex.Message}" );
					goto failed;
				}
	
				Hello h = null;
									
				try
				{
					tcp.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;
					tcp.ReceiveTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;

					Peer.SendHello(tcp, CreateHello(peer.IP, permanent));
					h = Peer.WaitHello(tcp);
				}
				catch(Exception ex)// when(!Settings.Dev.ThrowOnCorrupted)
				{
					Flow.Log?.ReportError(this, $"To {peer.IP}. {ex.Message}" );
					goto failed;
				}
	
				lock(Lock)
				{
					if(Flow.Aborted)
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

					if(h.PeerId == PeerId)
					{
						Flow.Log?.ReportError(this, $"To {peer.IP}. It's me" );
						IgnoredIPs.Add(peer.IP);
						Peers.Remove(peer);
						goto failed;
					}
													
					if(IP.Equals(IPAddress.None))
					{
						IP = h.IP;
						Flow.Log?.Report(this, $"Reported IP {IP}");
					}
	
					if(peer.Status == ConnectionStatus.OK)
					{
						Flow.Log?.ReportError(this, $"To {peer.IP}. Already established" );
						tcp.Close();
						return;
					}
	
					RefreshPeers(h.Peers.Append(peer));
	
					peer.Start(this, tcp, h, Name, false);
					Flow.Log?.Report(this, $"Connected to {peer}");
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

				while(Flow.Active && peer.Status != ConnectionStatus.Disconnected) 
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
					client.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;
					client.ReceiveTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;

					h = Peer.WaitHello(client);
				}
				catch(Exception ex) when(!NodeGlobals.ThrowOnCorrupted)
				{
					Flow.Log?.ReportError(this, $"From {ip}. WaitHello -> {ex.Message}");
					goto failed;
				}
				
				lock(Lock)
				{
					if(Flow.Aborted)
						return;

					if(h.Permanent)
					{
						if(Peers.Count(i => i.Status == ConnectionStatus.OK && i.Inbound && i.Permanent) + 1 > Settings.Peering.PermanentInboundMax)
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

					if(h.PeerId == PeerId)
					{
						Flow.Log?.ReportError(this, $"From {ip}. It's me");
						if(peer != null)
						{	
							IgnoredIPs.Add(peer.IP);
							Peers.Remove(peer);
						}
						goto failed;
					}

					if(peer != null && peer.Status == ConnectionStatus.OK)
					{
						Flow.Log?.ReportError(this, $"From {ip}. Already established" );
						goto failed;
					}
	
					if(IP.Equals(IPAddress.None))
					{
						IP = h.IP;
						Flow.Log?.Report(this, $"Reported IP {IP}");
					}
		
					try
					{
						Peer.SendHello(client, CreateHello(ip, false));
					}
					catch(Exception ex) when(!NodeGlobals.ThrowOnCorrupted)
					{
						Flow.Log?.ReportError(this, $"From {ip}. SendHello -> {ex.Message}");
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
					peer.Start(this, client, h, Name, true);
					Flow.Log?.Report(this, $"Connected from {peer}");
			
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

		public Peer ChooseBestPeer(Guid mcvid, long role, HashSet<Peer> exclusions)
		{
			return Peers.Where(i => i.GetRank(mcvid, role) > 0 && (exclusions == null || !exclusions.Contains(i)))
						.OrderByDescending(i => i.Status == ConnectionStatus.OK)
						//.ThenBy(i => i.GetRank(role))
						//.ThenByDescending(i => i.ReachFailures)
						.FirstOrDefault();
		}

		public Peer Connect(Guid mcvid, long role, HashSet<Peer> exclusions, Flow workflow)
		{
			Peer peer;
				
			while(workflow.Active)
			{
				Thread.Sleep(1);
	
				lock(Lock)
				{
					peer = ChooseBestPeer(mcvid, role, exclusions);
	
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

		public Peer[] Connect(Guid mcvid, long role, int n, Flow workflow)
		{
			var peers = new HashSet<Peer>();
				
			while(workflow.Active)
			{
				Peer p;
	
				lock(Lock)
					p = ChooseBestPeer(mcvid, role, peers);
	
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

		public void Connect(Peer peer, Flow workflow)
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

				if(!NodeGlobals.DisableTimeouts)
					if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
						throw new NodeException(NodeError.Timeout);
				
				Thread.Sleep(1);
			}
		}

		//public double EstimateFee(IEnumerable<Operation> operations)
		//{
		//	return Mcv != null ? operations.Sum(i => (double)i.CalculateTransactionFee(TransactionPerByteMinFee).ToDecimal()) : double.NaN;
		//}

// 		public Emission Emit(Nethereum.Web3.Accounts.Account a, BigInteger wei, AccountKey signer, Workflow workflow)
// 		{
// 						
// 			var o = new Emission(wei, eid);
// 
// 
// 			//flow?.SetOperation(o);
// 						
// 			if(FeeAsker.Ask(this, signer, o))
// 			{
// 				return o;
// 			}
// 
// 			return null;
// 		}
// 
// 		public Emission FinishEmission(AccountKey signer, Flow workflow)
// 		{
// 			lock(Lock)
// 			{
// 				var	l = Call(p =>{
// 									try
// 									{
// 										return p.Request(new AccountRequest(signer));
// 									}
// 									catch(EntityException ex) when (ex.Error == EntityError.NotFound)
// 									{
// 										return new AccountResponse();
// 									}
// 								}, 
// 								workflow);
// 			
// 				var eid = l.Account == null ? 0 : l.Account.LastEmissionId + 1;
// 
// 				var wei = Nas.FindEmission(signer, eid, workflow);
// 
// 				if(wei == 0)
// 					throw new RequirementException("No corresponding Ethrereum transaction found");
// 
// 				var o = new Emission(wei, eid);
// 
// 				if(FeeAsker.Ask(this, signer, o))
// 				{
// 					Transact(o, signer, TransactionStatus.Confirmed, workflow);
// 					return o;
// 				}
// 			}
// 
// 			return null;
// 		}

		public override PeerResponse Send(PeerRequest request)
  		{
			var rq = request;

			if(request.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), request); 
				s.Position = 0;
				
				rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constract);
				rq.Sun = this;
				rq.Mcv = request.Mcv;
				rq.McvId = request.McvId;
			}

			return rq.Execute();
 		}

		public override void Post(PeerRequest request)
  		{
			var rq = request;

			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq); 
				s.Position = 0;

				rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constract);
				rq.Sun = this;
				rq.Mcv = request.Mcv;
				rq.McvId = request.McvId;
			}

 			rq.Execute();
 		}

		public void Broadcast(Mcv mcv, Vote vote, Peer skip = null)
		{
			foreach(var i in Bases(mcv).Where(i => i != skip))
			{
				try
				{
					i.Post(new VoteRequest {Sun = this, 
											Mcv = mcv, 
											McvId = mcv.Guid, 
											Raw = vote.RawForBroadcast });
				}
				catch(NodeException)
				{
				}
			}
		}

		public static void CompareBases(string destination)
		{
			foreach(var i in All.SelectMany(i => i.Mcvs).DistinctBy(i => i.Guid))
			{
				var  d = Path.Join(destination, i.GetType().Name);

				Directory.CreateDirectory(d);

				CompareBase(i, d);
			}
		}

		public static void CompareBase(Mcv mcv, string destibation)
		{
			//Suns.GroupBy(s => s.Mcv.Accounts.SuperClusters.SelectMany(i => i.Value), Bytes.EqualityComparer);

			var jo = new JsonSerializerOptions(ApiClient.DefaultOptions);
			jo.WriteIndented = true;

			foreach(var i in All)
				Monitor.Enter(i.Lock);

			void compare(int table)
			{
				var cs = All.Where(i => i.FindMcv(mcv.Guid) != null && i.FindMcv(mcv.Guid).Settings.Base != null).Select(i => new {s = i, c = i.FindMcv(mcv.Guid).Tables[table].Clusters.OrderBy(i => i.Id, Bytes.Comparer).ToArray().AsEnumerable().GetEnumerator()}).ToArray();
	
				while(true)
				{
					var x = new bool[cs.Length];

					for(int i=0; i<cs.Length; i++)
						x[i] = cs[i].c.MoveNext();

					if(x.All(i => !i))
						break;
					else if(!x.All(i => i))
						Debugger.Break();
	
					var es = cs.Select(i => new {i.s, e = i.c.Current.BaseEntries.OrderBy(i => i.Id.Ei).ToArray().AsEnumerable().GetEnumerator()}).ToArray();
	
					while(true)
					{
						var y = new bool[es.Length];

						for(int i=0; i<es.Length; i++)
							y[i] = es[i].e.MoveNext();
	
						if(y.All(i => !i))
							break;
						else if(!y.All(i => i))
							Debugger.Break();
	
						var jes = es.Select(i => new {i.s, j = JsonSerializer.Serialize(i.e.Current, jo)}).GroupBy(i => i.j);

						if(jes.Count() > 1)
						{
							foreach(var i in jes)
							{
								File.WriteAllText(Path.Join(destibation, string.Join(',', i.Select(i => i.s.Name))), i.Key);
							}
							
							Debugger.Break();
						}
					}
				}
			}

			foreach(var t in mcv.Tables)
				compare(t.Id);

			foreach(var i in All)
				Monitor.Exit(i.Lock);
		}

		//public QueryResourceResponse QueryResource(string query, Workflow workflow) => Call(c => c.QueryResource(query), workflow);
		//public ResourceResponse FindResource(ResourceAddress query, Workflow workflow) => Call(c => c.FindResource(query), workflow);
	}
}
