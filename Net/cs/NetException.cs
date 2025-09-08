﻿namespace Uccs.Net;

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
	AllNodesFailed,
	AlreadyRunning,
	CircularRoute,
	Connectivity,
	Integrity,
	Internal,
	Invalid,
	LimitExceeded,
	NoIP,
	NoMcv,
	NoNodeForNet,
	NoNtn,
	NoPeering,
	NotChain,
	NotGraph,
	NotFound,
	NotMember,
	NotOnlineYet,
	NotSeed,
	NotSynchronized,
	Locked,
	Timeout,
	TransactionRejected,
	TooEearly,
	Unknown,
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

public class NodeException : CodeException
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

public class RequestException : CodeException
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

public class EntityException : CodeException
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

public class VaultException : CodeException
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
