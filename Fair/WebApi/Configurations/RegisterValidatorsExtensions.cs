namespace Explorer.Api.Configurations;

public static class RegisterValidatorsExtensions
{
	public static IServiceCollection RegisterValidators(this IServiceCollection services)
	{
		services.AddScoped<IAddressValidator, AddressValidator>();
		services.AddScoped<IAuctionNameValidator, AuctionNameValidator>();
		services.AddScoped<IAuthorNameValidator, AuthorNameValidator>();
		services.AddScoped<IQueryValidator, QueryValidator>();
		services.AddScoped<IOperationIdValidator, OperationIdValidator>();
		services.AddScoped<IResourceNameValidator, ResourceNameValidator>();
		services.AddScoped<IResourceIdValidator, ResourceIdValidator>();
		services.AddScoped<ITransactionIdValidator, TransactionIdValidator>();

		return services;
	}
}
