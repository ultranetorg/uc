using Uuc.Services;

namespace Uuc.Common.Configurations;

public static class ServicesConfiguration
{
	public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<IAccountsService, AccountsService>();
		builder.Services.AddTransient<INavigationService, MauiNavigationService>();
		builder.Services.AddTransient<ISessionService, SessionService>();
		builder.Services.AddTransient<ISettingsService, MauiSettingsService>();

		return builder;
	}
}
