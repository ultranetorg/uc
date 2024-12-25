using Explorer.MongoDB.Configurations;
using Uocs;

namespace Explorer.WebApi.Configurations;

public static class AddBlockchainDatabaseStoreExtensions
{
	public static IServiceCollection AddBlockchainDatabaseStore(this IServiceCollection services, ConfigurationManager configurationManager)
	{
		Guard.Against.Null(services);

		string connectionString = configurationManager.GetConnectionString("DefaultConnection")
			?? G.Dev.ExplorerMongo.ConnectionString;

		services.AddBlockchainDatabaseStore(options =>
		{
			options.ConnectionString = connectionString;
		});

		return services;
	}
}
