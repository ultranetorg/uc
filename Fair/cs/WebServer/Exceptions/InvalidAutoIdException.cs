using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidAutoIdException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidEntityId;

	public InvalidAutoIdException(string entityName) : base(string.Format(ErrorMessages.InvalidAutoIdFormat1, entityName))
	{
	}

	public InvalidAutoIdException(string entityName, string autoId) : base(string.Format(ErrorMessages.InvalidAutoIdFormat2, entityName, autoId))
	{
	}
}
