namespace UC.Umc.Services;

public static class ServiceExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
		// Services
        builder.Services.AddSingleton<IServicesMockData, ServicesMockData>(sp => new ServicesMockData(sp.GetService<ILogger<ServicesMockData>>()));
        builder.Services.AddSingleton<IAccountsService, AccountsMockService>(sp => new AccountsMockService(
			Ioc.Default.GetService<IServicesMockData>()));
        builder.Services.AddSingleton<IAuthorsService, AuthorsMockService>(sp => new AuthorsMockService(
			Ioc.Default.GetService<IServicesMockData>()));
        builder.Services.AddSingleton<INotificationsService, NotificationsMockService>(sp => new NotificationsMockService(
			Ioc.Default.GetService<IServicesMockData>()));
        builder.Services.AddSingleton<IProductsService, ProductsMockService>(sp => new ProductsMockService(
			Ioc.Default.GetService<IServicesMockData>()));
        builder.Services.AddSingleton<ITransactionsService, TransactionsMockService>(sp => new TransactionsMockService(
			Ioc.Default.GetService<IServicesMockData>()));

        return builder;
    }
}
