using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

public enum PacketType : byte
{
	None, Request, Response
}

public enum ConnectionStatus
{
	None, Disconnected, Initiated, OK, Disconnecting
}

public class Peer : IPeer, IBinarySerializable
{
	public IPAddress								IP {get; set;} 
	public string									Name;
	public string									Net;
	public ushort									Port;

	public ConnectionStatus							Status = ConnectionStatus.Disconnected;

	public bool										Forced;
	public bool										Permanent;
	public bool										Recent;
	int												IdCounter = 0;
	public DateTime									LastSeen = DateTime.MinValue;
	public DateTime									LastTry = DateTime.MinValue;
	public int										Retries;

	public bool										Inbound;
	public string									StatusDescription => Status == ConnectionStatus.OK ? (Inbound ? "Incoming" : "Outbound") : Status.ToString();

	//public Role										Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0) | (SeedRank > 0 ? Role.Seed : 0);
	public int										PeerRank = 0;
	public long										Roles;

	public Dictionary<Role, DateTime>				LastFailure = new();

	public TcpPeering								Peering;
	TcpClient										Tcp;
	NetworkStream									Stream;
	BinaryWriter									Writer;
	BinaryReader									Reader;
	Thread											ListenThread;
	Thread											SendThread;
	Queue<Packet>									Outs = new();
	public List<PeerRequest>						InRequests = new();
	List<PeerRequest>								OutRequests = new();
	AutoResetEvent									SendSignal = new AutoResetEvent(true);

	public Peer()
	{
	}

	public Peer(IPAddress ip, ushort port)
	{
		IP = ip;
		Port = port;
	}

	public override string ToString()
	{
		return $"{Name}, {IP}, {StatusDescription}, Permanent={Permanent}, Roles={Roles}, Forced={Forced}";
	}
 		
	public static bool operator == (Peer a, Peer b)
	{
		return a is null && b is null || a is not null && a.Equals(b);
	}

	public static bool operator != (Peer a, Peer b)
	{
		return !(a == b);
	}

	public override bool Equals(object o)
	{
		return o is Peer a && Equals(a);  
	}

	public bool Equals(Peer a)
	{
		return a is not null && IP.Equals(a.IP);
	}

	public override int GetHashCode()
	{
		return IP.GetHashCode();
	}

	public void SaveNode(BinaryWriter writer)
	{
		//writer.WriteUtf8(Net);
		writer.Write(Port);
		writer.Write7BitEncodedInt64(Roles);
		writer.Write7BitEncodedInt64(LastSeen.ToBinary());
		writer.Write(PeerRank);
	}

	public void LoadNode(BinaryReader reader)
	{
		//Net = reader.ReadUtf8();
		Port = reader.ReadUInt16();
		Roles = reader.Read7BitEncodedInt64();
		LastSeen = DateTime.FromBinary(reader.Read7BitEncodedInt64());
		PeerRank = reader.ReadInt32();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(IP);
		writer.Write(Port);
		writer.Write7BitEncodedInt64(Roles);
	}

	public void Read(BinaryReader reader)
	{
		IP = reader.ReadIPAddress();
		Port = reader.ReadUInt16();
		Roles = reader.Read7BitEncodedInt64();
	}

	public static void SendHello(TcpClient client, Hello h)
	{
		var w = new BinaryWriter(client.GetStream());

		h.Write(w);
	}

	public static Hello WaitHello(TcpClient client)
	{
		var r = new BinaryReader(client.GetStream());

		var h = new Hello();

		h.Read(r);
		
		return h;
	}

	public void Disconnect()
	{
		if(Status != ConnectionStatus.Disconnecting)
			Status = ConnectionStatus.Disconnecting;
		else
			return;

		Forced = false;
		Permanent = false;
		Recent = false;
		Retries = 0;
		IdCounter = 0;
		Inbound = false;

		lock(InRequests)
			InRequests.Clear();

		lock(OutRequests)
		{
			foreach(var i in OutRequests)
			{
				if(!i.Event.SafeWaitHandle.IsClosed)
				{
					i.Event.Set();
					i.Event.Close();
				}
			}

			OutRequests.Clear();
		}

		lock(Outs)
		{
			foreach(var i in Outs.OfType<PeerRequest>())
			{
				if(i.Event != null && !i.Event.SafeWaitHandle.IsClosed)
				{
					i.Event.Set();
					i.Event.Close();
				}
			}

			Outs.Clear();
		}

		if(Tcp != null)
		{
			Stream.Close();
			Tcp.Close();
			Tcp = null;

			SendSignal.Set();
		}

		if(SendThread == null && ListenThread == null)
		{
			Status = ConnectionStatus.Disconnected;
		}
	}

	public void Start(TcpPeering peering, TcpClient client, Hello h, bool inbound)
	{
		Peering = peering;
		Tcp = client;
		
		Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
		Tcp.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : HomoTcpPeering.Timeout;

		PeerRank++;
		Name		= h.Name;
		Forced		= false;
		Status		= ConnectionStatus.OK;
		Inbound		= inbound;
		Stream		= client.GetStream();
		Writer		= new BinaryWriter(Stream);
		Reader		= new BinaryReader(Stream);
		LastSeen	= DateTime.UtcNow;
		Roles		= h.Roles;

		ListenThread = Peering.Node.CreateThread(Listening);
		ListenThread.Name = $"{Peering.Node.Name} <- {h.Name}";
		ListenThread.Start();

		SendThread = Peering.Node.CreateThread(Sending);
		SendThread.Name = $"{Peering.Node.Name} -> {h.Name}";
		SendThread.Start();
	}

	void Sending()
	{
		try
		{
			while(Peering.Flow.Active && Status == ConnectionStatus.OK)
			{
				Peering.Statistics.Sending.Begin();

				PeerRequest[] inrq;

				lock(InRequests)
				{
					inrq = InRequests.ToArray();
					InRequests.Clear();
				}
						
				foreach(var i in inrq)
				{
// 						if(i is ProxyRequest px)
// 						{
// 							Task.Run(() =>	{
// 												var rp = i.SafeExecute(Sun);
// 
// 												if(i.WaitResponse)
// 													lock(Outs)
// 														Outs.Enqueue(rp);
// 
// 												SendSignal.Set();
// 											});
// 						}
// 						else
					{
						var rp = i.SafeExecute();

						if(i.WaitResponse)
							lock(Outs)
								Outs.Enqueue(rp);
					}
				}

				lock(Outs)
				{
					foreach(var i in Outs)
					{
						if(i is PeerRequest)
							Writer.Write((byte)PacketType.Request);
						else if(i is PeerResponse)
							Writer.Write((byte)PacketType.Response);
						else
							throw new IntegrityException("Wrong packet to write");

						BinarySerializator.Serialize(Writer, i, Peering.TypeToCode);
					}
					
					Outs.Clear();
				}

				Peering.Statistics.Sending.End();
				
				WaitHandle.WaitAny([SendSignal, Peering.Flow.Cancellation.WaitHandle]);
			}
		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();
		}

		//lock(Sun.Lock)
		{
			SendThread = null;

			if(Status == ConnectionStatus.Disconnecting && SendThread == null && ListenThread == null)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	void Listening()
	{
 		try
 		{
			while(Peering.Flow.Active && Status == ConnectionStatus.OK)
			{
				var pk = (PacketType)Reader.ReadByte();

				if(Peering.Flow.Aborted || Status != ConnectionStatus.OK)
					return;
				
				Peering.Statistics.Reading.Begin();

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var rq = BinarySerializator.Deserialize<PeerRequest>(Reader, Peering.Constract);
						rq.Peer = this;
						rq.Peering = Peering;

						lock(InRequests)
 								InRequests.Add(rq);

						#if DEBUG
						if(InRequests.Count > 100)
						{
							///Debugger.Break();
						}
						#endif
 	
						SendSignal.Set();

 						break;
 					}

					case PacketType.Response:
 						{
						var rp = BinarySerializator.Deserialize<PeerResponse>(Reader, Peering.Constract);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == rp.Id);

							if(rq != null)
							{
								rp.Peer = this;
								rq.Response = rp;
								rq.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

						break;
					}
				}

				Peering.Statistics.Reading.End();
			}
 		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();
		}

		//lock(Sun.Lock)
		{
			ListenThread = null;

			if(Status == ConnectionStatus.Disconnecting && SendThread == null && ListenThread == null)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	public override void Post(PeerRequest rq)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		rq.Id = IdCounter++;

		lock(Outs)
			Outs.Enqueue(rq);

		SendSignal.Set();
	}

	public override PeerResponse Send(PeerRequest rq)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		rq.Id = IdCounter++;

		if(rq.WaitResponse)
			lock(OutRequests)
			{
				rq.Event = new ManualResetEvent(false);
				OutRequests.Add(rq);
			}

		lock(Outs)
			Outs.Enqueue(rq);

		SendSignal.Set();

		if(rq.WaitResponse)
		{
			int i = -1;

			try
			{
				i = WaitHandle.WaitAny([rq.Event, Peering.Flow.Cancellation.WaitHandle], NodeGlobals.DisableTimeouts ? Timeout.Infinite : 60 * 1000);
			}
			catch(ObjectDisposedException)
			{
				throw new OperationCanceledException();
			}
			finally
			{
				rq.Event.Close();
			}

			if(i == 0)
			{
				if(rq.Response == null)
					throw new NodeException(NodeError.Connectivity);

				if(rq.Response.Error == null)
				{
					return rq.Response;
				}
				else
				{
					if(rq.Response.Error is NodeException e)
					{
						Peering.OnRequestException(this, e);
					}

					throw rq.Response.Error;
				}
			}
			else if(i == 1)
				throw new OperationCanceledException();
			else
				throw new NodeException(NodeError.Timeout);
		}
		else
			return null;
	}
}