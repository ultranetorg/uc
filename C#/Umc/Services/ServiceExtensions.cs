namespace UC.Umc.Services;

public static class ServiceExtensions
{
	public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
	{
		Guard.IsNotNull(builder);

		// Services
		builder.Services.AddSingleton<IServicesMockData, ServicesMockData>();
		builder.Services.AddSingleton<IAccountsService, AccountsMockService>();
		builder.Services.AddSingleton<IAuthorsService, AuthorsMockService>();
		builder.Services.AddSingleton<INotificationsService, NotificationsMockService>();
		builder.Services.AddSingleton<IProductsService, ProductsMockService>();
		builder.Services.AddSingleton<ITransactionsService, TransactionsMockService>();

		return builder;
	}
}
