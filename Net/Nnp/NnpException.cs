namespace Uccs.Net;

public enum NnError : byte
{
	None,
	NotFound,
	Unavailable,
	Unknown,
}

public class NnpException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (NnError)value; }
	public NnError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public NnpException()
	{
	}

	public NnpException(NnError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
