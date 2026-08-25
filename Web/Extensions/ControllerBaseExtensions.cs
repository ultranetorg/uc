namespace Microsoft.AspNetCore.Mvc;

public static class ControllerBaseExtensions
{
	public static T? OkPaged<T>(this ControllerBase controller, T? response, int page, int pageSize, int totalItems)
		where T : class
	{
		ArgumentNullException.ThrowIfNull(controller);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		controller.Response.Headers["X-Page"] = page.ToString();
		controller.Response.Headers["X-Page-Size"] = pageSize.ToString();
		controller.Response.Headers["X-Total-Items"] = totalItems.ToString();
		controller.Response.Headers.AccessControlExposeHeaders = "X-Page, X-Page-Size, X-Total-Items";

		return response;
	}

	public static T OkPaged<T>(this ControllerBase controller, T response, int page, int pageSize) where T : class
	{
		ArgumentNullException.ThrowIfNull(controller);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		controller.Response.Headers["X-Page"] = page.ToString();
		controller.Response.Headers["X-Page-Size"] = pageSize.ToString();
		controller.Response.Headers.AccessControlExposeHeaders = "X-Page, X-Page-Size";

		return response;
	}

	public static T OkPaged<T>(this ControllerBase controller, T response, int totalItems) where T : class
	{
		ArgumentNullException.ThrowIfNull(controller);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalItems);

		controller.Response.Headers["X-Total-Items"] = totalItems.ToString();
		controller.Response.Headers.AccessControlExposeHeaders = "X-Total-Items";

		return response;
	}
}
