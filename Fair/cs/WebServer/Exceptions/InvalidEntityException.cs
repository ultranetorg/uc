using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidEntityException : BaseException
{
	public override ErrorType ErrorType => ErrorType.InvalidEntity;

	public override int ErrorCode => (int) ErrorCodes.InvalidEntity;

	public InvalidEntityException(string entityName) : base(string.Format(ErrorMessages.InvalidEntityFormat1, entityName))
	{
	}

	public InvalidEntityException(string entityName, string autoId) : base(string.Format(ErrorMessages.InvalidEntityFormat2, entityName, autoId))
	{
	}
}
