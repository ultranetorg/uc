using Uccs;

namespace Uccs.Net;

public abstract class Argumentation : ITypeCode
{
//	public abstract void Read(Reader reader);
//	public abstract void Write(Writer writer);
}

public abstract class Result : ITypeCode
{
//	public abstract void Read(Reader reader);
//	public abstract void Write(Writer writer);
}


public interface ICall<A, R>  where A : Argumentation where R : Result /// Peer-to-Peer Call
{
}
