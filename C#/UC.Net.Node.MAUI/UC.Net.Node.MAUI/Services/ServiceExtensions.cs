using UC.Net.Node.MAUI.Workflows;

namespace UC.Net.Node.MAUI.Services;

public static class ServiceExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
		// Services
        builder.Services.AddSingleton<IServicesMockData, ServicesMockData>(sp => new ServicesMockData());
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

		// Workflows
        builder.Services.AddSingleton<ICreateAccountWorkflow, CreateAccountWorkflow>(sp => new CreateAccountWorkflow());
        builder.Services.AddSingleton<IRegisterAuthorWorkflow, RegisterAuthorWorkflow>(sp => new RegisterAuthorWorkflow());
        builder.Services.AddSingleton<IRestoreAccountWorkflow, RestoreAccountWorkflow>(sp => new RestoreAccountWorkflow());

        return builder;
    }
}
