using Uccs.Web.Exceptions;

namespace Uccs.Smp;

public class InvalidEntityIdException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidEntityId;

	public InvalidEntityIdException(string entityName) : base(string.Format(ErrorMessages.InvalidEntityIdFormat1, entityName))
	{
	}

	public InvalidEntityIdException(string entityName, string entityId) : base(string.Format(ErrorMessages.InvalidEntityIdFormat2, entityName, entityId))
	{
	}
}
