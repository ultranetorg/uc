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
		None, Request, Response
	}

	//public enum EstablishingStatus
	//{
	//	Failed = -1, Null = 0, Initiated = 1, Succeeded = 2, 
	//}

	public enum ConnectionStatus
	{
		None, Disconnected, Initiated, OK, Failed, Disconnecting
	}

	public class Peer : RdcInterface
	{
		public IPAddress			IP {get; set;} 

		public ConnectionStatus		Status = ConnectionStatus.Disconnected;

		public bool					Forced;
		public bool					Permanent;
		public DateTime				LastSeen = DateTime.MinValue;
		public DateTime				LastTry = DateTime.MinValue;
		public int					Retries;

		public bool					Inbound;
		public string				StatusDescription => Status == ConnectionStatus.OK ? (Inbound ? "Incoming" : "Outbound") : Status.ToString();

		public Role					Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0) | (SeedRank > 0 ? Role.Seed : 0);
		public int					PeerRank = 0;
		public int					ChainRank = 0;
		public int					BaseRank = 0;
		public int					SeedRank = 0;

		public Dictionary<Role, DateTime>	LastFailure = new();

		Sun							Sun;
		TcpClient					Tcp;
		NetworkStream				Stream;
		BinaryWriter				Writer;
		BinaryReader				Reader;
		Thread						ListenThread;
		Thread						SendThread;
		Queue<RdcPacket>			Outs = new();
		public List<RdcRequest>		InRequests = new();
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
			return $"{IP}, {StatusDescription}, Forced={Forced}, Permanent={Permanent}";
		}
 		
		public int GetRank(Role role)
		{
			if(role == Role.Base) return BaseRank;
			if(role == Role.Chain) return ChainRank;
			if(role == Role.Seed) return SeedRank;

			throw new IntegrityException("Wrong rank");
		}

  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write7BitEncodedInt64(LastSeen.ToBinary());
			w.Write(PeerRank);
			w.Write(ChainRank);
			w.Write(SeedRank);
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
			PeerRank = r.ReadInt32();
			ChainRank = r.ReadInt32();
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
			if(Status != ConnectionStatus.Disconnecting)
				Status = ConnectionStatus.Disconnecting;
			else
				return;

			Forced = false;

			foreach(var i in OutRequests)
				i.Event.Set();

			lock(OutRequests)
				OutRequests.Clear();
	
			if(Tcp != null)
			{
				Stream.Close();
				Tcp.Close();
				Tcp = null;

				SendSignal.Set();

				//Monitor.Exit(Sun.Lock);
				//WriteThread.Join();
				//ReadThread.Join();
				//Monitor.Enter(Sun.Lock);

			}

			if(SendThread == null && ListenThread == null)
			{
				//InStatus = EstablishingStatus.Null;
				//OutStatus = EstablishingStatus.Null;

				Status = ConnectionStatus.Disconnected;
			}
		}

		public void Start(Sun sun, TcpClient client, Hello h, string host, bool inbound)
		{
			Sun = sun;
			Tcp = client;
			
			Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
			Tcp.SendTimeout = DevSettings.DisableTimeouts ? 0 : Sun.Timeout;

			PeerRank++;
			Forced		= false;
			Status		= ConnectionStatus.OK;
			Inbound		= inbound;
			Stream		= client.GetStream();
			Writer		= new BinaryWriter(Stream);
			Reader		= new BinaryReader(Stream);
			LastSeen	= DateTime.UtcNow;
			BaseRank	= h.Roles.HasFlag(Role.Base)	? 1 : 0;
			ChainRank	= h.Roles.HasFlag(Role.Chain)	? 1 : 0;
			SeedRank	= h.Roles.HasFlag(Role.Seed)	? 1 : 0;

			sun.UpdatePeers(new Peer[]{this});

			ListenThread = new (() => Listening());
			ListenThread.Name = $"{host} <- {IP.GetAddressBytes()[3]}";
			ListenThread.Start();
	
			SendThread = new (() => Sending());
			SendThread.Name = $"{host} -> {IP.GetAddressBytes()[3]}";
			SendThread.Start();
		}

		void Sending()
		{
			try
			{
				while(Sun.Workflow.Active && Status == ConnectionStatus.OK)
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
									r = RdcResponse.FromType(i.Class);
									r.Result = RdcResult.NodeException;
									r.Error = (byte)ex.Error;
									r.ErrorDetails = ex.ToString();
								}
								catch(RdcEntityException ex)
								{
									r = RdcResponse.FromType(i.Class);
									r.Result = RdcResult.EntityException;
									r.Error = (byte)ex.Error;
									r.ErrorDetails = ex.ToString();
								}
								catch(Exception ex) when(!Debugger.IsAttached)
								{
									r = RdcResponse.FromType(i.Class);
									r.Result = RdcResult.NodeException;
									r.Error = (byte)RdcNodeError.Internal;
									r.ErrorDetails = ex.ToString();
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
								catch(Exception ex) when(!Debugger.IsAttached || ex is RdcEntityException || ex is RdcNodeException)
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
	
					Sun.Statistics.Sending.End();
					
					WaitHandle.WaitAny(new[] {SendSignal, Sun.Workflow.Cancellation.WaitHandle});
				}
			}
			catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
			{
				lock(Sun.Lock)
					if(Status != ConnectionStatus.Disconnecting)
						Status = ConnectionStatus.Failed;
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
				while(Sun.Workflow.Active && Status == ConnectionStatus.OK)
				{
					var pk = (PacketType)Reader.ReadByte();
					
					Sun.Statistics.Reading.Begin();

					switch(pk)
					{
 						case PacketType.Request:
 						{
							var rq = BinarySerializator.Deserialize<RdcRequest>(Reader,	Sun.Constract,i =>	{
																												if(i is RdcRequest r) 
																													r.Peer = this;
																											});
							lock(InRequests)
 								InRequests.Add(rq);
 	
							SendSignal.Set();

 							break;
 						}

						case PacketType.Response:
 						{
							var rp = BinarySerializator.Deserialize<RdcResponse>(Reader, Sun.Constract, i => {});

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
			catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
			{
				lock(Sun.Lock)
					if(Status != ConnectionStatus.Disconnecting)
						Status = ConnectionStatus.Failed;
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

 		public override void Send(RdcRequest rq)
 		{
			if(Status != ConnectionStatus.OK)
				throw new RdcNodeException(RdcNodeError.Connectivity);

			rq.Id = IdCounter++;

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();
 		}

 		public override RdcResponse Request(RdcRequest rq)
 		{
			if(Status != ConnectionStatus.OK)
				throw new RdcNodeException(RdcNodeError.Connectivity);

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
					{
						if(rq.Response == null)
							rq=rq;

						return rq.Response;
					}
	 				else if(rq.Response.Result == RdcResult.NodeException)
					{
						var e = (RdcNodeError)rq.Response.Error;
						
						if(e.HasFlag(RdcNodeError.NotBase))
							BaseRank = 0;

						if(e.HasFlag(RdcNodeError.NotChain))
							ChainRank = 0;

						if(e.HasFlag(RdcNodeError.NotSeed))
							SeedRank = 0;

						throw new RdcNodeException(e, rq.Response.ErrorDetails);
					}
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