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
		services.AddSingleton<IAuthorsService, AuthorsService>();
		services.AddSingleton<ICategoriesService, CategoriesService>();
		services.AddSingleton<ProposalCommentsService>();
		services.AddSingleton<IProposalService, ProposalService>();
		services.AddSingleton<IPublicationsService, PublicationsService>();
		services.AddSingleton<IReviewsService, ReviewsService>();
		services.AddSingleton<ISearchService, SearchService>();
		services.AddSingleton<ISitesService, SitesService>();
		services.AddSingleton<IAccountsService, AccountsService>();
	}

	private static void RegisterValidators(IServiceCollection services)
	{
		services.AddSingleton<IDepthValidator, DepthValidator>();
		services.AddSingleton<IAutoIdValidator, AutoIdValidator>();
		services.AddSingleton<IPaginationValidator, PaginationValidator>();
		services.AddSingleton<ISearchQueryValidator, SearchQueryValidator>();
		services.AddSingleton<ISiteSearchQueryValidator, SiteSearchQueryValidator>();
	}
}
