using Uuc.Common.Helpers;
using Uuc.Pages;

namespace Uuc.Common.Configurations;

public static class PagesConfiguration
{
	public static MauiAppBuilder ConfigurePages(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<AppShell>();
		
		RegisterPages(builder);

		return builder;
	}

	private static void RegisterPages(MauiAppBuilder builder)
	{
		Type[] pagesTypes = TypesHelper.GetPagesTypes();
		foreach (Type type in pagesTypes)
		{
			builder.Services.AddTransient(type);
		}
	}
}
