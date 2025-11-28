using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public enum NnpIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public class NnpIppClientConnection : IppConnection
{
	public R Call<A, R>(Nnc<A, R> call) where A : NnpArgumentation, new() where R : Return => Call(call.Argumentation) as R;

	public NnpIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Established()
	{
		Writer.Write(NnpIppConnectionType.Client);
	}
}

public class NnpIppNodeConnection : IppConnection
{
	public NnpIppNodeConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
