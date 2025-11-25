using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Uccs.Net;

public delegate void VoidDelegate();

public class Statistics
{
	public PerformanceMeter Consensing = new();
	public PerformanceMeter Generating = new();
	public PerformanceMeter Transacting = new();
	public PerformanceMeter Verifying = new();
	public PerformanceMeter Declaring = new();
	public PerformanceMeter Sending = new();
	public PerformanceMeter Reading = new();

	public int AcceptedVotes;
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

public enum PpcClass : byte
{
	None = 0, 
	_Last = 99
}

public abstract class TcpPeering : IPeer
{
	public const int							Timeout = 5000;

	public System.Version						Version => Assembly.GetAssembly(GetType()).GetName().Version;
	public static readonly int[]				Versions = {5};

	public IProgram								Program;
	public string								Name;
	public PeeringSettings						Settings;
	public Flow									Flow;
	public object								Lock = new ();

	public IPAddress							IP = IPAddress.None;
	public List<IPAddress>						IgnoredIPs = new();
	public bool									IsPeering => MainThread != null;
	public bool									IsListener => ListeningThread != null;

	public Statistics							Statistics = new();

	public List<TcpClient>						IncomingConnections = new();
	protected abstract IEnumerable<Peer>		PeersToDisconnect { get; }

	public TcpListener							Listener;
	protected Thread							MainThread;
	protected Thread							ListeningThread;
	public AutoResetEvent						MainWakeup = new AutoResetEvent(true);

	public Constructor							Constructor = new();

	protected abstract void						ProcessConnectivity();
	protected abstract Hello					CreateOutboundHello(Peer peer, bool permanent);
	protected abstract Hello					CreateInboundHello(IPAddress ip, Hello inbound);
	protected abstract bool						Consider(TcpClient client);
	protected abstract bool						Consider(bool inbound, Hello hello, Peer peer);
	protected abstract void						AddPeer(Peer peer);
	protected abstract void						RemovePeer(Peer peer);
	protected abstract Peer						FindPeer(IPAddress ip);
	protected virtual void						OnConnected(Peer peer) {}
	public virtual void							OnRequestException(Peer peer, NodeException ex){}


	public TcpPeering(IProgram program, string name, PeeringSettings settings, Flow flow)
	{
		Program = program;
		Name = name;
		Settings = settings;
		Flow = flow;

		Flow.Log?.Report(this, $"Version: {Version}");
		Flow.Log?.Report(this, $"Runtime: {Environment.Version}");
		Flow.Log?.Report(this, $"Protocols: {string.Join(',', Versions)}");

		Constructor.Register<CodeException>(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public virtual void Run()
	{
		if(Settings.IP != null)
		{
			ListeningThread = Program.CreateThread(Listening);
			ListeningThread.Name = $"{Name} Listening";
			ListeningThread.Start();
		}

		MainThread = Program.CreateThread(() => {
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

	public virtual void Stop()
	{
		Listener?.Stop();

		lock(Lock)
		{
			foreach(var i in IncomingConnections)
				i.Close();

			foreach(var i in PeersToDisconnect.Where(i => i.Status != ConnectionStatus.Disconnected).ToArray())
				i.Disconnect();
		}

		MainThread?.Join();
		ListeningThread?.Join();
	}

	public override string ToString()
	{
		return string.Join(",", new string[] {	Name,
												Settings.IP != null ? IP.ToString() : null}.Where(i => !string.IsNullOrWhiteSpace(i)));
	}

	protected void Listening()
	{
		try
		{
			Flow.Log?.Report(this, $"Listening {Settings.IP}:{Settings.Port}");

			Listener = new TcpListener(Settings.IP, Settings.Port);
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
					if(Consider(c))
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
				tcp = Settings.IP != null ? new TcpClient(new IPEndPoint(Settings.IP, 0)) : new TcpClient();

				tcp.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;
				//client.ReceiveTimeout = Timeout;
				tcp.Connect(peer.IP, peer.Port);
			}
			catch(SocketException ex) 
			{
				Flow.Log?.ReportError(this, $"To {peer.IP}. {ex.Message}" );
				goto failed;
			}

			Hello h = null;
								
			try
			{
				tcp.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;
				tcp.ReceiveTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;

				Peer.SendHello(tcp, CreateOutboundHello(peer, permanent));
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

				if(Consider(false, h, peer) == false)
					goto failed;
												
				if(IP.Equals(IPAddress.None))
				{
					IP = h.IP;
					Flow.Log?.Report(this, $"Reported IP {IP}");
				}


				peer.Start(this, tcp, h, false);

				OnConnected(peer);
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
		
		var t = Program.CreateThread(f);
		t.Name = Settings.IP?.GetAddressBytes()[3] + " -> out -> " + peer.IP.GetAddressBytes()[3];
		t.Start();
					
	}

	private void InboundConnect(TcpClient client)
	{
		var ip = (client.Client.RemoteEndPoint as IPEndPoint).Address.MapToIPv4();
		var peer = FindPeer(ip);

		if(ip.Equals(IP))
		{
			IgnoredIPs.Add(ip);
			
			if(peer != null)
				RemovePeer(peer);
			
			client.Close();
			return;
		}

		if(IgnoredIPs.Contains(ip))
		{
			if(peer != null)
				RemovePeer(peer);

			client.Close();
			return;
		}

		if(peer != null)
		{
			if(peer.Status != ConnectionStatus.Disconnected)
			{
				client.Close();
				return;
			}
		}

		IncomingConnections.Add(client);

		var t = Program.CreateThread(incon);
		t.Name = Settings.IP?.GetAddressBytes()[3] + " <- in <- " + ip.GetAddressBytes()[3];
		t.Start();

		void incon()
		{
			Hello h = null;

			try
			{
				client.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;
				client.ReceiveTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;

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
					goto failed;

				if(peer == null)
					peer = new Peer(ip, Settings.Port);

				if(Consider(true, h, peer) == false)
					goto failed;

				if(IP.Equals(IPAddress.None))
				{
					IP = h.IP;
					Flow.Log?.Report(this, $"Reported IP {IP}");
				}
	
				try
				{
					Peer.SendHello(client, CreateInboundHello(ip, h));
				}
				catch(Exception ex) when(!NodeGlobals.ThrowOnCorrupted)
				{
					Flow.Log?.ReportError(this, $"From {ip}. SendHello -> {ex.Message}");
					goto failed;
				}

				if(FindPeer(ip) == null)
					AddPeer(peer);

				peer.Permanent = h.Permanent;
				peer.Start(this, client, h, true);

				OnConnected(peer);

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

			if(!NodeGlobals.InfiniteTimeouts)
				if(DateTime.Now - t > TimeSpan.FromMilliseconds(Timeout))
					throw new NodeException(NodeError.Timeout);
			
			Thread.Sleep(1);
		}
	}

	public override Return Call(PeerRequest request)
	{
		var rq = request;

		if(request.Peer == null) /// self call, cloning needed
		{
			var s = new MemoryStream();
			BinarySerializator.Serialize(new(s), request, Constructor.TypeToCode);
			s.Position = 0;
			
			rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constructor.Construct);
			rq.Peering = this;
		}

		return rq.Execute();
	}

	public override void Send(PeerRequest request)
	{
		var rq = request;

		if(rq.Peer == null) /// self call, cloning needed
		{
			var s = new MemoryStream();
			BinarySerializator.Serialize(new(s), rq, Constructor.TypeToCode);
			s.Position = 0;
			
			rq = BinarySerializator.Deserialize<PeerRequest>(new(s), Constructor.Construct);
			rq.Peering = this;
		}

		rq.Execute();
	}
}
