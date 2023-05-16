using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Uccs.Net
{
	public enum PacketType : byte
	{
		Null, Request, Response
	}

	public enum EstablishingStatus
	{
		Failed = -1, Null = 0, Initiated = 1, Succeeded = 2, 
	}

	public enum ConnectionStatus
	{
		Disconnected = 0, OK, Failed, Disconnecting
	}

	public class Peer : RdcInterface
	{
		public IPAddress			IP {get; set;} 

		public EstablishingStatus	InStatus = EstablishingStatus.Null;
		public EstablishingStatus	OutStatus = EstablishingStatus.Null;
		public ConnectionStatus		Status = ConnectionStatus.Disconnected;
		Core						Core;
		public TcpClient			Tcp;
		NetworkStream				Stream;
		BinaryWriter				Writer;
		BinaryReader				Reader;
		Thread						ReadThread;
		Thread						WriteThread;
		Queue<RdcPacket>			Out = new();

		public DateTime				LastSeen = DateTime.MinValue;
		public DateTime				LastTry = DateTime.MinValue;
		public int					Retries;
		public int					ReachFailures;

		public bool					Established => Tcp != null && Tcp.Connected && Status == ConnectionStatus.OK;
		public string				StatusDescription => (Status == ConnectionStatus.OK ? (InStatus == EstablishingStatus.Succeeded ? "Inbound" : (OutStatus == EstablishingStatus.Succeeded ? "Outbound" : "<Error>")) : Status.ToString());

		public List<RdcRequest>		InRequests = new();
		public List<RdcRequest>		OutRequests = new();

		public Role					Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0) | (HubRank > 0 ? Role.Hub : 0) | (SeedRank > 0 ? Role.Seed : 0);
		public int					PeerRank = 0;
		public int					ChainRank = 0;
		public int					BaseRank = 0;
		public int					HubRank = 0;
		public int					SeedRank = 0;

		public Dictionary<Role, DateTime>	LastFailure = new();
		
		int							IdCounter = 0;
		public bool					Fresh;

		AutoResetEvent				SendSignal = new AutoResetEvent(true);

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
			w.Write(PeerRank);
			w.Write(ChainRank);
			w.Write(HubRank);
			w.Write(SeedRank);
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
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
			lock(Core.Lock)
			{
				if(Status != ConnectionStatus.Disconnecting)
					Status = ConnectionStatus.Disconnecting;
				else
					return;

				SendSignal.Set();

				foreach(var i in OutRequests)
					i.Event.Set();

				lock(OutRequests)
					OutRequests.Clear();
	
				if(Tcp != null)
				{
					Tcp.Close();
					Tcp = null;
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

		public void Start(Core core, TcpClient client, Hello h, string host)
		{
			Core = core;
			Tcp = client;
			
			Tcp.ReceiveTimeout = 0;
			Tcp.SendTimeout = Settings.Dev.DisableTimeouts ? 0 : Core.Timeout;

			PeerRank++;
			Status		= ConnectionStatus.OK;
			Stream		= client.GetStream();
			Writer		= new BinaryWriter(Stream);
			Reader		= new BinaryReader(Stream);
			LastSeen	= DateTime.UtcNow;
			BaseRank	= h.Roles.HasFlag(Role.Base)	? 1 : 0;
			ChainRank	= h.Roles.HasFlag(Role.Chain)	? 1 : 0;
			HubRank		= h.Roles.HasFlag(Role.Hub)		? 1 : 0;
			SeedRank	= h.Roles.HasFlag(Role.Seed)	? 1 : 0;

			core.UpdatePeers(new Peer[]{this});

			ReadThread = new (() => Listening());
			ReadThread.Name = $"{host} <- {IP.GetAddressBytes()[3]}";
			ReadThread.Start();
	
			WriteThread = new (() => Sending());
			WriteThread.Name = $"{host} -> {IP.GetAddressBytes()[3]}";
			WriteThread.Start();
		}

		void Sending()
		{
			try
			{
				while(!Core.Workflow.IsAborted && Established)
				{
					Core.Statistics.Sending.Begin();
	
					RdcRequest[] ins;
	
					lock(InRequests)
					{
						ins = InRequests.ToArray();
						InRequests.Clear();
					}
							
					foreach(var i in ins)
					{
						if(i.WaitResponse)
						{
							RdcResponse rp;
														
							try
							{
								rp = i.Execute(Core);
								rp.Result = RdcResult.Success;
							}
							catch(RdcNodeException ex)
							{
								rp = RdcResponse.FromType(Core.Database, i.Type);
								rp.Result = RdcResult.NodeException;
								rp.Error = (byte)ex.Error;
							}
							catch(RdcEntityException ex)
							{
								rp = RdcResponse.FromType(Core.Database, i.Type);
								rp.Result = RdcResult.EntityException;
								rp.Error = (byte)ex.Error;
							}
							catch(Exception)// when(!Debugger.IsAttached)
							{
								rp = RdcResponse.FromType(Core.Database, i.Type);
								rp.Result = RdcResult.NodeException;
								rp.Error = (byte)RdcNodeError.Internal;
							}
														
							rp.Id = i.Id;
															
		 					lock(Out)
								Out.Enqueue(rp);
						}
						else
						{
							try
							{
								i.Execute(Core);
							}
							catch(Exception) when(!Debugger.IsAttached)
							{
							}
						}
					}
		
					try
					{
						RdcPacket[] outs;
	
						lock(Out)
						{
							outs = Out.ToArray();
							Out.Clear();
						}
	
						foreach(var i in outs)
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
					}
					catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
					{
						lock(Core.Lock)
							if(Status != ConnectionStatus.Disconnecting)
								Status = ConnectionStatus.Failed;
	
						break;
					}
	
					Core.Statistics.Sending.End();
									
					SendSignal.WaitOne();
				}
			}
			catch(Exception) when(!Debugger.IsAttached)
			{
				Disconnect();
			}
		}

		void Listening()
		{
	 		try
	 		{
				while(true)
				{
					var pk = (PacketType)Reader.ReadByte();

					lock(Core.Lock)
						if(Core.Workflow.IsAborted || !Established)
							return;
					
					Core.Statistics.Reading.Begin();

					switch(pk)
					{
 						case PacketType.Request:
 						{
							RdcRequest rq;

 							try
 							{
								rq = BinarySerializator.Deserialize(Reader,	
																	t =>{
																			var o = RdcRequest.FromType(Core.Database, (Rdc)t); 
																			o.Peer = this; 
																			return o as object;
																		},
																	Core.Constract) as RdcRequest;
 							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								Disconnect();
 								break;
 							}

							lock(InRequests)
 								InRequests.Add(rq);
 	
							//lock(SendSignal)
								SendSignal.Set();

							//Core.Workflow.Log?.Report(this, pk.ToString(), $"{rq.Type} {rq.Id} {IP}");

 							break;
 						}

						case PacketType.Response:
 						{
							RdcResponse rp;
							
							try
 							{
								rp = BinarySerializator.Deserialize(Reader,	
																	t => RdcResponse.FromType(Core.Database, (Rdc)t) as object, 
																	Core.Constract) as RdcResponse;
							}
 							catch(Exception) when(!Settings.Dev.ThrowOnCorrupted)
 							{
 								Disconnect();
 								break;
 							}

							lock(OutRequests)
							{
								var rq = OutRequests.Find(j => j.Id == rp.Id);

								if(rq != null)
								{
									rp.Peer = this;
									rq.Response = rp;
									rq.Event.Set();
 									
									OutRequests.Remove(rq);
								}
							}

							//Core.Workflow.Log?.Report(this, pk.ToString(), $"{rp.Type} {rp.Id} {IP}");

							break;
						}

						default:
							Core.Workflow.Log?.ReportError(this, $"Wrong packet type {pk}");
							Status = ConnectionStatus.Failed;
							return;
					}

					Core.Statistics.Reading.End();
				}
	 		}
			catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
			{
				lock(Core.Lock)
					if(Status != ConnectionStatus.Disconnecting)
						Status = ConnectionStatus.Failed;
			}
			catch(Exception) when (!Debugger.IsAttached)
			{
				Disconnect();
			}
		}	

 		public override void Send(RdcRequest rq)
 		{
			if(!Established)
				throw new ConnectionFailedException("Peer is not connected");

			rq.Id = IdCounter++;

			lock(Out)
				Out.Enqueue(rq);

			SendSignal.Set();
 		}


 		public override Rp Request<Rp>(RdcRequest rq) where Rp : class
 		{
			if(!Established)
				throw new ConnectionFailedException("Peer is not connected");

			rq.Id = IdCounter++;

			if(rq.WaitResponse)
				lock(OutRequests)
					OutRequests.Add(rq);

			lock(Out)
				Out.Enqueue(rq);

			SendSignal.Set();

 			if(rq.WaitResponse)
 			{
	 			if(rq.Event.WaitOne(30*1000)) 
	 			{
					if(rq.Response == null)
						throw new OperationCanceledException();
	
	 				if(rq.Response.Result == RdcResult.Success)
		 				return rq.Response as Rp;
	 				else if(rq.Response.Result == RdcResult.NodeException)
						throw new RdcNodeException((RdcNodeError)rq.Response.Error);
	 				else if(rq.Response.Result == RdcResult.EntityException)
						throw new RdcEntityException((RdcEntityError)rq.Response.Error);
					else
						throw new RdcNodeException(RdcNodeError.Integrity);
 				}
				else
	 				throw new RdcNodeException(RdcNodeError.Timeout);
 			}
			else
				return null;
 		}
	}
}