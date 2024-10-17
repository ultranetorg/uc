using UC.Umc.Services;
using UC.Umc.Services.Accounts;
using UC.Umc.Services.Domains;
using UC.Umc.Services.Notifications;
using UC.Umc.Services.Resources;
using UC.Umc.Services.Transactions;

namespace UC.Umc.Common.Configurations;

public static class ServiceExtensions
{
	// TODO: use this method.
	public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
	{
		// Services
		builder.Services.AddSingleton<IServicesMockData, ServicesMockData>();
		builder.Services.AddSingleton<IAccountsService, AccountsMockService>();
		builder.Services.AddSingleton<IDomainsService, DomainsMockService>();
		builder.Services.AddSingleton<INotificationsService, NotificationsMockService>();
		builder.Services.AddSingleton<IResourcesService, ResourcesMockService>();
		builder.Services.AddSingleton<ITransactionsService, TransactionsMockService>();

		return builder;
	}
}
