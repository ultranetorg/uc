namespace Uccs.Fair;

public static class RegisterServicesExtensions
{
	public static IServiceCollection RegisterServices(this IServiceCollection services, FairNode node)
	{
		RegisterMisc(services, node);
		RegisterServicesInternal(services, node);
		RegisterValidators(services);

		return services;
	}

	private static void RegisterMisc(IServiceCollection services, FairNode node)
	{
		services.AddSingleton(node.Mcv);
	}

	private static void RegisterServicesInternal(IServiceCollection services, FairNode node)
	{
		services.AddSingleton<ICategoriesService, CategoriesService>();
		services.AddSingleton<IDisputesService, DisputesService>();
		services.AddSingleton<IPublicationsService, PublicationsService>();
		services.AddSingleton<IReviewsService, ReviewsService>();
		services.AddSingleton<ISitesService, SitesService>();
		services.AddSingleton<IUsersService, UsersService>();
	}

	private static void RegisterValidators(IServiceCollection services)
	{
		services.AddSingleton<IEntityIdValidator, EntityIdValidator>();
		services.AddSingleton<IPaginationValidator, PaginationValidator>();
	}
}
