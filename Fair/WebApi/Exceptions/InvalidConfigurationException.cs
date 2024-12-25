using Explorer.Common.Exceptions;

namespace Explorer.Api.Exceptions;

public class InvalidConfigurationException : BaseException
{
	public override ErrorType ErrorType => ErrorType.ServerError;

	public override int ErrorCode => (int) ErrorCodes.InvalidConfiguration;

	public InvalidConfigurationException() : base(WebApi.Exceptions.ErrorMessages.InvalidWebAPIConfiguration)
	{
	}
}
