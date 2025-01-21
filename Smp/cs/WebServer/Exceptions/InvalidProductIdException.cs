using Uccs.Web.Exceptions;

namespace Uccs.Smp;

public class InvalidProductIdException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidProductId;

	public InvalidProductIdException() : base(ErrorMessages.InvalidProductId)
	{
	}

	public InvalidProductIdException(string productId) : base(string.Format(ErrorMessages.InvalidProductIdFormat1, productId))
	{
	}
}
