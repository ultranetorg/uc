using System.Diagnostics;
using System.IO.Pipes;

namespace Uccs.Net;

//public interface IIpp
//{
// 	public void			Post(ProcIpc rq);
//	public IppResponse	Send(FuncIpc rq);
//	public Rp			Send<Rp>(Ipc<Rp> rq) where Rp : IppResponse => Send((FuncIpc)rq) as Rp;
//}

public abstract class IppPacket : ITypeCode
{
	public int				Id { get; set; }
	public IpcConnection	Connection;
	public object			Owner;
}

public abstract class IppRequest : IppPacket
{
}

public abstract class IppFuncRequest : IppRequest
{
	public IppResponse			Response;
	public ManualResetEvent		Event;

	public abstract IppResponse	Execute(Flow flow);

	public IppResponse SafeExecute(Flow flow)
	{
		IppResponse rp;

		try
		{
			rp = Execute(flow);
		}
		catch(CodeException ex)
		{
			rp = Connection.Constuctor.Constract(typeof(IppResponse), Connection.Constuctor.TypeToCode(GetType())) as IppResponse;
			rp.Error = ex;
		}
		catch(Exception) when(!Debugger.IsAttached)
		{
			rp = Connection.Constuctor.Constract(typeof(IppResponse), Connection.Constuctor.TypeToCode(GetType())) as IppResponse;
			rp.Error = new IpcException(IpcError.Unknown);
		}

		rp.Id = Id;

		return rp;
	}

}

public abstract class IppProcRequest : IppRequest
{
	public abstract void		Execute(Flow flow);

	public void SafeExecute(Flow flow)
	{
		try
		{
			Execute(flow);
		}
		catch(Exception ex) when(!Debugger.IsAttached || ex is CodeException)
		{
		}
	}

}

public abstract class IppResponse : IppPacket
{
	public CodeException	Error { get; set; }
}

public abstract class Ipc<R> : IppFuncRequest where R : IppResponse /// Pipe-to-Pipe Call
{
}

public enum NnIpcClass : byte
{
	None = 0, 
	HolderClasses
}

public class HolderClassesNnIpc : IppFuncRequest
{
	public string	Net { get; set; }
	public byte[]	Request { get; set; }

	public override IppResponse Execute(Flow flow)
	{
		throw new NotImplementedException();
	}
}

public class HolderClassesNnIpr : IppResponse
{
	public string[]  Classes { get; set; }
}

public class IpcConnection //: IIpp
{
	//public Channel<byte[]> Outbox { get; } = Channel.CreateUnbounded<byte[]>();

	IProgram			Program;
	public PipeStream	Pipe;
	BinaryReader		Reader;
	BinaryWriter		Writer;
	IpcServer			Server;
	List<IppRequest>	OutRequests = new();
	Flow				Flow;
	int					IdCounter = 0;
	public Constructor	Constuctor;

	internal IpcConnection(IProgram program, NamedPipeServerStream pipe, IpcServer server, Flow flow, Constructor constructor)
	{
		Program		= program;
		Pipe		= pipe;
		Server		= server; 
		Flow		= flow; 
		Constuctor	= constructor;

		Reader = new BinaryReader(pipe);
		Writer = new BinaryWriter(pipe);
	}

	public IpcConnection(IProgram program, string name, Flow flow)
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
	
												pipe.ConnectAsync(Flow.Cancellation).Wait();
	
												Reader = new BinaryReader(pipe);
												Writer = new BinaryWriter(pipe);
	
												Listen();
											}
											catch(AggregateException ex) when(ex.InnerException is TaskCanceledException)
											{
											}
										}
									})
				.Start();
	}

	void Request(IppRequest i)
	{
		try
		{
			lock(Writer)
			{
				Writer.Write((byte)PacketType.Request);
				BinarySerializator.Serialize(Writer, i, Constuctor.TypeToCode); 
			}
		}
		catch when(!Debugger.IsAttached)
		{
			Server?.Disconnect(this);

			throw new OperationCanceledException();
		}
	}

	void Respond(IppRequest i)
	{
		try
		{
			if(i is IppFuncRequest f)
			{
				var rp = f.SafeExecute(Flow);
							
				lock(Writer)
				{
					Writer.Write((byte)PacketType.Response);
					BinarySerializator.Serialize(Writer, rp, Constuctor.TypeToCode); 
				}
			}
			else
				(i as IppProcRequest).SafeExecute(Flow);
		}
		catch when(!Debugger.IsAttached)
		{
			Server?.Disconnect(this);

			throw new OperationCanceledException();
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
						var rq = BinarySerializator.Deserialize<IppRequest>(Reader, Constuctor.Constract);
						rq.Connection = this;
						
						Respond(rq);

 						break;
 					}

					case PacketType.Response:
 					{
						var rp = BinarySerializator.Deserialize<IppResponse>(Reader, Constuctor.Constract);

						lock(OutRequests)
						{
							var rq = OutRequests.Find(i => i.Id == rp.Id);

							if(rq is IppFuncRequest f)
							{
								rp.Connection = this;
								f.Response = rp;
								f.Event.Set();
 									
								OutRequests.Remove(rq);
							}
						}

						break;
					}
				}
			}
		}
		catch
		{
		}
		finally
		{
			Server.Disconnect(this);
		}
	}

	public void Post(IppProcRequest rq)
	{
		if(!Pipe.IsConnected)
			throw new IpcException(IpcError.ConnectionLost);

		rq.Id = IdCounter++;

		Request(rq);
	}

	public IppResponse Send(IppFuncRequest rq)
	{
		if(!Pipe.IsConnected)
			throw new IpcException(IpcError.ConnectionLost);

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
			i = WaitHandle.WaitAny([rq.Event, Flow.Cancellation.WaitHandle], NodeGlobals.InfiniteTimeouts ? Timeout.Infinite : 10 * 1000);
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
				throw new IpcException(IpcError.ConnectionLost);

			if(rq.Response.Error == null)
			{
				return rq.Response;
			}
			else
			{
				throw rq.Response.Error;
			}
		}
		else if(i == 1)
			throw new OperationCanceledException();
		else
			throw new IpcException(IpcError.ConnectionLost);
	}

	public Rp Send<Rp>(Ipc<Rp> rq) where Rp : IppResponse => Send((IppFuncRequest)rq) as Rp;

}

public class IpcServer
{
	IProgram						Program;
	Flow							Flow;
	readonly string					Name;
	readonly List<IpcConnection>	Clients = new();
	public Constructor				Constuctor = new();

	public IpcServer(IProgram program, string pipeName, Flow flow)
	{
		Program = program;
		Name = pipeName;
		Flow = flow;

		program.CreateThread(() =>	{ 
										while(Flow.Active)
										{
											var pipe = new NamedPipeServerStream(Name,
																				 PipeDirection.InOut,
																				 NamedPipeServerStream.MaxAllowedServerInstances,
																				 PipeTransmissionMode.Byte,
																				 PipeOptions.Asynchronous);

											pipe.WaitForConnectionAsync(Flow.Cancellation).Wait();

											var c = new IpcConnection(program, pipe, this, Flow, Constuctor);
		
											lock(Clients)
												Clients.Add(c);

											c.Listen();
										}
									})
			.Start();
	}

	public void Disconnect(IpcConnection client)
	{
		lock(Clients)
		{
			Clients.Remove(client);
			client.Pipe.Dispose();
		}
	}
}
