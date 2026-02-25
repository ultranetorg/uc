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

public abstract class Peering
{
	public IProgram			Program;
	public string			Name;
	public object			Lock = new ();
	public Constructor		Constructor = new();
	public Flow				Flow;
	public Statistics		Statistics = new();
	
	public virtual void		OnRequestException(Peer peer, NodeException ex){}
}

public abstract class TcpPeering<P> : Peering where P : Peer
{
	public const int							Timeout = 5000;

	public System.Version						Version => Assembly.GetAssembly(GetType()).GetName().Version;
	public static readonly int[]				Versions = {5};

	public PeeringSettings						Settings;

	public Endpoint								EP;
	public List<IPAddress>						IgnoredIPs = new();
	public bool									IsPeering => MainThread != null;
	public bool									IsListener => ListeningThread != null;


	public List<TcpClient>						IncomingConnections = new();
	protected abstract IEnumerable<P>			PeersToDisconnect { get; }

	public TcpListener							Listener;
	protected Thread							MainThread;
	protected Thread							ListeningThread;
	public AutoResetEvent						MainWakeup = new AutoResetEvent(true);


	protected abstract void						ProcessMain();
	protected abstract Hello					CreateOutboundHello(P peer, bool permanent);
	protected abstract Hello					CreateInboundHello(IPAddress ip, Hello inbound);
	protected abstract bool						Consider(TcpClient client);
	protected abstract bool						Consider(bool inbound, Hello hello, P peer);
	protected abstract void						AddPeer(P peer);
	protected abstract void						RemovePeer(P peer);
	protected abstract P						FindPeer(Endpoint ip);
	protected virtual void						OnConnected(P peer) {}

	protected abstract P						CreatePeer();

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
		if(Settings.EP != null)
		{
			ListeningThread = Program.CreateThread(Listening);
			ListeningThread.Name = $"{Name} Listening";
			ListeningThread.Start();
		}

		MainThread = Program.CreateThread(() => {
											 		while(Flow.Active)
											 		{
											 			var r = WaitHandle.WaitAny([MainWakeup, Flow.Cancellation.WaitHandle], 500);
											 
											 			//lock(Lock)
											 			//{
											 				ProcessMain();
											 			//}
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
												Settings.EP?.ToString()}.Where(i => !string.IsNullOrWhiteSpace(i)));
	}

	protected void Listening()
	{
		try
		{
			Flow.Log?.Report(this, $"Listening {Settings.EP}");

			Listener = new TcpListener(Settings.EP.IP, Settings.EP.Port);
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

	protected void OutboundConnect(P peer, bool permanent)
	{
		peer.Status = ConnectionStatus.Initiated;
		peer.Permanent = permanent;
		peer.LastTry = DateTime.UtcNow;
		peer.Retries++;

		TcpClient tcp = null;
		
		Task.Run(() =>	{
							try
							{
								tcp = Settings.EP != null ? new TcpClient(new IPEndPoint(Settings.EP.IP, 0)) : new TcpClient();

								tcp.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : Timeout;
								//client.ReceiveTimeout = Timeout;
								tcp.Connect(peer.EP.IP, peer.EP.Port);
							}
							catch(SocketException ex) 
							{
								Flow.Log?.ReportError(this, $"To {peer.EP}. {ex.Message}" );
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
								Flow.Log?.ReportError(this, $"To {peer.EP}. {ex.Message}" );
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
												
								if(EP == null)
								{
									EP = new Endpoint(h.YourIP, Settings.EP.Port);
									Flow.Log?.Report(this, $"Reported IP {EP}");
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
						});
	
	}

	private void InboundConnect(TcpClient client)
	{
		var ip = (client.Client.RemoteEndPoint as IPEndPoint).Address.MapToIPv4();

///		if(ip.Equals(IP))
///		{
///			IgnoredIPs.Add(ip);
///			
///			if(peer != null)
///				RemovePeer(peer);
///			
///			client.Close();
///			return;
///		}

		if(IgnoredIPs.Contains(ip))
		{
			///var peer = FindPeer(ip);
			///
			///if(peer != null)
			///	RemovePeer(peer);

			client.Close();
			return;
		}

		P peer = null;

		IncomingConnections.Add(client);

		Task.Run(() =>	{
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

								var ep = new Endpoint(ip, h.MyPort);
								peer = FindPeer(ep);

								if(peer != null)
								{
									if(peer.Status != ConnectionStatus.Disconnected)
									{
										client.Close();
										return;
									}
								}

								if(peer == null)
								{	
									peer = CreatePeer();
									peer.EP = ep;
								}

								if(Consider(true, h, peer) == false)
									goto failed;

								if(EP == null)
								{
									EP = new Endpoint(h.YourIP, Settings.EP.Port);
									Flow.Log?.Report(this, $"Reported IP {EP}");
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

								if(FindPeer(peer.EP) == null)
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

						});
	}

	public void Connect(P peer, Flow workflow)
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
}
