namespace Uccs.Net;

public enum IccpError : byte
{
	None,
	ExcutionFailed,
	NotFound,
	NotReady,
	Unavailable,
	Unknown,
	PpcFailure,
}

public class IccpException : CodeException
{
	public override int		Code { get => (int)Error; set => Error = (IccpError)value; }
	public IccpError		Error { get; protected set; }

	public IccpException()
	{
	}

	public IccpException(IccpError erorr) : base(erorr.ToString())
	{
		Error = erorr;
	}
}
