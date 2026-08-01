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
	public override int		Code { get => (int)Error; set => Error = (LcpError)value; }
	public LcpError			Error { get; protected set; }

	public LcpException()
	{
	}

	public LcpException(LcpError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
