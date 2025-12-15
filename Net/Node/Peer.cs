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
	public Endpoint					EP {get; set;} 
	public string					Name;
	public string					Net;

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
	public long						Roles;

	public Peering					Peering;
	protected TcpClient				Tcp;
	protected NetworkStream			Stream;
	protected BinaryWriter			Writer;
	protected BinaryReader			Reader;
	protected Thread				ListenThread;
	protected int					IdCounter = 0;
	
	protected List<RequestPacket>	OutRequests = new();

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

	public void SaveNode(BinaryWriter writer)
	{
		//writer.WriteUtf8(Net);
		writer.Write(EP);
		writer.Write7BitEncodedInt64(Roles);
		writer.Write7BitEncodedInt64(LastSeen.ToBinary());
		writer.Write(PeerRank);
	}

	public void LoadNode(BinaryReader reader)
	{
		//Net = reader.ReadUtf8();
		EP = reader.Read<Endpoint>();
		Roles = reader.Read7BitEncodedInt64();
		LastSeen = DateTime.FromBinary(reader.Read7BitEncodedInt64());
		PeerRank = reader.ReadInt32();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(EP);
		writer.Write7BitEncodedInt64(Roles);
	}

	public void Read(BinaryReader reader)
	{
		EP = reader.Read<Endpoint>();
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

		if(ListenThread == null)
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

	protected abstract void Listening();

}