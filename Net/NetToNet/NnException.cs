namespace Uccs.Net;

public enum NnError : byte
{
	None,
	NotFound,
	Unavailable,
	Unknown,
}

public class NnException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (NnError)value; }
	public NnError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public NnException()
	{
	}

	public NnException(NnError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
