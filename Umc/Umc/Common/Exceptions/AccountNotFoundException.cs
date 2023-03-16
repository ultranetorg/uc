namespace UC.Umc.Exceptions;

public class AccountNotFoundException : BaseException
{
    public override ExceptionCode Code => ExceptionCode.AccountNotFound;
}
