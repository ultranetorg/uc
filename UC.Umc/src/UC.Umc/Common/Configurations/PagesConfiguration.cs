using UC.Umc.Pages;

namespace UC.Umc.Common.Configurations;

public static class PagesConfiguration
{
	public static MauiAppBuilder ConfigurePages(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<AppShell>();

		return builder;
	}
}
