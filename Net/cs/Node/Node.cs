using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void VoidDelegate();
 	public delegate void NodeDelegate(Node d);

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

	public enum PeerCallClass : byte
	{
		None = 0, 
		PeersBroadcast, 
		ShareNets,
		
		_Last = 99
	}


	public abstract class Node : IPeer
	{
		public Net									Net;
		public abstract long						Roles { get; }
		public IEnumerable<Peer>					Connections => Peers.Where(i => i.Status == ConnectionStatus.OK);

		public System.Version						Version => Assembly.GetAssembly(GetType()).GetName().Version;
		public static readonly int[]				Versions = {4};
		public const string							FailureExt = "failure";
		public const int							Timeout = 5000;

		public NodeSettings							Settings;
		public Flow									Flow;
		public object								Lock = new();
		protected JsonServer						ApiServer;

		public RocksDb								Database;
		public ColumnFamilyHandle					PeersFamily => Database.GetColumnFamily(nameof(Peers));
		readonly DbOptions							DatabaseOptions	 = new DbOptions()	.SetCreateIfMissing(true)
																						.SetCreateMissingColumnFamilies(true);

		public string								Name;
		public Guid									PeerId;
		public IPAddress							IP = IPAddress.None;
		public List<IPAddress>						IgnoredIPs = new();
		public bool									IsPeering => MainThread != null;
		public bool									IsListener => ListeningThread != null;
		public List<Peer>							Peers = new();

		public Statistics							PrevStatistics = new();
		public Statistics							Statistics = new();

		public List<TcpClient>						IncomingConnections = new();

		TcpListener									Listener;
		Thread										MainThread;
		Thread										ListeningThread;
		public AutoResetEvent						MainWakeup = new AutoResetEvent(true);

		public Dictionary<Type, byte>								Codes = [];
		public Dictionary<Type, Dictionary<byte, ConstructorInfo>>	Contructors = [];

		protected virtual void						CreateTables(ColumnFamilies columns) {}
		protected virtual void						Share(Peer peer) {}

		public Node(string name, Net net, NodeSettings settings, Flow flow)
		{
			Name = name;
			Net = net;
			Settings = settings;
			Flow = flow;

			if(Flow.Log != null)
			{
				new FileLog(Flow.Log, nameof(Node), Settings.Profile);
			}

			var cf = new ColumnFamilies();

			foreach(var i in new ColumnFamilies.Descriptor[] { new(nameof(Peers), new()) })
				cf.Add(i);

			CreateTables(cf);

			Database = RocksDb.Open(DatabaseOptions, Path.Join(Settings.Profile, "Node"), cf);

			if(PeerId != Guid.Empty)
				throw new NodeException(NodeError.AlreadyRunning);

			Flow.Log?.Report(this, $"Ultranet Node {Version}");
			Flow.Log?.Report(this, $"Runtime: {Environment.Version}");
			Flow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");
			Flow.Log?.Report(this, $"Profile: {Settings.Profile}");

			if(NodeGlobals.Any)
				Flow.Log?.ReportWarning(this, $"Dev: {NodeGlobals.AsString}");


			Contructors[typeof(PeerRequest)] = [];
			Contructors[typeof(PeerResponse)] = [];
			Contructors[typeof(NetException)] = [];
 
 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsGenericType))
 			{	
 				if(Enum.TryParse<PeerCallClass>(i.Name.Remove(i.Name.IndexOf("Request")), out var c))
 				{
 					Codes[i] = (byte)c;
 					Contructors[typeof(PeerRequest)][(byte)c] = i.GetConstructor([]);
 				}
 			}
 	 
 			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse))))
 			{	
 				if(Enum.TryParse<PeerCallClass>(i.Name.Remove(i.Name.IndexOf("Response")), out var c))
 				{
 					Codes[i] = (byte)c;
 					Contructors[typeof(PeerResponse)][(byte)c]  = i.GetConstructor([]);
 				}
 			}

			foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(NetException))))
			{
				Codes[i] = (byte)Enum.Parse<ExceptionClass>(i.Name);
				Contructors[typeof(NetException)][(byte)Enum.Parse<ExceptionClass>(i.Name)]  = i.GetConstructor([]);
			}
		}

		public virtual void RunPeer()
		{
			PeerId = Guid.NewGuid();

			LoadPeers();

			if(Settings.Peering.IP != null)
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
		}

		public override string ToString()
		{
			return string.Join(",", new string[] {	Name,
													Settings.Peering.IP != null ? IP.ToString() : null}.Where(i => !string.IsNullOrWhiteSpace(i)));
		}

// 		public virtual PeerRequest CreateRequest(int classid)
// 		{
// 			return Assembly.GetExecutingAssembly().GetType(typeof(PeerRequest).Namespace + "." + classid + "Request").GetConstructor([]).Invoke(null) as PeerRequest;
// 		}
// 
// 		public virtual PeerResponse CreateResponse(int classid)
// 		{
// 			return Assembly.GetExecutingAssembly().GetType(typeof(PeerResponse).Namespace + "." + classid + "Response").GetConstructor([]).Invoke(null) as PeerResponse;
// 		}
// 
// 		public virtual int GetRequestClass(PeerRequest request)
// 		{
// 			return (int)Enum.Parse<PeerCallClass>(request.GetType().Name.Remove(GetType().Name.IndexOf("Request")));
// 		}
// 		
// 		public virtual int GetResponseClass(PeerRequest request)
// 		{
// 			return (int)Enum.Parse<PeerCallClass>(request.GetType().Name.Remove(GetType().Name.IndexOf("Response")));
// 		}

		public virtual object Constract(Type type, byte code)
		{
			return Contructors.TryGetValue(type, out var t) && t.TryGetValue(code, out var c) ? c.Invoke(null) : null;
		}

		public virtual byte TypeToCode(Type code)
		{
			return Codes[code];
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

		public virtual void OnRequestException(Peer peer, NodeException ex)
		{
		}

		public virtual void Stop()
		{
			Flow.Abort();

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

			Database?.Dispose();
			Flow.Log?.Report(this, "Stopped");
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
				Peers = Net.Initials.Select(i => new Peer(i) {Recent = false, LastSeen = DateTime.MinValue}).ToList();

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
					else if(p.Roles != i.Roles)
					{
						p.Recent = true;

						p.Roles = i.Roles;
						toupdate.Add(p);
					}
				}
	
				UpdatePeers(toupdate);

				return toupdate;
			}
		}

		protected virtual void ProcessConnectivity()
		{
			var needed = Settings.Peering.PermanentMin - Peers.Count(i => i.Permanent && i.Status != ConnectionStatus.Disconnected);
		
			foreach(var p in Peers	.Where(p =>	p.Status == ConnectionStatus.Disconnected &&
												DateTime.UtcNow - p.LastTry > TimeSpan.FromSeconds(5))
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

		}

		void Listening()
		{
			try
			{
				Flow.Log?.Report(this, $"Listening starting {Settings.Peering.IP}:{Settings.Peering.Port}");

				Listener = new TcpListener(Settings.Peering.IP, Settings.Peering.Port);
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

				h.NetId		= Net.Id;
				h.Roles			= Roles;
				h.Versions		= Versions;
				h.IP			= ip;
				h.Name			= Name;
				h.PeerId		= PeerId;
				h.Permanent		= permanent;
			
				return h;
			}
		}

		protected void OutboundConnect(Peer peer, bool permanent)
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
					tcp = Settings.Peering.IP != null ? new TcpClient(new IPEndPoint(Settings.Peering.IP, 0)) : new TcpClient();

					tcp.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : Timeout;
					//client.ReceiveTimeout = Timeout;
					tcp.Connect(peer.IP, Net.Port);
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
						goto failed;

					if(h.NetId != Net.Id)
						goto failed;

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
	
					RefreshPeers([peer]);
	
					peer.Start(this, tcp, h, Name, false);
					peer.Post(new PeersBroadcastRequest{Peers = Peers.Where(i => i.Recent).ToArray()});

					foreach(var c in Connections.Where(i => i != peer))
						Post(new PeersBroadcastRequest {Peers = [peer]});

					Share(peer);
				}
	
				Flow.Log?.Report(this, $"Connected to {peer}");
				return;

				failed:
				{
					lock(Lock)
						peer.Disconnect();;
									
					tcp?.Close();
				}
			}
			
			var t = CreateThread(f);
			t.Name = Settings.Peering.IP?.GetAddressBytes()[3] + " -> out -> " + peer.IP.GetAddressBytes()[3];
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
			t.Name = Settings.Peering.IP?.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
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
							goto failed;
					}

					if(!h.Versions.Any(i => Versions.Contains(i)))
						goto failed;

					if(h.NetId != Net.Id)
						goto failed;

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

					RefreshPeers([peer]);
	
					peer.Permanent = h.Permanent;
					peer.Start(this, client, h, Name, true);
					peer.Post(new PeersBroadcastRequest{Peers = Peers.Where(i => i.Recent).ToArray()});
									
					foreach(var c in Connections.Where(i => i != peer))
						Post(new PeersBroadcastRequest {Peers = [peer]});

					Share(peer);

					IncomingConnections.Remove(client);
				}
	
				Flow.Log?.Report(this, $"Connected from {peer}");
				return;

			failed:
				if(peer != null)
					lock(Lock)
						peer.Disconnect();;

				client.Close();

			}
		}

		public Peer ChooseBestPeer(Guid mcvid, long role, HashSet<Peer> exclusions)
		{
			return Peers.Where(i => i.Roles.IsSet(role) && (exclusions == null || !exclusions.Contains(i)))
						.OrderByDescending(i => i.Status == ConnectionStatus.OK)
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
				BinarySerializator.Serialize(new(s), request, TypeToCode); 
				s.Position = 0;
				
				rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constract);
				rq.Node = this;
			}

			return rq.Execute();
 		}

		public override void Post(PeerRequest request)
  		{
			var rq = request;

			if(rq.Peer == null) /// self call, cloning needed
			{
				var s = new MemoryStream();
				BinarySerializator.Serialize(new(s), rq, TypeToCode); 
				s.Position = 0;

				rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constract);
				rq.Node = this;
			}

 			rq.Execute();
 		}
	}
}
