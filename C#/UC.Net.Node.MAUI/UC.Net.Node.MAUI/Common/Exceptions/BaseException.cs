namespace UC.Net.Node.MAUI.Exceptions;

public abstract class BaseException : Exception
{
    public abstract ExceptionCode Code { get; }
}
