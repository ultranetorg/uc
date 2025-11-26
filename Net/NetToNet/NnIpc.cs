using System.Numerics;
using System.Reflection;

namespace Uccs.Net;

public enum NnIppConnectionType : byte
{
	Unknown = 0,
	Node,
	Client
}

public class NnIppClientConnection : IppConnection
{
	public R Call<A, R>(Nnc<A, R> call) where A : NnArgumentation, new() where R : Return => Call(call.Argumentation) as R;

	public NnIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}

	public override void Established()
	{
		Writer.Write(NnIppConnectionType.Client);
	}
}

public class NnIppNodeConnection : IppConnection
{
	public NnIppNodeConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<Argumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<Return>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>	(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
