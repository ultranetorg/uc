using Uccs.Net;
using Uuc.Services;

namespace Uuc.Common.Configurations;

public static class ServicesConfiguration
{
	public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<IAccountsService, AccountsService>();
		builder.Services.AddTransient<IApplicationService, ApplicationService>();
		builder.Services.AddTransient<IAuthorizationsService, AuthorizationsService>();
		builder.Services.AddTransient<IDigitalIdentitiesService, DigitalIdentitiesService>();
		builder.Services.AddTransient<INavigationService, NavigationService>();
		builder.Services.AddTransient<IOperationsService, OperationsService>();
		builder.Services.AddSingleton<IPasswordService, PasswordService>();
		builder.Services.AddSingleton<ISessionService, SessionService>();
		builder.Services.AddTransient<ISettingsService, PreferencesService>();

		builder.Services.AddTransient<Cryptography, NormalCryptography>();

		return builder;
	}
}
