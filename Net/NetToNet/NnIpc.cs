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
	public Rp Call<Rp>(Nnc<Rp> call) where Rp : class, IBinarySerializable => Send(call) as Rp;

	public NnIppClientConnection(IProgram program, string name, Flow flow) : base(program, name, flow)
	{
		Constructor = new ();
		Constructor.Register<CallArgumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CallReturn>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>		(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
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
		Constructor.Register<CallArgumentation>	(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CallReturn>		(Assembly.GetExecutingAssembly(), typeof(NnClass), i => i.Remove(i.Length - 3));
		Constructor.Register<CodeException>		(Assembly.GetExecutingAssembly(), typeof(ExceptionClass), i => i.Remove(i.IndexOf("Exception")));
	}
}
