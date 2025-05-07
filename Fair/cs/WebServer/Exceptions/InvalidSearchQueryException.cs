using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidSearchQueryException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidSearchQuery;

	public InvalidSearchQueryException() : base(ErrorMessages.InvalidSearchQuery)
	{
	}
}
