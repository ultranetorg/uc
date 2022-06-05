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
using System.Net.Http;
using System.Text.Json;

namespace UC.Net
{
	public enum PacketType : byte
	{
		Null, Hello, Blocks, RoundsRequest, Rounds, Request, Response
	}

	public class Packet
	{
		//public Header			Header;
		public PacketType		Type;
		public MemoryStream		Data;

		public Packet()
		{
		}

		public Packet(PacketType type, MemoryStream data)
		{
			Type = type;
			Data = data;
		}

		public static Packet Create<T>(PacketType type, IEnumerable<T> many) where T : IBinarySerializable
		{
			if(many.Count() > 0)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				w.Write7BitEncodedInt(many.Count());
	
				foreach(var i in many)
				{
					if(i is ITypedBinarySerializable t)
						w.Write(t.TypeCode);

					i.Write(w);
				}
	
				return new Packet(type, s);
			}
			else
				return null;
		}
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

	public class Peer : Nci
	{
		public int					JoinedAt {get; set;} /// json serializable
		public IPAddress			IP {get; set;} 
		
		public int					LastRound;
		public int					LastConfirmedRound;

		public EstablishingStatus	InStatus = EstablishingStatus.Null;
		public EstablishingStatus	OutStatus = EstablishingStatus.Null;
		public ConnectionStatus		Status = ConnectionStatus.Disconnected;
		Core						Core;
		public TcpClient			Client;
		NetworkStream				Stream;
		BinaryWriter				Writer;
		object						Lock;
		Thread						ReadThread;
		Thread						WriteThread;
		Queue<Packet>				Out = new();

		public DateTime				LastSeen = DateTime.MinValue;
		public DateTime				LastTry = DateTime.MinValue;
		public int					Retries;

		public bool					Established => Client != null && Client.Connected && Status == ConnectionStatus.OK;
		public string				StatusDescription => (Status == ConnectionStatus.OK ? (InStatus == EstablishingStatus.Succeeded ? "Inbound" : (OutStatus == EstablishingStatus.Succeeded ? "Outbound" : "<Error>")) : Status.ToString());

		public List<Request>		InRequests = new();
		public List<Request>		OutRequests = new();

		public Role					Role;

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
		
 
 		public void WriteMember(BinaryWriter w)
 		{
 			w.Write(IP.GetAddressBytes());
 			w.Write(Generator);
			w.Write7BitEncodedInt(JoinedAt);
 		}
 
 		public void ReadMember(BinaryReader r)
 		{
 			IP = new IPAddress(r.ReadBytes(4));
			Generator = r.ReadAccount();
			JoinedAt = r.Read7BitEncodedInt();
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
			lock(Lock)
			{
				if(Status != ConnectionStatus.Disconnecting)
					Status = ConnectionStatus.Disconnecting;
				else
					return;

				foreach(var i in OutRequests)
					i.Event.Set();

				lock(OutRequests)
					OutRequests.Clear();
	
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
			Core = core;
			Client = client;

			Status				= ConnectionStatus.OK;
			Stream				= client.GetStream();
			Writer				= new BinaryWriter(Stream);
			LastSeen			= DateTime.UtcNow;
			Lock				= lockk;
			LastRound			= h.LastRound;
			LastConfirmedRound	= h.LastConfirmedRound;
			Role				= h.Capabilities;
	
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

										lock(InRequests)
											if(InRequests.Any())
											{	
												var responses = new List<Response>();
											
												foreach(var i in InRequests.ToArray())
												{
													Response rp;
												
													lock(core.Lock)
														try
														{
															rp = i.Execute(core);
															//rp = core.Respond(this, i);
															rp.Status = ResponseStatus.OK;
														}
														catch(RpcException ex)
														{
															rp = Response.FromType(core.Chain, i.Type);
															rp.Status = ResponseStatus.Failed;
														}
												
													rp.Id = i.Id;
													responses.Add(rp);
													InRequests.Remove(i);
												}

												if(responses.Any())
												{
													var s = new MemoryStream();
													BinarySerializator.Serialize(new BinaryWriter(s), responses);
													Send(new Packet(PacketType.Response, s));
												}
											}

										p = null;
	
										lock(Out)
											if(Out.Count > 0)
											{
												p = Out.Dequeue();
											}
	
										if(p != null)
										{
											try
											{
												var h = core.Header;

									 			Writer.Write7BitEncodedInt(h.LastRound);
									 			Writer.Write7BitEncodedInt(h.LastConfirmedRound);
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
	
			WriteThread.Name = $"{host} sending to {IP.GetAddressBytes()[3]}";
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

		public void Send(Packet packet)
		{
			if(packet == null)
			{
				packet = packet;
			}

			lock(Out)
			{
				Out.Enqueue(packet);
			}
		}

		public void RequestRounds(int from, int to)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write7BitEncodedInt(from);
			w.Write7BitEncodedInt(to);

			Send(new Packet(PacketType.RoundsRequest, s));
		}

 		public override Rp Request<Rp>(Request rq) where Rp : class
 		{
			if(!Established)
				throw new RpcException("Peer is not connectevd");

			var s = new MemoryStream();
			BinarySerializator.Serialize(new BinaryWriter(s), new[] {rq});
												
			Send(new Packet(PacketType.Request, s));

			rq.Sent = true;

			lock(OutRequests)
			{
				OutRequests.Add(rq);
			}
 
 			if(rq.Event.WaitOne(Settings.Dev.DisableTimeouts ? Timeout.Infinite : 15000))
 			{
				if(rq.RecievedResponse == null)
					throw new OperationCanceledException();

 				if(rq.RecievedResponse.Status == ResponseStatus.OK)
	 				return rq.RecievedResponse as Rp;
 				else
					throw new RpcException("Operation failed");
 			}
			else
 				throw new TimeoutException($"Request {rq.GetType().Name} has timed out");
 		}

 		//public void Send(Request rq)
		//{
		//	var s = new MemoryStream();
		//	BinarySerializator.Serialize(new BinaryWriter(s), new[]{rq});
		//										
		//	Send(Core.Header, PacketType.Request, s);
		//
		//	rq.Sent = true;
		//	OutRequests.Add(rq);
		//}
	}
}