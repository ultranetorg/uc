using System.Diagnostics;
using System.Net.Sockets;

namespace Uccs.Net;

public class IccpRequestPacket: RequestPacket
{
	public IccpArgumentation	Argumentation { get ; set; }
}

public class IccpPeer : Peer, IBinarySerializable
{
	public List<string>		Nets;

	public IccpPeer()
	{
	}

	public IccpPeer(Endpoint endpoint)
	{
		EP = endpoint;
	}

	public override string ToString()
	{
		return $"{Name}, {EP}, {StatusDescription}, Permanent={Permanent}, Roles={Roles}, Forced={Forced}";
	}
 	
	void Request(string from, string to, int id, IccpArgumentation packet)
	{
		lock(Writer)
		{
			Writer.Write(PacketType.Request);
			Writer.WriteASCII(from);
			Writer.WriteASCII(to);
			Writer.Write(id);
			
			WriteStream.SetLength(0);
			PacketWriter.WriteVirtual(packet);

			if(WriteStream.Length > PacketLengthMaximum)
				throw new IntegrityException("PacketLengthMaximum exceeded");
			
			Writer.Write((int)WriteStream.Length);
			Writer.Write(new ReadOnlySpan<byte>(WriteStream.GetBuffer(), 0, (int)WriteStream.Length));
		}
	}

	T ReadRequest<T>(out string from, out string to, out int id) where T : class, IBinarySerializable, ITypeCode
	{
		from = Reader.ReadASCII();
		to	 = Reader.ReadASCII();
		id	 = Reader.ReadInt32();
		var l = Reader.ReadInt32();

		if(l > ReadBuffer.Length)
			throw new IntegrityException("PacketLengthMaximum exceeded");

		Stream.Read(ReadBuffer, 0, l);
		ReadStream.Position = 0;

		return PacketReader.ReadVirtual<T>();
	}

	protected override void Listening()
	{
 		try
 		{
			while(Peering.Flow.Active && Status == ConnectionStatus.OK)
			{
				var pk = Reader.Read<PacketType>();

				if(Peering.Flow.Aborted || Status != ConnectionStatus.OK)	
					return;
				
				Peering.Statistics.Reading.Begin();

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var rq = ReadRequest<IccpArgumentation>(out var from, out var to, out var id);
						
						try
						{
							var r = (Peering as IccpPeering).Lcp.Relay(from, to, rq, null, Peering.Flow);

							if(r != null)
							{
								Write(PacketType.Response, id, r);
							}
						}
						catch(CodeException ex)
						{
							Write(PacketType.Failure, id, ex);
						}

 						break;
 					}

					case PacketType.Response:
 					{
						var r = Read<IccpResult>(out var id);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq.Event != null)
							{
								rq.Result = r;
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

			if(Status == ConnectionStatus.Disconnecting)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	public void Send(string from, string to, IccpArgumentation args)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		Request(from, to, IdCounter++, args);
	}

	public IccpResult Call(string from, string to, IccpArgumentation args, Flow flow)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		var p = new IccpRequestPacket();

		p.Argumentation = args;
		p.Id = IdCounter++;

		lock(OutRequests)
		{
			p.Event = new ManualResetEvent(false);
			OutRequests.Add(p);
		}

		Request(from, to, p.Id, args);

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
				if(p.Result == null)
					throw new NodeException(NodeError.Connectivity);

				return p.Result as IccpResult;
			}
			else
			{
				///if(p.Exception is NodeException e)
				///{
				///	Peering.OnRequestException(this, e);
				///}

				throw p.Exception;
			}
		}
		else if(i == 1 || i == 2)
			throw new OperationCanceledException();
		else
			throw new NodeException(NodeError.Timeout);
	}
}