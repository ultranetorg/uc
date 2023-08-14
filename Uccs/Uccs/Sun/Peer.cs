using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
		public TcpClient			Tcp;

		public DateTime				LastSeen = DateTime.MinValue;
		public DateTime				LastTry = DateTime.MinValue;
		public int					Retries;
		//public int					ReachFailures;

		public bool					Established => Tcp != null && Tcp.Connected && Status == ConnectionStatus.OK;
		public string				StatusDescription => (Status == ConnectionStatus.OK ? (InStatus == EstablishingStatus.Succeeded ? "Inbound" : (OutStatus == EstablishingStatus.Succeeded ? "Outbound" : "<Error>")) : Status.ToString());

		public Role					Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0);
		public int					PeerRank = 0;
		public int					ChainRank = 0;
		public int					BaseRank = 0;
		//public int					HubRank = 0;
		//public int					SeedRank = 0;

		public Dictionary<Role, DateTime>	LastFailure = new();

		Sun							Sun;
		NetworkStream				Stream;
		BinaryWriter				Writer;
		BinaryReader				Reader;
		Thread						ReadThread;
		Thread						WriteThread;
		Queue<RdcPacket>			Outs = new();
		public List<RdcRequest>			InRequests = new();
		List<RdcRequest>			OutRequests = new();
		AutoResetEvent				SendSignal = new AutoResetEvent(true);
		
		int							IdCounter = 0;
		public bool					Fresh;

		public Peer()
		{
		}

		public Peer(IPAddress ip)
		{
			IP = ip;
		}

		public override string ToString()
		{
			return $"{IP}, {StatusDescription}";
		}
 		
		public int GetRank(Role role)
		{
			if(role == Role.Base) return BaseRank;
			if(role == Role.Chain) return ChainRank;

			throw new IntegrityException("Wrong rank");
		}

  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write7BitEncodedInt64(LastSeen.ToBinary());
			w.Write(PeerRank);
			w.Write(ChainRank);
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
			PeerRank = r.ReadInt32();
			ChainRank = r.ReadInt32();
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
			lock(Sun.Lock)
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

		public void Start(Sun sun, TcpClient client, Hello h, string host)
		{
			Sun = sun;
			Tcp = client;
			
			Tcp.ReceiveTimeout = 0;
			Tcp.SendTimeout = DevSettings.DisableTimeouts ? 0 : Sun.Timeout;

			PeerRank++;
			Status		= ConnectionStatus.OK;
			Stream		= client.GetStream();
			Writer		= new BinaryWriter(Stream);
			Reader		= new BinaryReader(Stream);
			LastSeen	= DateTime.UtcNow;
			BaseRank	= h.Roles.HasFlag(Role.Base)	? 1 : 0;
			ChainRank	= h.Roles.HasFlag(Role.Chain)	? 1 : 0;

			sun.UpdatePeers(new Peer[]{this});

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
				while(Sun.Workflow.Active && Established)
				{
					Sun.Statistics.Sending.Begin();
	
					RdcRequest[] ins;
	
					lock(InRequests)
					{
						ins = InRequests.ToArray();
						InRequests.Clear();
					}
							
					foreach(var i in ins)
					{
						void execute()
						{
							if(i.WaitResponse)
							{
								RdcResponse r;

								try
								{
									r = i.Execute(Sun);
									r.Result = RdcResult.Success;
								}
								catch(RdcNodeException ex)
								{
									r = RdcResponse.FromType(i.Type);
									r.Result = RdcResult.NodeException;
									r.Error = (byte)ex.Error;
								}
								catch(RdcEntityException ex)
								{
									r = RdcResponse.FromType(i.Type);
									r.Result = RdcResult.EntityException;
									r.Error = (byte)ex.Error;
								}
								catch(Exception) when(!Debugger.IsAttached)
								{
									r = RdcResponse.FromType(i.Type);
									r.Result = RdcResult.NodeException;
									r.Error = (byte)RdcNodeError.Internal;
								}

								r.Id = i.Id;

								lock(Outs)
									Outs.Enqueue(r);
							}
							else
							{
								try
								{
									i.Execute(Sun);
								}
								catch(Exception) when(!Debugger.IsAttached)
								{
								}
							}
						}

						if(i is ProxyRequest px)
						{
							Task.Run(() =>	{
												execute();
												SendSignal.Set();
											});
						}
						else
						{
							execute();
						}
					}

					try
					{
						RdcPacket[] outs;
	
						lock(Outs)
						{
							outs = Outs.ToArray();
							Outs.Clear();
						}
	
						foreach(var i in outs)
						{
							if(i is RdcRequest)
								Writer.Write((byte)PacketType.Request);
							else if(i is RdcResponse)
								Writer.Write((byte)PacketType.Response);
							else
								throw new IntegrityException("Wrong packet to write");
	
							BinarySerializator.Serialize(Writer, i);
						}
					}
					catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
					{
						lock(Sun.Lock)
							if(Status != ConnectionStatus.Disconnecting)
								Status = ConnectionStatus.Failed;
	
						break;
					}
	
					Sun.Statistics.Sending.End();
									
					SendSignal.WaitOne();
				}
			}
			catch(Exception) when(!Debugger.IsAttached)
			{
				Status = ConnectionStatus.Failed;
			}
		}

		void Listening()
		{
	 		try
	 		{
				while(true)
				{
					var pk = (PacketType)Reader.ReadByte();

					lock(Sun.Lock)
						if(Sun.Workflow.IsAborted || !Established)
							return;
					
					Sun.Statistics.Reading.Begin();

					switch(pk)
					{
 						case PacketType.Request:
 						{
							RdcRequest rq;

 							try
 							{
								rq = BinarySerializator.Deserialize<RdcRequest>(Reader,	Sun.Constract,i =>	{
																												if(i is RdcRequest r) 
																													r.Peer = this;
																											});
 							}
 							catch(Exception) when(!DevSettings.ThrowOnCorrupted)
 							{
 								Disconnect();
 								break;
 							}

							lock(InRequests)
 								InRequests.Add(rq);
 	
							SendSignal.Set();

 							break;
 						}

						case PacketType.Response:
 						{
							RdcResponse rp;
							
							try
 							{
								rp = BinarySerializator.Deserialize<RdcResponse>(Reader, Sun.Constract, i => {});
							}
 							catch(Exception) when(!DevSettings.ThrowOnCorrupted)
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

							break;
						}

						default:
							Sun.Workflow.Log?.ReportError(this, $"Wrong packet type {pk}");
							Status = ConnectionStatus.Failed;
							return;
					}

					Sun.Statistics.Reading.End();
				}
	 		}
			catch(Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
			{
				lock(Sun.Lock)
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

			lock(Outs)
				Outs.Enqueue(rq);

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

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();

 			if(rq.WaitResponse)
 			{
	 			if(rq.Event.WaitOne(DevSettings.DisableTimeouts ? Timeout.Infinite : 60*1000)) 
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