namespace Uccs.Net;

public enum LcpError : byte
{
	None,
	NotFound,
	Unavailable,
	Unknown,
	ConnectionLost,
}

public class LcpException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (LcpError)value; }
	public LcpError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public LcpException()
	{
	}

	public LcpException(LcpError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
