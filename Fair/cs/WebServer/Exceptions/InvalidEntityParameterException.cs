using Uccs.Web.Exceptions;

namespace Uccs.Fair;

public class InvalidEntityParameterException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ClientError;

	public override int ErrorCode => (int) ErrorCodes.InvalidEntityParameter;

	public InvalidEntityParameterException(string entityName, string paramterName) : base(string.Format(ErrorMessages.InvalidEntityParameterFormat2, entityName, paramterName))
	{
	}

	public InvalidEntityParameterException(string entityName, string paramterName, string value) : base(string.Format(ErrorMessages.InvalidEntityParameterFormat3, entityName, paramterName, value))
	{
	}
}
