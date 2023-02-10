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
using Newtonsoft.Json.Linq;

namespace UC.Net
{
	public enum PacketType : byte
	{
		Null, /*Hello, Blocks, */Request, Response
	}

	public enum EstablishingStatus
	{
		Failed = -1, Null = 0, Initiated = 1, Succeeded = 2, 
	}

	public enum ConnectionStatus
	{
		Disconnected = 0, OK, Failed, Disconnecting
	}

	//public class Header : IBinarySerializable
	//{
	//	public int		LastNonEmptyRound;
	//	public int		LastConfirmedRound;
	//	public byte[]	BaseHash;
	//
	//	public void Read(BinaryReader reader)
	//	{
	//		LastNonEmptyRound	= reader.Read7BitEncodedInt();
	//		LastConfirmedRound	= reader.Read7BitEncodedInt();
	//		BaseHash			= reader.ReadSha3();
	//	}
	//
	//	public void Write(BinaryWriter writer)
	//	{
	//		writer.Write7BitEncodedInt(LastNonEmptyRound);
	//		writer.Write7BitEncodedInt(LastConfirmedRound);
	//		writer.Write(BaseHash);
	//	}
	//}

	public class Peer : RdcInterface
	{
		public IPAddress			IP {get; set;} 

		public EstablishingStatus	InStatus = EstablishingStatus.Null;
		public EstablishingStatus	OutStatus = EstablishingStatus.Null;
		public ConnectionStatus		Status = ConnectionStatus.Disconnected;
		Core						Core;
		public TcpClient			Client;
		NetworkStream				Stream;
		BinaryWriter				Writer;
		BinaryReader				Reader;
		object						Lock;
		Thread						ReadThread;
		Thread						WriteThread;
		List<ITypedBinarySerializable>				Out = new();

		public DateTime				LastSeen = DateTime.MinValue;
		public DateTime				LastTry = DateTime.MinValue;
		public int					Retries;
		public int					ReachFailures;

		public bool					Established => Client != null && Client.Connected && Status == ConnectionStatus.OK;
		public string				StatusDescription => (Status == ConnectionStatus.OK ? (InStatus == EstablishingStatus.Succeeded ? "Inbound" : (OutStatus == EstablishingStatus.Succeeded ? "Outbound" : "<Error>")) : Status.ToString());

		public List<RdcRequest>		InRequests = new();
		public List<RdcRequest>		OutRequests = new();

		public Role					Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0) | (HubRank > 0 ? Role.Hub : 0) | (SeedRank > 0 ? Role.Seed : 0);
		public int					PeerRank = 0;
		public int					ChainRank = 0;
		public int					BaseRank = 0;
		public int					HubRank = 0;
		public int					SeedRank = 0;

		public Peer()
		{
		}

		public Peer(IPAddress ip)
		{
			IP = ip;
		}

		public override string ToString()
		{
			return $"{IP}, {StatusDescription}, Cr={ChainRank}, Hr={HubRank}, Sr={SeedRank}";
		}
 		
		public int GetRank(Role role)
		{
			if(role.HasFlag(Role.Base)) return BaseRank;
			if(role.HasFlag(Role.Chain)) return ChainRank;
			if(role.HasFlag(Role.Hub)) return HubRank;
			if(role.HasFlag(Role.Seed)) return SeedRank;

			throw new IntegrityException("Wrong rank");
		}

  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write7BitEncodedInt64(LastSeen.ToBinary());
			//w.Write7BitEncodedInt(HubHits);
			//w.Write7BitEncodedInt(HubMisses);
			w.Write(PeerRank);
			w.Write(ChainRank);
			w.Write(HubRank);
			w.Write(SeedRank);
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
			//HubHits = r.Read7BitEncodedInt();
			//HubMisses = r.Read7BitEncodedInt();
			PeerRank = r.ReadInt32();
			ChainRank = r.ReadInt32();
			HubRank = r.ReadInt32();
			SeedRank = r.ReadInt32();
  		}
 
 		public void WritePeer(BinaryWriter w)
 		{
 			w.Write(IP);
			w.Write((byte)Roles);
 		}
 
 		public void ReadPeer(BinaryReader reader)
 		{
 			IP = reader.ReadIPAddress();
			var r = (Role)reader.ReadByte();
			BaseRank	= r.HasFlag(Role.Base) ? 1 : 0;
			ChainRank	= r.HasFlag(Role.Chain) ? 1 : 0;
			HubRank		= r.HasFlag(Role.Hub) ? 1 : 0;
			SeedRank	= r.HasFlag(Role.Seed) ? 1 : 0;

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

		public void Start(Core core, TcpClient client, Hello h, object lockk, string host)
		{
			Core = core;
			Client = client;
			
			Client.ReceiveTimeout = 0;
			Client.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Core.Timeout;

			PeerRank++;
			Status				= ConnectionStatus.OK;
			Stream				= client.GetStream();
			Writer				= new BinaryWriter(Stream);
			Reader				= new BinaryReader(Stream);
			LastSeen			= DateTime.UtcNow;
			Lock				= lockk;
			BaseRank			= h.Roles.HasFlag(Role.Base) ? 1 : 0;
			ChainRank			= h.Roles.HasFlag(Role.Chain) ? 1 : 0;
			HubRank				= h.Roles.HasFlag(Role.Hub) ? 1 : 0;
			SeedRank			= h.Roles.HasFlag(Role.Seed) ? 1 : 0;

			core.UpdatePeers(new Peer[]{this});

			ReadThread = new (() => Listening());
			ReadThread.Name = $"{host} listening to {IP.GetAddressBytes()[3]}";
			ReadThread.Start();
	
			WriteThread =	new (() => 
							{
								try
								{
									while(core.Running && Established)
									{
										Thread.Sleep(1);

										lock(InRequests)
											if(InRequests.Any())
											{	
												foreach(var i in InRequests.ToArray())
												{
													if(i.WaitResponse)
													{
														RdcResponse rp;
													
														lock(core.Lock)
															try
															{
																rp = i.Execute(core);
															}
															catch(RdcException ex)// when(!Debugger.IsAttached)
															{
																rp = RdcResponse.FromType(core.Database, i.Type);
																rp.Error = ex.Error;
															}
															catch(Exception)// when(!Debugger.IsAttached)
															{
																//Core?.Workflow?.Log.ReportError(this, "Distributed Call Execution", ex);
																rp = RdcResponse.FromType(core.Database, i.Type);
																rp.Error = RdcError.Internal;
															}
													
														rp.Id = i.Id;
														
	 													lock(Out)
															Out.Add(rp);
													} 
													else
													{
														lock(core.Lock)
															try
															{
																i.Execute(core);
															}
															catch(Exception) when(!Debugger.IsAttached)
															{
															}
													}

													InRequests.Remove(i);
												}
											}

	
										try
										{
											lock(Out)
											{
												foreach(var i in Out)
												{
													if(i is RdcRequest)
								 						Writer.Write((byte)PacketType.Request);
													else if(i is RdcResponse)
								 						Writer.Write((byte)PacketType.Response);
													else
														throw new IntegrityException("Wrong packet to write");

													Writer.Write(i.TypeCode);

													BinarySerializator.Serialize(Writer, i);
												}

												Out.Clear();
											}
										}
										catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
										{
											lock(Lock)
								 				if(Status != ConnectionStatus.Disconnecting)
								 					Status = ConnectionStatus.Failed;

											break;
										}
									}
								}
								catch(Exception) when(!Debugger.IsAttached)
								{
									Disconnect();
								}
							});
	
			WriteThread.Name = $"{host} sending to {IP.GetAddressBytes()[3]}";
			WriteThread.Start();
		}

		void Listening()
		{
	 		try
	 		{
				while(Established)
				{
					var pk = (PacketType)Reader.ReadByte();

					//if(pk == null)
					//{
					//	lock(Lock)
					//		peer.Status = ConnectionStatus.Failed;
					//	return;
					//}

					lock(Lock)
						if(!Core.Running || !Established)
							return;
					
					switch(pk)
					{
 						case PacketType.Request:
 						{
							RdcRequest rq;

 							try
 							{
								rq = BinarySerializator.Deserialize(Reader,	
																	t => {
																			var o = UC.Net.RdcRequest.FromType(Core.Database, (Rdc)t); 
																			o.Peer = this; 
																			return o as object;
																	},
																	Core.Constractor) as RdcRequest;

 							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								Disconnect();
 								break;
 							}

							lock(InRequests)
 								InRequests.Add(rq);
 	
 							break;
 						}

						case PacketType.Response:
 						{
							RdcResponse rp;
							
							try
 							{
								rp = BinarySerializator.Deserialize(Reader,	
																	t => UC.Net.RdcResponse.FromType(Core.Database, (Rdc)t) as object, 
																	Core.Constractor) as RdcResponse;
							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								Disconnect();
 								break;
 							}

							lock(OutRequests)
								//foreach(var rp in responses)
								{
									var rq = OutRequests.Find(j => j.Id.SequenceEqual(rp.Id));

									if(rq != null)
									{
										rp.Peer = this;
										rq.Response = rp;
										rq.Event.Set();
 									
										if(rp.Final)
										{
											OutRequests.Remove(rq);
										}
									}
								}
							break;
						}

						default:
							Core.Workflow.Log?.ReportError(this, $"Wrong packet type {pk}");
							Status = ConnectionStatus.Failed;
							return;
					}
				}
	 		}
			catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
			{
				lock(Lock)
					if(Status != ConnectionStatus.Disconnecting)
						Status = ConnectionStatus.Failed;
			}
			catch(Exception) when (!Debugger.IsAttached)
			{
				Disconnect();
			}
		}	
// 		public PacketType Read()
// 		{
// 			try
// 			{
// 				//var buf = new byte[65636];
// 
// 				//var p = new Packet();
// 				//var s = new MemoryStream();
// 				var r = new BinaryReader(Stream);
// 	
// 				return (PacketType)r.ReadByte();
// 				//var ndata	= r.Read7BitEncodedInt64();
// 				//
// 				//if(ndata > 0)
// 				//{
// 				//	while(s.Length < ndata)
// 				//	{
// 				//		var n = Stream.Read(buf, 0, Math.Min(buf.Length, (int)(ndata - s.Length)));
// 				//		s.Write(buf, 0, n);
// 				//	}
// 				//		
// 				//	s.Position = 0;
// 				//	p.Data = s;
// 				//}
// 				//
// 				//return p;
// 			}
// 			catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
// 			{
// 				lock(Lock)
// 					if(Status != ConnectionStatus.Disconnecting)
// 						Status = ConnectionStatus.Failed;
// 			}
// 
// 			//return null;
// 		}

// 		public void Send(RdcRequest packet)
// 		{
// 			lock(Out)
// 			{
// 				Out.Add(packet);
// 			}
// 		}
// 
// 		public void Send(object packet)
// 		{
// // 			var rq = new Packet();
// // 			//rq.Header = h;
// // 			rq.Type = type;
// // 			rq.Data = data;
// 
// 			lock(Out)
// 			{
// 				Out.Enqueue(packet);
// 			}
// 		}

 		public override Rp Request<Rp>(RdcRequest rq) where Rp : class
 		{
			if(!Established)
				throw new ConnectionFailedException("Peer is not connected");

			lock(OutRequests)
			{	
				//var p = new Packet();
				//p.Type = PacketType.Request;
				//var s = new MemoryStream();
				//BinarySerializator.Serialize(new BinaryWriter(s), new[]{rq});
				//p.Data = s;

				if(rq.WaitResponse)
					OutRequests.Add(rq);
				
				lock(Out)
					Out.Add(rq);
			}
 
 			if(rq.WaitResponse)
 			{
	 			if(rq.Event.WaitOne(Settings.Dev.DisableTimeouts ? Timeout.Infinite : Core.Timeout))
	 			{
					if(rq.Response == null)
						throw new OperationCanceledException();
	
	 				if(rq.Response.Error == RdcError.Null)
		 				return rq.Response as Rp;
	 				else
						throw new RdcException(rq.Response.Error);
	 			}
				else
	 				throw new RdcException(RdcError.Timeout);
 			}
			else
				return null;
 		}
	}
}