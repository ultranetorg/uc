namespace UC.Umc.Exceptions;

public abstract class BaseException : Exception
{
    public abstract ExceptionCode Code { get; }
}
