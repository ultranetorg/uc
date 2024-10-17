namespace UC.Umc.Common.Exceptions;

public class AccountNotFoundException : BaseException
{
	public override ExceptionCode Code => ExceptionCode.AccountNotFound;
}
