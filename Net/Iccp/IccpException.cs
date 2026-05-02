namespace Uccs.Net;

public enum IccpError : byte
{
	None,
	ExcutionFailed,
	NotFound,
	Unavailable,
	Unknown,
}

public class IccpException : CodeException
{
	public override int		ErrorCode { get => (int)Error; set => Error = (IccpError)value; }
	public IccpError			Error { get; protected set; }
	public override string	Message => Error.ToString();

	public IccpException()
	{
	}

	public IccpException(IccpError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
