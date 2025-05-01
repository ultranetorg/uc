using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidDepthException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidDepth;

	public InvalidDepthException(int depth) : base(string.Format(ErrorMessages.InvalidDepthFormat1, depth))
	{
	}
}
