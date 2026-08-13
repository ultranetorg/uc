using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidSearchParamsException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidSearchParams;

	public InvalidSearchParamsException() : base(ErrorMessages.InvalidSearchParams)
	{
	}
}
