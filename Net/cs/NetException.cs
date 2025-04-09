namespace Uccs.Net;

public enum ExceptionClass : byte
{
	None, Node, Request, Entity,
	_Next,
	Ntn = _Next,
	Vault,
}

public enum NodeError : byte
{
	None,
	Unknown,
	Connectivity,
	Integrity,
	Internal,
	Invalid,
	AlreadyRunning,
	Timeout,
	NoIP,
	NotFound,
	NotBase,
	NotChain,
	NotSeed,
	NotMember,
	NotSynchronized,
	NotUnlocked,
	NoMcv,
	NoNtn,
	NoNodeForNet,
	TooEearly,
	AllNodesFailed,
	NotOnlineYet,
	CircularRoute,
	LimitExceeded
}

public enum EntityError : byte
{
	None,
	NotFound,
	ExcutionFailed,
	EmissionFailed,
	NoMembers,
	HashMismatach
}

public enum RequestError : byte
{
	None,
	IncorrectRequest,
	OutOfRange,
}

public enum VaultError : byte
{
	None,
	Locked,
	AlreadyLocked,
	AlreadyUnlocked,
    AccountNotFound,
    NetNotFound,
}

public abstract class NetException : Exception, ITypeCode, IBinarySerializable 
{
	public abstract int		ErrorCode { get; set; }

	static NetException()
	{
	}

	public NetException()
	{
	}

	public NetException(string message) : base(message)
	{
	}

	public void Read(BinaryReader reader)
	{
		ErrorCode = reader.Read7BitEncodedInt();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt(ErrorCode);
	}
}

public class NodeException : NetException
{
	public override int				ErrorCode { get => (int)Error; set => Error = (NodeError)value; }
	public NodeError				Error { get; protected set; }
	public override string			Message => Error.ToString();

	public NodeException()
	{
	}

	public NodeException(NodeError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}

}

public class RequestException : NetException
{
	public override int				ErrorCode { get => (int)Error; set => Error = (RequestError)value; }
	public RequestError				Error { get; protected set; }
	public override string			Message => Error.ToString();

	public RequestException()
	{
	}

	public RequestException(RequestError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}

public class EntityException : NetException
{
	public override int				ErrorCode { get => (int)Error; set => Error = (EntityError)value; }
	public EntityError				Error { get; protected set; }
	public override string			Message => Error.ToString();

	public EntityException()
	{
	}

	public EntityException(EntityError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}

public class VaultException : NetException
{
	public override int				ErrorCode { get => (int)Error; set => Error = (VaultError)value; }
	public VaultError				Error { get; protected set; }
	public override string			Message => Error.ToString();

	public VaultException()
	{
	}

	public VaultException(VaultError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
