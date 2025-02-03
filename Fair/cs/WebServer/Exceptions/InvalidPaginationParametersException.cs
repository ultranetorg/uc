using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidPaginationParametersException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidPaginationParameters;

	public InvalidPaginationParametersException() : base(ErrorMessages.InvalidPaginationParameters)
	{
	}
}
