namespace UC.Net.Node.MAUI.Exceptions;

public class AccountNotFoundException : BaseException
{
    public override ExceptionCode Code => ExceptionCode.AccountNotFound;
}
