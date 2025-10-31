namespace Microsoft.AspNetCore.Hosting;

public static class HostingEnvironmentExtensions
{
	private static readonly string Local = nameof(Local);

	public static bool IsLocal(this IWebHostEnvironment env)
	{
		return env.EnvironmentName == Local;
	}
}
