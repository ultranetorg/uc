using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidCategoryException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidCategoryException;

	public InvalidCategoryException() : base(ErrorMessages.InvalidCategory)
	{
	}
}
