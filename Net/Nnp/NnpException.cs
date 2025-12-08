namespace Uccs.Net;

public enum NnpError : byte
{
	None,
	NotFound,
	Unavailable,
	Unknown,
}

public class NnpException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (NnpError)value; }
	public NnpError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public NnpException()
	{
	}

	public NnpException(NnpError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
