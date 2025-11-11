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
		services.AddSingleton<FilesService>();
		services.AddSingleton<IProposalService, ProposalService>();
		services.AddSingleton<PublicationsService>();
		services.AddSingleton<IReviewsService, ReviewsService>();
		services.AddSingleton<ISearchService, SearchService>();
		services.AddSingleton<SitesService>();
		services.AddSingleton<ProductsService>();
		services.AddSingleton<IAccountsService, AccountsService>();
		services.AddSingleton<ModeratorProposalsService>();
		services.AddSingleton<ProposalCommentsService>();
	}

	private static void RegisterValidators(IServiceCollection services)
	{
		services.AddSingleton<IDepthValidator, DepthValidator>();
		services.AddSingleton<IAutoIdValidator, AutoIdValidator>();
		services.AddSingleton<LimitValidator>();
		services.AddSingleton<IPaginationValidator, PaginationValidator>();
		services.AddSingleton<ISearchQueryValidator, SearchQueryValidator>();
		services.AddSingleton<ISiteSearchQueryValidator, SiteSearchQueryValidator>();
		services.AddSingleton<VersionValidator>();
	}
}
