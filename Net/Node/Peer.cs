using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

public enum PacketType : byte
{
	None, Request, Response, Failure
}

public enum ConnectionStatus
{
	None, Disconnected, Initiated, OK, Disconnecting
}

public abstract class Peer : IBinarySerializable
{
	public const int				PacketLengthMaximum = 1024*1024;

	public Endpoint					EP {get; set;} 
	public long						Roles  {get; set;} 
	public string					Name;

	public ConnectionStatus			Status = ConnectionStatus.Disconnected;

	public bool						Forced;
	public bool						Permanent;
	public bool						Recent;
	public DateTime					LastSeen = DateTime.MinValue;
	public DateTime					LastTry = DateTime.MinValue;
	public int						Retries;

	public bool						Inbound;
	public string					StatusDescription => Status == ConnectionStatus.OK ? (Inbound ? "Incoming" : "Outbound") : Status.ToString();

	public int						PeerRank = 0;

	public Peering					Peering;
	protected TcpClient				Tcp;
	protected NetworkStream			Stream;
	protected Writer				Writer;
	protected Writer				PacketWriter;
	protected MemoryStream			WriteStream;
	protected Reader				Reader;
	protected byte[]				ReadBuffer;
	protected Reader				PacketReader;
	protected MemoryStream			ReadStream;
	protected Thread				ListenThread;
	protected int					IdCounter = 0;
	
	protected List<RequestPacket>	OutRequests = new();

	protected abstract void			Listening();

	public Peer()
	{
	}

//	public Peer(IPAddress ip, ushort port)
//	{
//		IP = ip;
//		Port = port;
//	}

	public override string ToString()
	{
		return $"{Name}, {EP}, {StatusDescription}, Permanent={Permanent}, Roles={Roles}, Forced={Forced}";
	}
 		
//	public static bool operator == (Peer a, Peer b)
//	{
//		return a is null && b is null || a is not null && a.Equals(b);
//	}
//
//	public static bool operator != (Peer a, Peer b)
//	{
//		return !(a == b);
//	}
//
//	public override bool Equals(object o)
//	{
//		return o is Peer a && Equals(a);  
//	}
//
//	public bool Equals(Peer a)
//	{
//		return a is not null && EP.Equals(a.EP);
//	}

	public override int GetHashCode()
	{
		return EP.GetHashCode();
	}

	public void SaveNode(Writer writer)
	{
		//writer.WriteUtf8(Net);
		writer.Write(EP);
		writer.Write7BitEncodedInt64(Roles);
		writer.Write7BitEncodedInt64(LastSeen.ToBinary());
		writer.Write(PeerRank);
	}

	public void LoadNode(Reader reader)
	{
		//Net = reader.ReadUtf8();
		EP = reader.Read<Endpoint>();
		Roles = reader.Read7BitEncodedInt64();
		LastSeen = DateTime.FromBinary(reader.Read7BitEncodedInt64());
		PeerRank = reader.ReadInt32();
	}

	public void Write(Writer writer)
	{
		writer.Write(EP);
		writer.Write7BitEncodedInt64(Roles);
	}

	public void Read(Reader reader)
	{
		EP = reader.Read<Endpoint>();
		Roles = reader.Read7BitEncodedInt64();
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
		///Writer = null; Leave for locks
		///Reader = null;
	
		lock(OutRequests)
		{
			foreach(var i in OutRequests)
			{
				if(i.Event != null && !i.Event.SafeWaitHandle.IsClosed)
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
	
		//if(ListenThread == null)
		{
			Status = ConnectionStatus.Disconnected;
		}
	}

	public void Start(Peering peering, TcpClient client, Hello h, bool inbound)
	{
		Peering = peering;
		Tcp = client;
		
		Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
		Tcp.SendTimeout = NodeGlobals.InfiniteTimeouts ? 0 : TcpPeering<Peer>.Timeout;

		PeerRank++;
		Name			= h.Name;
		Forced			= false;
		Status			= ConnectionStatus.OK;
		Inbound			= inbound;
		Stream			= client.GetStream();
		LastSeen		= DateTime.UtcNow;
		Roles			= h.Roles;
		Writer			= new Writer(Stream);
		Reader			= new Reader(Stream);
		
		WriteStream		= new();
		PacketWriter	= new(WriteStream, Peering.Constructor);
		
		ReadBuffer		= new byte[PacketLengthMaximum];
		ReadStream		= new MemoryStream(ReadBuffer);
		PacketReader	= new Reader(ReadStream, Peering.Constructor);

		ListenThread = Peering.Program.CreateThread(Listening);
		ListenThread.Name = $"{Peering.Name} <- {h.Name}";
		ListenThread.Start();
	}


	protected void Write(PacketType type, int id, object packet)
	{
		lock(Writer)
		{
			Writer.Write(type);
			Writer.Write(id);
			
			WriteStream.SetLength(0);
			BinarySerializator.Serialize(PacketWriter, packet);

			if(WriteStream.Length > PacketLengthMaximum)
				throw new IntegrityException("PacketLengthMaximum exceeded");
			
			Writer.Write((int)WriteStream.Length);
			Writer.Write(new ReadOnlySpan<byte>(WriteStream.GetBuffer(), 0, (int)WriteStream.Length));
		}
	}
//
//	protected T ReadVirtual<T>(out int id) where T : class, IBinarySerializable, ITypeCode
//	{
//		id = Reader.ReadInt32();
//		var l = Reader.ReadInt32();
//
//		if(l > ReadBuffer.Length)
//			throw new IntegrityException("PacketLengthMaximum exceeded");
//
//		Stream.Read(ReadBuffer, 0, l);
//		ReadStream.Position = 0;
//
//		return PacketReader.ReadVirtual<T>();
//	}

	protected T Read<T>(out int id) where T : class, ITypeCode
	{
		id = Reader.ReadInt32();
		var l = Reader.ReadInt32();

		if(l < 0 || l > ReadBuffer.Length)
			throw new IntegrityException("PacketLengthMaximum exceeded");

		Stream.Read(ReadBuffer, 0, l);
		ReadStream.Position = 0;

		return BinarySerializator.Deserialize<T>(PacketReader);
	}

}