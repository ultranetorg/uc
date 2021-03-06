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
		public IPAddress			IP {get; set;} 
		public int					JoinedGeneratorsAt {get; set;}
		
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
		public int					HubHits;
		public int					HubMisses;
		//public List<PackageAddress>	HubMissedResources = new();
		//public List<PackageAddress>	HubHitResources = new();

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
			return $"{IP}, {StatusDescription}, Generator={Generator}, JoinedAt={JoinedGeneratorsAt}";
		}
 		
  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write7BitEncodedInt64(LastSeen.ToBinary());
			w.Write7BitEncodedInt(HubHits);
			w.Write7BitEncodedInt(HubMisses);
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
			HubHits = r.Read7BitEncodedInt();
			HubMisses = r.Read7BitEncodedInt();
  		}
 
 		public void WriteNode(BinaryWriter w)
 		{
 			w.Write(IP);
			w.Write((byte)Role);
 		}
 
 		public void ReadNode(BinaryReader r)
 		{
 			IP = r.ReadIPAddress();
			Role = (Role)r.ReadByte();
 		}
		
  		public void WriteMember(BinaryWriter w)
 		{
 			w.Write(IP);
 			w.Write(Generator);
			w.Write7BitEncodedInt(JoinedGeneratorsAt);
 		}
 
 		public void ReadMember(BinaryReader r)
 		{
 			IP = r.ReadIPAddress();
			Generator = r.ReadAccount();
			JoinedGeneratorsAt = r.Read7BitEncodedInt();
 		}
		
//   		public void WriteHub(BinaryWriter w)
//  		{
//  			w.Write(IP);
// 			w.Write7BitEncodedInt(JoinedHubsAt);
//  		}
//  
//  		public void ReadHub(BinaryReader r)
//  		{
//  			IP = r.ReadIPAddress();
// 			JoinedHubsAt = r.Read7BitEncodedInt();
//  		}

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
			Role				= h.Roles;
	
			ReadThread = new (() => { read(this); });
			ReadThread.Name = $"{host} listening to {IP.GetAddressBytes()[3]}";
			ReadThread.Start();
	
			WriteThread =	new (() => 
							{
								try
								{
									while(core.Running && Established)
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
															//rp.Status = ResponseStatus.OK;
														}
														catch(Exception ex)// when(!Debugger.IsAttached)
														{
															rp = Response.FromType(core.Chain, i.Type);
															rp.Error = ex.Message;
														}
												
													rp.Id = i.Id;
													responses.Add(rp);
													InRequests.Remove(i);
												}

												if(responses.Any())
												{
													p = new Packet();
													//p.Header = core.Header;
													p.Type = PacketType.Response;
													//p.Data = Core.Write(responses);
													var s = new MemoryStream();
													BinarySerializator.Serialize(new BinaryWriter(s), responses);
													p.Data = s;

													lock(Out)
														Out.Enqueue(p);
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
			lock(Out)
			{
				Out.Enqueue(packet);
			}
		}

		public void Send(Header h, PacketType type, MemoryStream data)
		{
			var rq = new Packet();
			//rq.Header = h;
			rq.Type = type;
			rq.Data = data;

			lock(Out)
			{
				Out.Enqueue(rq);
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

 		public override Rp Request<Rp>(Request rq) where Rp : class
 		{
			if(!Established)
				throw new RequirementException("Peer is not connectevd");

			lock(OutRequests)
			{	
				var p = new Packet();
				p.Type = PacketType.Request;
				var s = new MemoryStream();
				BinarySerializator.Serialize(new BinaryWriter(s), new[]{rq});
				p.Data = s;

				OutRequests.Add(rq);
												
				lock(Out)
					Out.Enqueue(p);
			}
 
 			if(rq.Event.WaitOne(Settings.Dev.DisableTimeouts ? Timeout.Infinite : 15000))
 			{
				if(rq.RecievedResponse == null)
					throw new OperationCanceledException();

 				if(rq.RecievedResponse.Error == null)
	 				return rq.RecievedResponse as Rp;
 				else
					throw new RemoteCallException(rq.RecievedResponse);
 			}
			else
 				throw new TimeoutException($"Request {rq.GetType().Name} has timed out");
 		}
	}
}