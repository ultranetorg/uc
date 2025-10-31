using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Uccs.Web.Extensions;

public static class HttpResponseExtensions
{
	public static Task WriteJsonAsync<T>(this HttpResponse response, int statusCode, T value,
		JsonSerializerOptions? options = null)
		where T : class
	{
		string text = JsonSerializer.Serialize<T>(value, options);

		response.StatusCode = statusCode;
		response.ContentType = MediaTypeNames.Application.Json;

		return response.WriteAsync(text);
	}
}
