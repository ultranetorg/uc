namespace UO.Mobile.UUC.Exceptions;

public abstract class BaseException : Exception
{
    public abstract ExceptionCode Code { get; }
}
