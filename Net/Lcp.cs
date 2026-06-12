using System.IO.Pipes;

namespace Uccs.Net;

public abstract class Argumentation : ITypeCode
{
}

public abstract class Result : ITypeCode
{
}

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
	public string					Application;
	public PipeStream				Pipe;
	public bool						Connected => Pipe != null && Pipe.IsConnected;
	public Reader					Reader;
	public Writer					Writer;
	protected List<LcpRequest>		OutRequests = new();
	protected LcpServer				Server;
	protected int					IdCounter = 0;
	protected Flow					Flow;

	public Func<string, string, IccpArgumentation, IccpLcpConnection, Flow, IccpResult>	Handler;

	public abstract void			Listen();

	public LcpConnection(IProgram program, NamedPipeServerStream pipe, LcpServer server, Flow flow) /// For Server
	{
		Program		= program;
		Pipe		= pipe;
		Server		= server; 
		Flow		= flow.CreateNested();;

		Reader = new Reader(Pipe, Iccp.Constructor);
		Writer = new Writer(Pipe, Iccp.Constructor);

		Application = Reader.ReadASCII();
	}

	public LcpConnection(IProgram program, string name, string application, Flow flow) /// For Client
	{
		Program		= program;
		Application	= application;
		Flow		= flow.CreateNested(name);

		var t = program.CreateThread(() =>	{ 
												while(Flow.Active)
												{
													try
													{
														var pipe = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
												
														Pipe = pipe;
														IdCounter = 0;
	
														flow.Log?.Report(this, $"Listening nexus {name}");

														pipe.ConnectAsync(Flow.Cancellation).Wait();
	
														Reader = new Reader(pipe, Iccp.Constructor);
														Writer = new Writer(pipe, Iccp.Constructor);
															
														Writer.WriteASCII(Application);

														Established();
														Listen();

														flow.Log?.Report(this, $"Established to {name}");
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
			Server.ConnectionLost?.Invoke(this);

			lock(Server.Connections)
			{
				Server.Connections.Remove(this);
			}
		}
	}
}

public abstract class LcpServer
{
	protected IProgram					Program;
	protected Flow						Flow;
	readonly string						Name;
	public readonly List<LcpConnection>	Connections = [];

	public delegate void				LcpConnectionDelegate(LcpConnection connection);

	public LcpConnectionDelegate		ConnectionEstablished;
	public LcpConnectionDelegate		ConnectionLost;

	public virtual void					Accept(LcpConnection connection){}
	public abstract IccpResult			Relay(string from, string to, IccpArgumentation call, IccpLcpConnection connection, Flow flow);
	public IccpResult					Call(string from, string to, IccpArgumentation call, Flow flow) => Relay(from, to, call, null, flow);
	public R							Call<R>(string from, string to, IccpArgumentation argumentation, Flow flow) where R : IccpResult => Call(from, to, argumentation, flow) as R;

	protected abstract LcpConnection	CreateConnection(NamedPipeServerStream pipe);

	public LcpServer(IProgram program, string pipeName, Flow flow)
	{
		Program = program;
		Name = pipeName;
		Flow = flow;

		flow.Log?.Report(this, $"Listen to {Name}");
		
		var t = program.CreateThread(() =>	{ 
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
	
														lock(Connections)
															Connections.Add(c);
	
														Accept(c);

														flow.Log?.Report(this, $"Accepted from {c.Application}");

														var t = program.CreateThread(c.Listen);

														t.Name = $"{Name} {nameof(LcpServer)} <- {c.Application}";
														t.Start();
													}
													catch(AggregateException ex) when(ex.InnerException is OperationCanceledException)
													{
														return;
													}
												}
											});

		t.Name = $"{Name} {GetType().Name}";
		t.Start();
	}

	//{
	//	return new LcpConnection(Program, pipe, this, Flow, Constructor);
	//}
}
