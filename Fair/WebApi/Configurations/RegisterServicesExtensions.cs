using Explorer.Api.Exceptions;
using Explorer.BLL.Configurations;
using Explorer.BLL.Services.Interfaces;
using Explorer.BLL.Services.PriceConverter;

namespace Explorer.Api.Configurations;

public static class RegisterServicesExtensions
{
	public static IServiceCollection RegisterServices(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		services.AddSingleton<IChainService, ChainService>();
		services.AddScoped<IAccountsService, AccountsService>();
		services.AddScoped<IAuctionsService, AuctionsService>();
		services.AddScoped<IAuthorsService, AuthorsService>();
		services.AddScoped<IOperationsService, OperationsService>();
		services.AddScoped<IResourcesService, ResourcesServices>();
		services.AddScoped<IRoundsService, RoundsService>();
		services.AddScoped<ITransactionsService, TransactionsService>();
		services.AddScoped<ISearchService, SearchService>();

		AddCurrencyRatesService(services, configurationManager);

		return services;
	}

	private static void AddCurrencyRatesService(IServiceCollection services, ConfigurationManager configurationManager)
	{
		CoinCapAPIConfiguration? config = configurationManager.Get<CoinCapAPIConfiguration>();
		If.Value(config).IsNull().Throw<InvalidConfigurationException>();
		services.AddSingleton(config.CoinCapAPI);

		services.AddSingleton<IEthereumRateUsd, EthereumRateUsd>();
	}
}
