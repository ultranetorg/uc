using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

public interface IHomoPeer
{
 	public abstract	void			Send(PeerRequest rq);
	public abstract Return			Call(PeerRequest rq);
	public R						Call<R>(Ppc<R> rq) where R : Return => Call((PeerRequest)rq) as R;
}

public class HomoPeer : Peer, IHomoPeer
{
	public HomoPeer()
	{
	}

	public HomoPeer(IPAddress ip, ushort port)
	{
		IP = ip;
		Port = port;
	}

	void Request(int id, PeerRequest request)
	{
		try
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Request);
				Writer.Write(id);
				BinarySerializator.Serialize(Writer, request, Peering.Constructor.TypeToCode); 
			}
		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();

			throw new OperationCanceledException();
		}
	}

	void Respond(int id, PeerRequest request)
	{
		try
		{
			try
			{
				var r =  request.Execute();
				
				if(r != null)
				{
					lock(Writer)
					{
						Writer.Write((byte)PacketType.Response);
						Writer.Write(id);
						BinarySerializator.Serialize(Writer, r, Peering.Constructor.TypeToCode);
					}
				}
			}
			catch(CodeException ex)
			{
				lock(Writer)
				{
					Writer.Write((byte)PacketType.Failure);
					Writer.Write(id);
					BinarySerializator.Serialize(Writer, ex, Peering.Constructor.TypeToCode);
				}
			}
			catch(Exception) when(!Debugger.IsAttached)
			{
				lock(Writer)
				{
					Writer.Write((byte)PacketType.Failure);
					Writer.Write(id);
					BinarySerializator.Serialize(Writer, new NodeException(NodeError.Unknown), Peering.Constructor.TypeToCode);
				}
			}
		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();

			throw new OperationCanceledException();
		}

	}

	protected override void Listening()
	{
 		try
 		{
			while(Peering.Flow.Active && Status == ConnectionStatus.OK)
			{
				var pk = (PacketType)Reader.ReadByte();

				if(Peering.Flow.Aborted || Status != ConnectionStatus.OK)
					return;
				
				Peering.Statistics.Reading.Begin();

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var id = Reader.ReadInt32();
						var rq = BinarySerializator.Deserialize<PeerRequest>(Reader, Peering.Constructor.Construct);
						rq.Peer = this;
						rq.Peering = Peering as HomoTcpPeering;
						
						Respond(id, rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var id = Reader.ReadInt32();
						var r = BinarySerializator.Deserialize<Return>(Reader, Peering.Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq.Event != null)
							{
								rq.Return = r;
								rq.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

						break;
					}

 					case PacketType.Failure:
 					{
						var id = Reader.ReadInt32();
						var ex = BinarySerializator.Deserialize<CodeException>(Reader, Peering.Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq.Event != null)
							{
								rq.Exception = ex;
								rq.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

 						break;
 					}
				}

				Peering.Statistics.Reading.End();
			}
 		}
		catch(Exception ex) when(ex is SocketException || ex is IOException || ex is ObjectDisposedException || !Debugger.IsAttached)
		{
			lock(Peering.Lock)
				Disconnect();
		}

		//lock(Sun.Lock)
		{
			ListenThread = null;

			if(Status == ConnectionStatus.Disconnecting && ListenThread == null)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	public void Send(PeerRequest args)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		Request(IdCounter++, args);
	}

	public Return Call(PeerRequest args)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		var p = new HomoRequestPacket();

		p.Request = args;
		p.Id = IdCounter++;

		lock(OutRequests)
		{
			p.Event = new ManualResetEvent(false);
			OutRequests.Add(p);
		}

		Request(p.Id, args);

		int i = -1;

		try
		{
			i = WaitHandle.WaitAny([p.Event, Peering.Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
		}
		catch(ObjectDisposedException)
		{
			throw new OperationCanceledException();
		}
		finally
		{
			p.Event.Close();
		}

		if(i == 0)
		{
			if(p.Exception == null)
			{
				if(p.Return == null)
					throw new NodeException(NodeError.Connectivity);

				return p.Return;
			}
			else
			{

				if(p.Exception is NodeException e)
				{
					Peering.OnRequestException(this, e);
				}

				throw p.Exception;
			}
		}
		else if(i == 1)
			throw new OperationCanceledException();
		else
			throw new NodeException(NodeError.Timeout);
	}
}