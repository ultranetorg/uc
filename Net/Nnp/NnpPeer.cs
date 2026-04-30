using System.Diagnostics;
using System.Net.Sockets;

namespace Uccs.Net;

public class NnRequestPacket: RequestPacket
{
	public Argumentation	Argumentation { get ; set; }
}

public class NnpPeer : Peer, IBinarySerializable
{
	LcpServer				Lcp;
	public List<string>		Nets;

	public NnpPeer()
	{
	}

	public NnpPeer(Endpoint endpoint)
	{
		EP = endpoint;
	}

	public override string ToString()
	{
		return $"{Name}, {EP}, {StatusDescription}, Permanent={Permanent}, Roles={Roles}, Forced={Forced}";
	}
 		
	void Request(string from, string to, int id, Argumentation request)
	{
		try
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Request);
				Writer.WriteASCII(from);
				Writer.WriteASCII(to);
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
						var from = Reader.ReadASCII();
						var to = Reader.ReadASCII();
						var id = Reader.ReadInt32();
						var rq = BinarySerializator.Deserialize<Argumentation>(Reader, Peering.Constructor.Construct);
						
						try
						{
							var r = Lcp.Relay(from, to, rq as NnpArgumentation);

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

 						break;
 					}

					case PacketType.Response:
 					{
						var id = Reader.ReadInt32();
						var r = BinarySerializator.Deserialize<Result>(Reader, Peering.Constructor.Construct);

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

			if(Status == ConnectionStatus.Disconnecting)
			{
				Status = ConnectionStatus.Disconnected;
			}
		}
	}

	public void Send(string from, string to, Argumentation args)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		Request(from, to, IdCounter++, args);
	}

	public Result Call(string from, string to, Argumentation args, Flow flow)
	{
		if(Status != ConnectionStatus.OK)
			throw new NodeException(NodeError.Connectivity);

		var p = new NnRequestPacket();

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
				if(p.Return == null)
					throw new NodeException(NodeError.Connectivity);

				return p.Return;
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