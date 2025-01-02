namespace Uccs.WebUI.Middlewares;

public sealed class ErrorResponse(int errorCode, string message)
{
	public int ErrorCode { get; } = errorCode;

	public string Message { get; } = message;

	public string? StackTrace { get; init; }
}
