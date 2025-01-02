namespace Uccs.Web.Exceptions;

public abstract class BaseException : Exception
{
	public abstract ErrorType ErrorType { get; }

	public abstract int ErrorCode { get; }

	protected BaseException()
	{
	}

	protected BaseException(string message) : base(message)
	{
	}
}
