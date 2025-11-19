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
	public IPAddress			IP {get; set;} 
	public string				Name;
	public string				Net;
	public ushort				Port;

	public ConnectionStatus		Status = ConnectionStatus.Disconnected;

	public bool					Forced;
	public bool					Permanent;
	public bool					Recent;
	int							IdCounter = 0;
	public DateTime				LastSeen = DateTime.MinValue;
	public DateTime				LastTry = DateTime.MinValue;
	public int					Retries;

	public bool					Inbound;
	public string				StatusDescription => Status == ConnectionStatus.OK ? (Inbound ? "Incoming" : "Outbound") : Status.ToString();

	public int					PeerRank = 0;
	public long					Roles;

	public TcpPeering			Peering;
	TcpClient					Tcp;
	NetworkStream				Stream;
	BinaryWriter				Writer;
	BinaryReader				Reader;
	Thread						ListenThread;
	List<PeerRequest>			OutRequests = new();

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

		lock(OutRequests)
		{
			foreach(var i in OutRequests.OfType<FuncPeerRequest>())
			{
				if(!i.Event.SafeWaitHandle.IsClosed)
				{
					i.Event.Set();
					i.Event.Close();
				}
			}

			OutRequests.Clear();
		}

		if(Tcp != null)
		{
			Stream.Close();
			Tcp.Close();
			Tcp = null;
		}

		if(ListenThread == null)
		{
			Status = ConnectionStatus.Disconnected;
		}
	}

	public void Start(TcpPeering peering, TcpClient client, Hello h, bool inbound)
	{
		Peering = peering;
		Tcp = client;
		
		Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
		Tcp.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : TcpPeering.Timeout;

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

		ListenThread = Peering.Program.CreateThread(Listening);
		ListenThread.Name = $"{Peering.Name} <- {h.Name}";
		ListenThread.Start();
	}

	void Request(PeerRequest i)
	{
		try
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Request);
				BinarySerializator.Serialize(Writer, i, Peering.Constructor.TypeToCode); 
			}
		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();

			throw new OperationCanceledException();
		}
	}

	void Respond(PeerRequest i)
	{
		try
		{
			if(i is FuncPeerRequest f)
			{
				var rp = f.SafeExecute();
							
				lock(Writer)
				{
					Writer.Write((byte)PacketType.Response);
					BinarySerializator.Serialize(Writer, rp, Peering.Constructor.TypeToCode); 
				}
			}
			else
				(i as ProcPeerRequest).SafeExecute();
		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();

			throw new OperationCanceledException();
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
						var rq = BinarySerializator.Deserialize<PeerRequest>(Reader, Peering.Constructor.Constract);
						rq.Peer = this;
						rq.Peering = Peering;
						
						Respond(rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var rp = BinarySerializator.Deserialize<PeerResponse>(Reader, Peering.Constructor.Constract);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == rp.Id);

							if(rq is FuncPeerRequest f)
							{
								rp.Peer = this;
								f.Response = rp;
								f.Event.Set();
 									
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

			if(Status == ConnectionStatus.Disconnecting && ListenThread == null)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	public override void Post(ProcPeerRequest rq)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		rq.Id = IdCounter++;

		Request(rq);
	}

	public override PeerResponse Send(FuncPeerRequest rq)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		rq.Id = IdCounter++;

		lock(OutRequests)
		{
			rq.Event = new ManualResetEvent(false);
			OutRequests.Add(rq);
		}

		Request(rq);

		int i = -1;

		try
		{
			i = WaitHandle.WaitAny([rq.Event, Peering.Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
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
}