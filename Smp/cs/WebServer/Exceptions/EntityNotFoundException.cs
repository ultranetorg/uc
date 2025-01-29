using Uccs.Net;
using Uccs.Web.Exceptions;

namespace Uccs.Smp;

public class EntityNotFoundException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ResourceNotFound;

	public override int ErrorCode => (int) ErrorCodes.EntityNotFound;

	public EntityNotFoundException(string entityName) : base(string.Format(ErrorMessages.ProductNotFoundFormat1, entityName))
	{
	}

	public EntityNotFoundException(string entityName, string entityId) : base(string.Format(ErrorMessages.ProductNotFoundFormat2, entityName, entityId))
	{
	}
}
