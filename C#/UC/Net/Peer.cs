using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Signer;
using System.Reflection;
using System.Diagnostics;

namespace UC.Net
{
	public enum PacketType : byte
	{
		Null, Hello, Blocks, RoundsRequest, Rounds, Message
	}

	public class Packet
	{
		public Header			Header;
		public PacketType		Type;
		public MemoryStream		Data;
	}

	public enum EstablishingStatus
	{
		Failed = -1, Null = 0, Initiated = 1, Succeeded = 2, 
	}

	public enum ConnectionStatus
	{
		Disconnected = 0, OK, Failed, Disconnecting
	}

	public class Header
	{
		public int		LastRound;
		public int		LastConfirmedRound;
	}

	public class Peer// : IBinarySerializable
	{
		public IPAddress				IP {get; set;} /// json serializable
		public Account					Generator {get; set;} /// json serializable
		public int						JoinedAt {get; set;} /// json serializable
		
		public int						LastRound;
		public int						LastConfirmedRound;

		public EstablishingStatus		InStatus = EstablishingStatus.Null;
		public EstablishingStatus		OutStatus = EstablishingStatus.Null;
		public ConnectionStatus			Status = ConnectionStatus.Disconnected;
		public TcpClient				Client;
		NetworkStream					Stream;
		BinaryWriter					Writer;
		object							Lock;
		Thread							ReadThread;
		Thread							WriteThread;
		Queue<Packet>					OutQueue = new();

		public DateTime					LastSeen = DateTime.MinValue;
		public DateTime					LastTry = DateTime.MinValue;
		public int						Retries;

		public bool						Established => Client != null && Client.Connected && Status == ConnectionStatus.OK;
		public string					StatusDescription => (Status == ConnectionStatus.OK ? (InStatus == EstablishingStatus.Succeeded ? "Inbound" : (OutStatus == EstablishingStatus.Succeeded ? "Outbound" : "<Error>")) : Status.ToString());

		public JsonClient				Api;
		public int						ApiReachFailures;

		public Peer()
		{
		}

		public Peer(IPAddress ip)
		{
			IP = ip;
		}

		public override string ToString()
		{
			return $"{IP}, {StatusDescription}, Generator={Generator}, JoinedAt={JoinedAt}";
		}
 		
  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write(LastSeen.ToBinary());
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.ReadInt64());
  		}
 
 		public void WriteNode(BinaryWriter w)
 		{
 			w.Write(IP.GetAddressBytes());
 		}
 
 		public void ReadNode(BinaryReader r)
 		{
 			IP = new IPAddress(r.ReadBytes(4));
 		}

		public static void SendHello(TcpClient client, int[] versions, string zone, Guid sid, IPAddress ip, IEnumerable<Peer> peers, Header head)
		{
			var w = new BinaryWriter(client.GetStream());

			w.Write(versions, i => w.Write7BitEncodedInt(i));
			w.WriteUtf8(zone);
			w.Write(ip.GetAddressBytes());
			w.Write(sid.ToByteArray());
			w.Write(peers, i => i.WriteNode(w));
			w.Write7BitEncodedInt(head.LastRound);
			w.Write7BitEncodedInt(head.LastConfirmedRound);
		}


		public static Hello WaitHello(TcpClient client)
		{
			var r = new BinaryReader(client.GetStream());

			var h = new Hello();

			h.Versions				= r.ReadArray(() => r.Read7BitEncodedInt());
			h.Zone					= r.ReadUtf8();
			h.IP					= new IPAddress(r.ReadBytes(4));
			h.Session				= new Guid(r.ReadBytes(16));
			h.Peers					= r.ReadArray<Peer>(() => {var p = new Peer(); p.ReadNode(r); return p;});
			h.LastRound				= r.Read7BitEncodedInt();
			h.LastConfirmedRound	= r.Read7BitEncodedInt();

			return h;
		}

		public void Disconnect()
		{
			lock(Lock)
			{
				if(Status != ConnectionStatus.Disconnecting)
					Status = ConnectionStatus.Disconnecting;
				else
					return;
	
				if(Client != null)
				{
					Client.Close();
					Client = null;
				}
	
				if(WriteThread != null)
					WriteThread = null;
	
				if(ReadThread != null)
	 				ReadThread = null;
		
				Status = ConnectionStatus.Disconnected;
				InStatus = EstablishingStatus.Null;
				OutStatus = EstablishingStatus.Null;
			}
		}

		public void Start(Core core, TcpClient client, Hello h, Action<Peer> read, object lockk, string host)
		{
			Client = client;

			Status				= ConnectionStatus.OK;
			Stream				= client.GetStream();
			Writer				= new BinaryWriter(Stream);
			LastSeen			= DateTime.UtcNow;
			Lock				= lockk;
			LastRound			= h.LastRound;
			LastConfirmedRound	= h.LastConfirmedRound;
	
			ReadThread = new (() => { read(this); });
			ReadThread.Name = $"{host} listening to {IP.GetAddressBytes()[3]}";
			ReadThread.Start();
	
			WriteThread =	new (() => 
							{
								try
								{
									while(core.Working && Established)
									{
										Packet p = null;
	
										lock(OutQueue)
										{
											if(OutQueue.Count > 0)
											{
												p = OutQueue.Dequeue();
											}
										}
	
										if(p != null)
										{
											try
											{
								 				Writer.Write7BitEncodedInt(p.Header.LastRound);
								 				Writer.Write7BitEncodedInt(p.Header.LastConfirmedRound);
								 				Writer.Write((byte)p.Type);
								 
								 				if(p.Data != null)
								 				{
								 					Writer.Write7BitEncodedInt64(p.Data.Length);
													p.Data.WriteTo(Stream);
								 				}
								 				else
								 					Writer.Write7BitEncodedInt64(0);

												Writer.Flush();
											}
											catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
											{
												lock(Lock)
								 					if(Status != ConnectionStatus.Disconnecting)
								 						Status = ConnectionStatus.Failed;
											}
										}
										else
											Thread.Sleep(1);
									}
								}
								catch(Exception ex) when(!Debugger.IsAttached)
								{
									core.Stop(MethodBase.GetCurrentMethod(), ex);
								}
							});
	
			WriteThread.Name			= $"{host} sending to {IP.GetAddressBytes()[3]}";
			//WriteThread.IsBackground	= true;
			WriteThread.Start();
		}

		public Packet Read()
		{
			try
			{
				var buf = new byte[Client.ReceiveBufferSize];

				var p = new Packet();
				var s = new MemoryStream();
				var r = new BinaryReader(Stream);
	
				LastRound			= r.Read7BitEncodedInt();
				LastConfirmedRound	= r.Read7BitEncodedInt();
				p.Type				= (PacketType)r.ReadByte();
				var ndata			= r.Read7BitEncodedInt64();
	
				if(ndata > 0)
				{
					while(s.Length < ndata)
					{
						var n = Stream.Read(buf, 0, Math.Min(buf.Length, (int)(ndata - s.Length)));
						s.Write(buf, 0, n);
					}
						
					s.Position = 0;
					p.Data = s;
				}
			
				return p;
			}
			catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
			{
				lock(Lock)
					if(Status != ConnectionStatus.Disconnecting)
						Status = ConnectionStatus.Failed;
			}

			return null;
		}

		public void Send(Header h, PacketType type, MemoryStream data)
		{
			var rq = new Packet();
			rq.Header = h;
			rq.Type = type;
			rq.Data = data;

			lock(OutQueue)
			{
				OutQueue.Enqueue(rq);
			}
		}

		public void RequestRounds(Header h, int from, int to)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write7BitEncodedInt(from);
			w.Write7BitEncodedInt(to);

			Send(h, PacketType.RoundsRequest, s);
		}
	}
}