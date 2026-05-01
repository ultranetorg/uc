using System.IO.Pipes;

namespace Uccs.Net;

public abstract class LcpPacket
{
	public int					Id { get; set; }
}

public class LcpRequest : LcpPacket
{
	public Result				Return;
	public ManualResetEvent		Event;
	public CodeException		Exception;
	public Argumentation		Argumentation { get; set; }
}

public abstract class LcpConnection
{
	protected IProgram				Program;
	public PipeStream				Pipe;
	public Reader					Reader;
	public Writer					Writer;
	protected List<LcpRequest>		OutRequests = new();
	protected LcpServer				Server;
	protected int					IdCounter = 0;
	public Constructor				Constructor;
	protected Flow					Flow;

	public Func<string, string, NnpArgumentation, Result>	Handler;

	public abstract void			Listen();
	public abstract void			BuildConstructor();

	public LcpConnection(IProgram program, NamedPipeServerStream pipe, LcpServer server, Constructor constructor, Flow flow)
	{
		Program		= program;
		Pipe		= pipe;
		Server		= server; 
		Constructor = constructor;
		Flow		= flow.CreateNested();;

		Reader = new Reader(Pipe, Constructor);
		Writer = new Writer(Pipe, Constructor);
	}

	public LcpConnection(IProgram program, string name, Flow flow)
	{
		Program	= program;
		Flow	= flow.CreateNested(name);

		BuildConstructor();

		var t = program.CreateThread(() =>	{ 
												while(Flow.Active)
												{
													try
													{
														var pipe = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
												
														Pipe = pipe;
														IdCounter = 0;
	
														pipe.ConnectAsync(Flow.Cancellation).Wait();
	
														Reader = new Reader(pipe, Constructor);
														Writer = new Writer(pipe, Constructor);
	
														Established();
														Listen();
													}
													catch(AggregateException ex) when(ex.InnerException is TaskCanceledException)
													{
														return;
													}
												}
											});
		t.Name = name;
		t.Start();
	}

	public virtual void Established()
	{
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

		Pipe.Close();

		if(Server != null)
		{
			lock(Server.Clients)
			{
				Server.Clients.Remove(this);
			}
		}
	}
}

public abstract class LcpServer
{
	protected IProgram					Program;
	protected Flow						Flow;
	readonly string						Name;
	public readonly List<LcpConnection>	Clients = new();
	public Constructor					Constructor = new();

	public virtual void					Accept(LcpConnection connection){}
	public abstract Result				Relay(string from, string to, NnpArgumentation call);

	protected abstract LcpConnection	CreateConnection(NamedPipeServerStream pipe);

	public LcpServer(IProgram program, string pipeName, Flow flow)
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
	
												LcpConnection c = CreateConnection(pipe);
	
												lock(Clients)
													Clients.Add(c);
	
												Accept(c);
												program.CreateThread(() => c.Listen()).Start();
											}
											catch(AggregateException ex) when(ex.InnerException is OperationCanceledException)
											{
												return;
											}
										}
									})
									.Start();
	}

	//{
	//	return new LcpConnection(Program, pipe, this, Flow, Constructor);
	//}
}
