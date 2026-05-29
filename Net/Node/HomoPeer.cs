using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Uccs.Net;

public interface IHomoPeer
{
 	public abstract	void	Send(PeerRequest rq);
	public abstract Result	CallMe(PeerRequest rq, Flow flow);
	public R				CallMe<R>(Ppc<R> rq, Flow flow) where R : Result => CallMe((PeerRequest)rq, flow) as R;
}

public class HomoPeer : Peer, IHomoPeer
{
	public HomoPeer()
	{
	}

	public HomoPeer(Endpoint endpoint)
	{
		EP = endpoint;
	}

	public HomoPeer(IPAddress ip, ushort port)
	{
		EP = new Endpoint(ip, port);
	}

	void Respond(int id, PeerRequest request)
	{
		try
		{
			var r = request.Execute();
				
			if(r != null)
			{
				Write(PacketType.Response, id, r);
			}
		}
		catch(CodeException ex)
		{
			Write(PacketType.Failure, id, ex);
		}
	}

	protected override void Listening()
	{
 		try
 		{
			while(Peering.Flow.Active && Status == ConnectionStatus.OK)
			{
				var pt = Reader.Read<PacketType>();

				if(Peering.Flow.Aborted || Status != ConnectionStatus.OK)
					return;
				
				Peering.Statistics.Reading.Begin();

				switch(pt)
				{
 					case PacketType.Request:
 					{
						var rq = Read<PeerRequest>(out var id);
						rq.Peer = this;
						rq.Peering = Peering as HomoPeering;
						
						Respond(id, rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var r = Read<Result>(out var id);

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
						var ex = Read<CodeException>(out var id);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq?.Event != null)
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
		}
		catch(IntegrityException)
		{
		}

		lock(Peering.Lock)
			Disconnect();
		
		//lock(Sun.Lock)
		//{
		//	ListenThread = null;
		//
		//	if(Status == ConnectionStatus.Disconnecting)
		//	{
		//		Status = ConnectionStatus.Disconnected;
		//	}
		//}
	}

	public void Send(PeerRequest args)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		Write(PacketType.Request, IdCounter++, args);
	}

	public Result CallMe(PeerRequest args, Flow flow)
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

		Write(PacketType.Request, p.Id, args);

		int i = -1;

		try
		{
			i = WaitHandle.WaitAny([p.Event, flow.Cancellation.WaitHandle, Peering.Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
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
				throw p.Exception;
			}
		}
		else if(i == 1 || i == 2)
			throw new OperationCanceledException();
		else
			throw new NodeException(NodeError.Timeout);
	}
}