using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidAccountAddressException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidAccountAddress;

	public InvalidAccountAddressException() : base(ErrorMessages.InvalidAccountAddress)
	{
	}

	public InvalidAccountAddressException(string accountAddress) : base(string.Format(ErrorMessages.InvalidAccountAddressFormat1, accountAddress))
	{
	}
}
