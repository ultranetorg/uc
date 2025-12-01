namespace Uccs.Net;

public enum IpcError : byte
{
	None,
	NotFound,
	Unavailable,
	Unknown,
	ConnectionLost,
}

public class IpcException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (IpcError)value; }
	public IpcError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public IpcException()
	{
	}

	public IpcException(IpcError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
