using Uccs;

namespace Uccs.Net;

public abstract class Argumentation : ITypeCode
{
//	public abstract void Read(BinaryReader reader);
//	public abstract void Write(BinaryWriter writer);
}

public abstract class Return : ITypeCode
{
//	public abstract void Read(BinaryReader reader);
//	public abstract void Write(BinaryWriter writer);
}


public interface ICall<A, R>  where A : Argumentation where R : Return /// Peer-to-Peer Call
{
}
