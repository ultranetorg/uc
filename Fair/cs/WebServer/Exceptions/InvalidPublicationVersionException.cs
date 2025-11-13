using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidPublicationVersionException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidProductVersion;

	public InvalidPublicationVersionException() : base(ErrorMessages.InvalidPublicationVersion)
	{
	}

	public InvalidPublicationVersionException(string publicationId, int version) : base(string.Format(ErrorMessages.InvalidPublicationVersionFormat2, publicationId, version))
	{
	}
}
