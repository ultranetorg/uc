using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		None, Disconnected, Initiated, OK, Disconnecting
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

			lock(OutRequests)
			{
				foreach(var i in OutRequests)
				{
					if(!i.Event.SafeWaitHandle.IsClosed)
					{
						i.Event.Set();
						i.Event.Close();
					}
				}

				OutRequests.Clear();
			}

			lock(Outs)
			{
				foreach(var i in Outs.OfType<RdcRequest>())
				{
					if(!i.Event.SafeWaitHandle.IsClosed)
					{
						i.Event.Set();
						i.Event.Close();
					}
				}

				Outs.Clear();
			}
	
			if(Tcp != null)
			{
				Stream.Close();
				Tcp.Close();
				Tcp = null;

				SendSignal.Set();
			}

			if(SendThread == null && ListenThread == null)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}

		public void Start(Sun sun, TcpClient client, Hello h, string host, bool inbound)
		{
			Sun = sun;
			Tcp = client;
			
			Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
			Tcp.SendTimeout = SunGlobals.DisableTimeouts ? 0 : Sun.Timeout;

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
	
					RdcRequest[] inrq;
	
					lock(InRequests)
					{
						inrq = InRequests.ToArray();
						InRequests.Clear();
					}
							
					foreach(var i in inrq)
					{
						if(i is ProxyRequest px)
						{
							Task.Run(() =>	{
												var rp = i.SafeExecute(Sun);

												if(i.WaitResponse)
													lock(Outs)
														Outs.Enqueue(rp);

												SendSignal.Set();
											});
						}
						else
						{
							var rp = i.SafeExecute(Sun);

							if(i.WaitResponse)
								lock(Outs)
									Outs.Enqueue(rp);
						}
					}

					//RdcPacket[] outs;
	
					lock(Outs)
					{
						//outs = Outs.ToArray();

						foreach(var i in Outs)
						{
							if(i is RdcRequest)
								Writer.Write((byte)PacketType.Request);
							else if(i is RdcResponse)
								Writer.Write((byte)PacketType.Response);
							else
								throw new IntegrityException("Wrong packet to write");
	
							BinarySerializator.Serialize(Writer, i);
						}
						
						Outs.Clear();
					}
	
					Sun.Statistics.Sending.End();
					
					WaitHandle.WaitAny(new[] {SendSignal, Sun.Workflow.Cancellation.WaitHandle});
				}
			}
			catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
			{
				lock(Sun.Lock)
					Disconnect();
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

					if(Sun.Workflow.Aborted || Status != ConnectionStatus.OK)
						return;
					
					Sun.Statistics.Reading.Begin();

					switch(pk)
					{
 						case PacketType.Request:
 						{
							var rq = BinarySerializator.Deserialize<RdcRequest>(Reader,	Sun.Constract);
							rq.Peer = this;

							lock(InRequests)
 								InRequests.Add(rq);
 	
							SendSignal.Set();

 							break;
 						}

						case PacketType.Response:
 						{
							var rp = BinarySerializator.Deserialize<RdcResponse>(Reader, Sun.Constract);

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
					}

					Sun.Statistics.Reading.End();
				}
	 		}
			catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
			{
				lock(Sun.Lock)
					Disconnect();
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
				throw new NodeException(NodeError.Connectivity);

			rq.Id = IdCounter++;

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();
 		}

 		public override RdcResponse Request(RdcRequest rq)
 		{
			if(Status != ConnectionStatus.OK)
				throw new NodeException(NodeError.Connectivity);

			rq.Id = IdCounter++;

			if(rq.WaitResponse)
				lock(OutRequests)
					OutRequests.Add(rq);

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();

 			if(rq.WaitResponse)
 			{
				var i = WaitHandle.WaitAny(new WaitHandle[] {rq.Event, Sun.Workflow.Cancellation.WaitHandle}, SunGlobals.DisableTimeouts ? Timeout.Infinite : 60*1000);

				rq.Event.Close();

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
							if(e.Error == NodeError.NotBase)
								BaseRank = 0;
	
							if(e.Error == NodeError.NotChain)
								ChainRank = 0;
	
							if(e.Error == NodeError.NotSeed)
								SeedRank = 0;
						}

						throw rq.Response.Error;
					}
 				}
	 			if(i == 1)
					throw new OperationCanceledException();
				else
	 				throw new NodeException(NodeError.Timeout);
 			}
			else
				return null;
 		}

 		public override RdcResponse SafeRequest(RdcRequest rq)
 		{
 			if(Status != ConnectionStatus.OK)
			{
				rq.Response.Error = new NodeException(NodeError.Connectivity);
				return rq.Response;
			}
 
 			rq.Id = IdCounter++;
 
 			if(rq.WaitResponse)
 				lock(OutRequests)
 					OutRequests.Add(rq);
 
 			lock(Outs)
 				Outs.Enqueue(rq);
 
 			SendSignal.Set();
 
  			if(rq.WaitResponse)
  			{
				var i = WaitHandle.WaitAny(new WaitHandle[] {rq.Event, Sun.Workflow.Cancellation.WaitHandle}, SunGlobals.DisableTimeouts ? Timeout.Infinite : 60*1000);

				rq.Event.Close();

	 			if(i == 0)
	 			{
					if(rq.Response == null)
					{
						rq.Response.Error = new NodeException(NodeError.Connectivity);
						return rq.Response;
					}
	
	 				if(rq.Response.Error == null)
					{
						return rq.Response;
					}
	 				else 
					{
						if(rq.Response.Error is NodeException e)
						{
							if(e.Error == NodeError.NotBase)
								BaseRank = 0;
	
							if(e.Error == NodeError.NotChain)
								ChainRank = 0;
	
							if(e.Error == NodeError.NotSeed)
								SeedRank = 0;
						}

						return rq.Response;
					}
 				}
	 			else if(i == 1)
					throw new OperationCanceledException();
 				else
				{
					rq.Response.Error = new NodeException(NodeError.Timeout);
					return rq.Response;
				}
  			}
 			else
 				return null;
 		}
	}
}