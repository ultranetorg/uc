using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Uccs.Web.Exceptions;
using Uccs.Web.Extensions;

namespace Uccs.Web.Middlewares;

public sealed class ExceptionTransformMiddleware(RequestDelegate next, ILogger<ExceptionTransformMiddleware> logger,
	IWebHostEnvironment env, IOptions<JsonOptions> jsonOptions) : IMiddleware
{
	private readonly IDictionary<ErrorType, int> _errorTypeToHttpStatusCodeMap =
		new Dictionary<ErrorType, int>
	{
		{ ErrorType.ClientError, StatusCodes.Status400BadRequest },
		{ ErrorType.ServerError, StatusCodes.Status500InternalServerError },
		{ ErrorType.ResourceNotFound, StatusCodes.Status404NotFound },
		{ ErrorType.ResourceAlreadyExists, StatusCodes.Status409Conflict },
		{ ErrorType.NotAllowed, StatusCodes.Status405MethodNotAllowed },
	};

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (BaseException ex)
		{
			await HandleBaseExceptionAsync(context, ex);
		}
		catch (Exception ex)
		{
			await HandleInternalExceptionAsync(context, ex);
		}
	}

	private Task HandleBaseExceptionAsync(HttpContext context, BaseException exception)
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
		ErrorResponse errorResponse =
			CreateErrorResponse(exception.ErrorCode, exception.Message, exception.StackTrace);
		return context.Response
			.WriteJsonAsync(statusCode, errorResponse, SerializerOptions);
	}

	private Task HandleInternalExceptionAsync(HttpContext context, Exception exception)
	{
		logger.LogError(exception, "An error occurred while processing an API request");

		ErrorResponse errorResponse =
			CreateErrorResponse(500, Resources.InternalServerError, exception.StackTrace);
		return context.Response
			.WriteJsonAsync(StatusCodes.Status500InternalServerError, errorResponse, SerializerOptions);
	}

	private ErrorResponse CreateErrorResponse(int errorCode, string message, string? stackTrace)
	{
		return new ErrorResponse(errorCode, message)
		{
			StackTrace = env.IsLocal() || env.IsDevelopment() ? stackTrace : null,
		};
	}

	private int GetStatusCodeByErrorType(ErrorType errorType)
	{
		return _errorTypeToHttpStatusCodeMap.TryGetValue(errorType, out var statusCode)
			? statusCode
			: StatusCodes.Status500InternalServerError;
	}

	private JsonSerializerOptions SerializerOptions => jsonOptions.Value.SerializerOptions;
}
