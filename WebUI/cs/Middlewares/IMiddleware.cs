using Microsoft.AspNetCore.Http;

namespace Uccs.WebUI.Middlewares;

internal interface IMiddleware
{
	Task InvokeAsync(HttpContext context);
}
