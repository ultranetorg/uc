namespace UC.Umc.Common.Exceptions;

public abstract class BaseException : Exception
{
	public abstract ExceptionCode Code { get; }
}
