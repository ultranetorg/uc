namespace UO.Mobile.UUC.Exceptions;

public class AccountNotFoundException : BaseException
{
    public override ExceptionCode Code => ExceptionCode.AccountNotFound;
}
