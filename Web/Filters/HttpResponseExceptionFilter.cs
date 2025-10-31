using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uccs.Web.Exceptions;
using Uccs.Web.Middlewares;

namespace Uccs.Web.Filters;

public class HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, IWebHostEnvironment env) : IActionFilter
{
	private readonly IDictionary<ErrorType, int> _errorTypeToHttpStatusCodeMap = new Dictionary<ErrorType, int>
	{
		{ ErrorType.ClientError, StatusCodes.Status400BadRequest },
		{ ErrorType.ServerError, StatusCodes.Status500InternalServerError },
		{ ErrorType.ResourceNotFound, StatusCodes.Status404NotFound },
		{ ErrorType.ResourceAlreadyExists, StatusCodes.Status409Conflict },
		{ ErrorType.NotAllowed, StatusCodes.Status405MethodNotAllowed },
	};

	public void OnActionExecuting(ActionExecutingContext context)
	{
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
		if (context.Exception is BaseException httpResponseException)
		{
			HandleHttpResponseException(context, httpResponseException);
			return;
		}

		if (context.Exception is Exception exception)
		{
			HandleException(context, exception);
			return;
		}
	}

	private void HandleHttpResponseException(ActionExecutedContext context, BaseException exception)
	{
		if (exception.ErrorType == ErrorType.ServerError)
		{
			logger.LogError(exception, "An error occurred while processing an API request");
		}
		else
		{
			logger.LogWarning(exception, "A warning occurred while processing an API request");
		}

		int statusCode = GetStatusCodeByErrorType(exception.ErrorType);
		ErrorResponse errorResponse = CreateErrorResponse(exception.ErrorCode, exception.Message, exception.StackTrace);

		context.Result = new ObjectResult(errorResponse)
		{
			StatusCode = statusCode,
		};
		context.ExceptionHandled = true;
	}

	private void HandleException(ActionExecutedContext context, Exception exception)
	{
		logger.LogError(exception, "An error occurred while processing an API request");

		ErrorResponse errorResponse = CreateErrorResponse(500, Resources.InternalServerError, exception.StackTrace);

		context.Result = new ObjectResult(errorResponse)
		{
			StatusCode = 500,
		};
		context.ExceptionHandled = true;
	}

	private int GetStatusCodeByErrorType(ErrorType errorType)
	{
		return _errorTypeToHttpStatusCodeMap.TryGetValue(errorType, out var statusCode)
			? statusCode
			: StatusCodes.Status500InternalServerError;
	}

	private ErrorResponse CreateErrorResponse(int errorCode, string message, string? stackTrace)
	{
		return new ErrorResponse(errorCode, message)
		{
			StackTrace = env.IsLocal() || env.IsDevelopment() ? stackTrace : null,
		};
	}
}
