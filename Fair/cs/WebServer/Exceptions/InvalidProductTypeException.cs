using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidProductTypeException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidProductType;

	public InvalidProductTypeException(ProductType productType) : base(string.Format(ErrorMessages.InvalidProductTypeFormat1, productType))
	{
	}
}
