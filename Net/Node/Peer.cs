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

	public class Peer : IPeer, IBinarySerializable
	{
		public IPAddress								IP {get; set;} 
		public string									Name;

		public ConnectionStatus							Status = ConnectionStatus.Disconnected;

		public bool										Forced;
		public bool										Permanent;
		public bool										Recent;
		int												IdCounter = 0;
		public DateTime									LastSeen = DateTime.MinValue;
		public DateTime									LastTry = DateTime.MinValue;
		public int										Retries;

		public bool										Inbound;
		public string									StatusDescription => Status == ConnectionStatus.OK ? (Inbound ? "Incoming" : "Outbound") : Status.ToString();

		//public Role										Roles => (ChainRank > 0 ? Role.Chain : 0) | (BaseRank > 0 ? Role.Base : 0) | (SeedRank > 0 ? Role.Seed : 0);
		public int										PeerRank = 0;
		public Dictionary<Guid, Dictionary<long, byte>>	Ranks = [];

		public Dictionary<Role, DateTime>				LastFailure = new();

		public Node										Sun;
		TcpClient										Tcp;
		NetworkStream									Stream;
		BinaryWriter									Writer;
		BinaryReader									Reader;
		Thread											ListenThread;
		Thread											SendThread;
		Queue<Packet>									Outs = new();
		public List<PeerRequest>						InRequests = new();
		List<PeerRequest>								OutRequests = new();
		AutoResetEvent									SendSignal = new AutoResetEvent(true);

		public Peer()
		{
		}

		public Peer(IPAddress ip)
		{
			IP = ip;
		}

		public override string ToString()
		{
			return $"{Name}, {IP}, {StatusDescription}, Forced={Forced}, Permanent={Permanent}";
		}
 		
		public byte GetRank(Guid mcvid, long role)
		{
			return Ranks.TryGetValue(mcvid, out var ranks) && ranks.TryGetValue(role, out var v) ? v : (byte)0;
			//throw new IntegrityException("Wrong rank");
		}

  		public void SaveNode(BinaryWriter w)
  		{
  			w.Write7BitEncodedInt64(LastSeen.ToBinary());
			w.Write(PeerRank);
			w.Write(Ranks, i => { w.Write(i.Key); 
								  w.Write(i.Value, j => { w.Write7BitEncodedInt64(j.Key);
														  w.Write(j.Value); });  });
  		}
  
  		public void LoadNode(BinaryReader r)
  		{
  			LastSeen = DateTime.FromBinary(r.Read7BitEncodedInt64());
			PeerRank = r.ReadInt32();
			Ranks = r.ReadDictionary(() => r.ReadGuid(), 
									 () => r.ReadDictionary(() => r.Read7BitEncodedInt64(), 
															() => r.ReadByte()));
  		}
 
 		public void Write(BinaryWriter w)
 		{
 			w.Write(IP);
			w.Write(Ranks, i => { w.Write(i.Key);
								  w.Write7BitEncodedInt64((int)i.Value.Keys.Aggregate(0L, (a, b) => a|b)); });
 		}
 
 		public void Read(BinaryReader reader)
 		{
 			IP = reader.ReadIPAddress();
			Ranks = reader.ReadDictionary(() => reader.ReadGuid(), 
										  () => {
													var r = reader.Read7BitEncodedInt64();
													return Enumerable.Range(0, 64).Select(j => 1L << j).Where(j => j.IsSet(r)).ToDictionary(i => i, i => (byte)1);
												});
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

			lock(InRequests)
				InRequests.Clear();

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
				foreach(var i in Outs.OfType<PeerRequest>())
				{
					if(i.Event != null && !i.Event.SafeWaitHandle.IsClosed)
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

		public void Start(Node sun, TcpClient client, Hello h, string host, bool inbound)
		{
			Sun = sun;
			Tcp = client;
			
			Tcp.ReceiveTimeout = Permanent ? 0 : 60 * 1000;
			Tcp.SendTimeout = NodeGlobals.DisableTimeouts ? 0 : Node.Timeout;

			PeerRank++;
			Name		= h.Name;
			Forced		= false;
			Status		= ConnectionStatus.OK;
			Inbound		= inbound;
			Stream		= client.GetStream();
			Writer		= new BinaryWriter(Stream);
			Reader		= new BinaryReader(Stream);
			LastSeen	= DateTime.UtcNow;
			Ranks		= h.Roles.ToDictionary(i => i.Key, i => Enumerable.Range(0, 64).Select(j => 1L << j).Where(j => j.IsSet(i.Value)).ToDictionary(i => i, i => (byte)1));

			sun.UpdatePeers([this]);

			ListenThread = Sun.CreateThread(Listening);
			ListenThread.Name = $"{host} <- {h.Name}";
			ListenThread.Start();
	
			SendThread = Sun.CreateThread(Sending);
			SendThread.Name = $"{host} -> {h.Name}";
			SendThread.Start();
		}

		void Sending()
		{
			try
			{
				while(Sun.Flow.Active && Status == ConnectionStatus.OK)
				{
					Sun.Statistics.Sending.Begin();
	
					PeerRequest[] inrq;
	
					lock(InRequests)
					{
						inrq = InRequests.ToArray();
						InRequests.Clear();
					}
							
					foreach(var i in inrq)
					{
// 						if(i is ProxyRequest px)
// 						{
// 							Task.Run(() =>	{
// 												var rp = i.SafeExecute(Sun);
// 
// 												if(i.WaitResponse)
// 													lock(Outs)
// 														Outs.Enqueue(rp);
// 
// 												SendSignal.Set();
// 											});
// 						}
// 						else
						{
							var rp = i.SafeExecute();

							if(i.WaitResponse)
								lock(Outs)
									Outs.Enqueue(rp);
						}
					}

					lock(Outs)
					{
						foreach(var i in Outs)
						{
							if(i is PeerRequest)
								Writer.Write((byte)PacketType.Request);
							else if(i is PeerResponse)
								Writer.Write((byte)PacketType.Response);
							else
								throw new IntegrityException("Wrong packet to write");
	
							BinarySerializator.Serialize(Writer, i);
						}
						
						Outs.Clear();
					}
	
					Sun.Statistics.Sending.End();
					
					WaitHandle.WaitAny([SendSignal, Sun.Flow.Cancellation.WaitHandle]);
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
				while(Sun.Flow.Active && Status == ConnectionStatus.OK)
				{
					var pk = (PacketType)Reader.ReadByte();

					if(Sun.Flow.Aborted || Status != ConnectionStatus.OK)
						return;
					
					Sun.Statistics.Reading.Begin();

					switch(pk)
					{
 						case PacketType.Request:
 						{
							var rq = BinarySerializator.Deserialize<PeerRequest>(Reader,	Sun.Constract);
							rq.Peer = this;
							rq.Sun = Sun;
							rq.Mcv = Sun.FindMcv(rq.McvId);

							lock(InRequests)
 								InRequests.Add(rq);

							#if DEBUG
							if(InRequests.Count > 100)
							{
								///Debugger.Break();
							}
							#endif
 	
							SendSignal.Set();

 							break;
 						}

						case PacketType.Response:
 						{
							var rp = BinarySerializator.Deserialize<PeerResponse>(Reader, Sun.Constract);

							lock(OutRequests)
							{
								var rq = OutRequests.Find(i => i.Id == rp.Id);

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

 		public override void Post(PeerRequest rq)
 		{
			if(Status != ConnectionStatus.OK)
				throw new NodeException(NodeError.Connectivity);

			rq.Id = IdCounter++;

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();
 		}

 		public override PeerResponse Send(PeerRequest rq)
 		{
			if(Status != ConnectionStatus.OK)
				throw new NodeException(NodeError.Connectivity);

			rq.Id = IdCounter++;

			if(rq.WaitResponse)
				lock(OutRequests)
				{
					rq.Event = new ManualResetEvent(false);
					OutRequests.Add(rq);
				}

			lock(Outs)
				Outs.Enqueue(rq);

			SendSignal.Set();

 			if(rq.WaitResponse)
 			{
				int i = -1;

				try
				{
					i = WaitHandle.WaitAny([rq.Event, Sun.Flow.Cancellation.WaitHandle], NodeGlobals.DisableTimeouts ? Timeout.Infinite : 60*1000);
				}
				catch(ObjectDisposedException)
				{
					throw new OperationCanceledException();
				}
				finally
				{
					rq.Event.Close();
				}
	
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
							if(e.Error == NodeError.NotBase)	Ranks[rq.Mcv.Guid][(long)Role.Base] = 0;
							if(e.Error == NodeError.NotChain)	Ranks[rq.Mcv.Guid][(long)Role.Chain] = 0;
							if(e.Error == NodeError.NotSeed)	Ranks[rq.Mcv.Guid][(long)RdnRole.Seed] = 0;
						}
	
						throw rq.Response.Error;
					}
	 			}
		 		else if(i == 1)
					throw new OperationCanceledException();
				else
		 			throw new NodeException(NodeError.Timeout);
 			}
			else
				return null;
 		}
	}
}