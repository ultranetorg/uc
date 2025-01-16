using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class ProductNotFoundException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ResourceNotFound;

	public override int ErrorCode => (int) ErrorCodes.ProductNotFound;

	public ProductNotFoundException() : base(ErrorMessages.ProductNotFound)
	{
	}

	public ProductNotFoundException(string productId) : base(string.Format(ErrorMessages.ProductNotFoundFormat1, productId))
	{
	}
}
