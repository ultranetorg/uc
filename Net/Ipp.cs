using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

namespace Uccs.Net;

public abstract class IppPacket
{
	public int					Id { get; set; }
}

public class IppRequest : IppPacket
{
	public Return				Return;
	public ManualResetEvent		Event;
	public CodeException		Exception;
	public Argumentation		Argumentation { get; set; }
}

public class IppConnection
{
	protected IProgram				Program;
	public PipeStream				Pipe;
	public BinaryReader				Reader;
	public BinaryWriter				Writer;
	List<IppRequest>				OutRequests = new();
	IppServer						Server;
	int								IdCounter = 0;
	public Constructor				Constructor;
	public Flow						Flow;

	object							Handler;
	Dictionary<byte, MethodInfo>	Methods = [];

	public IppConnection(IProgram program, NamedPipeServerStream pipe, IppServer server, Flow flow, Constructor constructor)
	{
		Program		= program;
		Pipe		= pipe;
		Server		= server; 
		Constructor	= constructor;
		Flow		= flow.CreateNested();;

		Reader = new BinaryReader(pipe);
		Writer = new BinaryWriter(pipe);
	}

	public IppConnection(IProgram program, string name, Flow flow)
	{
		Program	= program;
		Flow	= flow;

		program.CreateThread(() =>	{ 
										while(Flow.Active)
										{
											try
											{
												var pipe = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
												
												Pipe = pipe;
												IdCounter = 0;
	
												pipe.ConnectAsync(Flow.Cancellation).Wait();
	
												Reader = new BinaryReader(pipe);
												Writer = new BinaryWriter(pipe);
	
												Established();
												Listen();
											}
											catch(AggregateException ex) when(ex.InnerException is TaskCanceledException)
											{
											}
										}
									})
				.Start();
	}

	public virtual void Established()
	{
	}

	public void RegisterHandler(Type enumclass, object handler)
	{
		Handler = handler;

		foreach(var i in handler.GetType().GetMethods().Where(i => i.GetParameters().Length == 2 && i.ReturnType == typeof(Return)))
		{
			if(Enum.TryParse(enumclass, i.Name, out var c))
			{
				Methods[(byte)c] = i;
			}

		}
	}

	void Request(IppRequest request)
	{
		lock(Writer)
		{
			Writer.Write((byte)PacketType.Request);
			Writer.Write(request.Id);
			//Writer.Write(Constructor.TypeToCode(request.Argumentation.GetType()));
			//(request.Argumentation as IBinarySerializable).Write(Writer);
			BinarySerializator.Serialize(Writer, request.Argumentation, Constructor.TypeToCode);
		}
	}

	void Respond(IppRequest request)
	{
		try
		{
			var r = Methods[Constructor.TypeToCode(request.Argumentation.GetType())].Invoke(Handler, [this, request.Argumentation]) as Return;
	
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Response);
				Writer.Write(request.Id);
				//Writer.Write(Constructor.TypeToCode(r.GetType()));
				//(r as IBinarySerializable).Write(Writer); 
				BinarySerializator.Serialize(Writer, r, Constructor.TypeToCode);
			}
		}
		catch(TargetInvocationException ex) when (ex.InnerException is CodeException)
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Failure);
				Writer.Write(request.Id);
				BinarySerializator.Serialize(Writer, ex.InnerException, Constructor.TypeToCode);
			}
		}
	}

	public void Listen()
	{
		try
		{
			while(Pipe.IsConnected && Flow.Active)
			{
				var pk = (PacketType)Reader.ReadByte();

//				if(Pipeing.Flow.Aborted || Status != ConnectionStatus.OK)
//					return;

				switch(pk)
				{
 					case PacketType.Request:
 					{
						var rq = new IppRequest();
						rq.Id = Reader.ReadInt32();
						//rq.Argumentation = Constructor.Construct(typeof(Argumentation), Reader.ReadByte()) as Argumentation;
						//(rq.Argumentation as IBinarySerializable).Read(Reader);
						rq.Argumentation = BinarySerializator.Deserialize<Argumentation>(Reader, Constructor.Construct);
						
						Respond(rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var id = Reader.ReadInt32();
						//var r = Constructor.Construct(typeof(Return), Reader.ReadByte()) as Return;
						//(r as IBinarySerializable).Read(Reader);
						var r = BinarySerializator.Deserialize<Return>(Reader, Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq is IppRequest f)
							{
								f.Return = r;
								f.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

						break;
					}

 					case PacketType.Failure:
 					{
						var id = Reader.ReadInt32();
						var ex = BinarySerializator.Deserialize<CodeException>(Reader, Constructor.Construct);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == id);

							if(rq is IppRequest f)
							{
								f.Exception = ex;
								f.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}


 						break;
 					}
				}
			}
		}
		catch(Exception ex)
		{
		}
		finally
		{
			Disconnect();
		}
	}

///	public void Post(IppProcRequest rq)
///	{
///		if(!Pipe.IsConnected)
///			throw new IpcException(IpcError.ConnectionLost);
///
///		rq.Id = IdCounter++;
///
///		Request(rq);
///	}

	public Return Call(Argumentation argumentation, Flow flow)
	{
		if(!Pipe.IsConnected)
			throw new IpcException(IpcError.ConnectionLost);

		var rq = new IppRequest {Argumentation = argumentation};

		rq.Id = IdCounter++;

		lock(OutRequests)
		{
			rq.Event = new ManualResetEvent(false);
			OutRequests.Add(rq);
		}

		Request(rq);

		int i = -1;

		try
		{
			i = WaitHandle.WaitAny([rq.Event, flow.Cancellation.WaitHandle, Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
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
			if(rq.Exception == null)
			{
				if(rq.Return == null)
					throw new NodeException(NodeError.Connectivity);

				return rq.Return;
			}
			else
			{
				throw rq.Exception;
			}
		}
		else if(i == 1)
			throw new OperationCanceledException();
		else
			throw new IpcException(IpcError.ConnectionLost);
	}

	public void Disconnect()
	{
		Flow.Abort();

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

		Pipe.Dispose();

		if(Server != null)
		{
			lock(Server.Clients)
			{
				Server.Clients.Remove(this);
			}
		}
	}

}

public abstract class IppServer
{
	protected IProgram					Program;
	protected Flow						Flow;
	readonly string						Name;
	public readonly List<IppConnection>	Clients = new();
	public Constructor					Constructor = new();

	public virtual void					Accept(IppConnection connection){}

	public IppServer(IProgram program, string pipeName, Flow flow)
	{
		Program = program;
		Name = pipeName;
		Flow = flow;

		program.CreateThread(() =>	{ 
										while(Flow.Active)
										{
											try
											{
												var pipe = new NamedPipeServerStream(Name,
																					 PipeDirection.InOut,
																					 NamedPipeServerStream.MaxAllowedServerInstances,
																					 PipeTransmissionMode.Byte,
																					 PipeOptions.Asynchronous);
	
												pipe.WaitForConnectionAsync(Flow.Cancellation).Wait();
	
												IppConnection c = CreateConnection(pipe);
	
												lock(Clients)
													Clients.Add(c);
	
												Accept(c);
												program.CreateThread(() => c.Listen()).Start();
											}
											catch(AggregateException ex) when(ex.InnerException is OperationCanceledException)
											{
												break;
											}
										}
									})
									.Start();
	}

	protected virtual IppConnection CreateConnection(NamedPipeServerStream pipe)
	{
		return new IppConnection(Program, pipe, this, Flow, Constructor);
	}
}
