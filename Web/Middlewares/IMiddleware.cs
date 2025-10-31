using Microsoft.AspNetCore.Http;

namespace Uccs.Web.Middlewares;

internal interface IMiddleware
{
	Task InvokeAsync(HttpContext context);
}
