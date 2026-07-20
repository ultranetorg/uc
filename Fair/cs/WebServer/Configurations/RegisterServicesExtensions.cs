namespace Uccs.Fair;

public static class RegisterServicesExtensions
{
	public static IServiceCollection RegisterServices(this IServiceCollection services, FairNode node)
	{
		RegisterFairNode(services, node);
		RegisterServicesInternal(services, node);
		RegisterValidators(services);

		return services;
	}

	private static void RegisterFairNode(IServiceCollection services, FairNode node)
	{
		services.AddSingleton(node);
		services.AddSingleton(node.Mcv);
	}

	private static void RegisterServicesInternal(IServiceCollection services, FairNode node)
	{
		services.AddSingleton<AuthorsService>();
		services.AddSingleton<CategoriesService>();
		services.AddSingleton<FilesService>();
		services.AddSingleton<ProposalService>();
		services.AddSingleton<PublicationsService>();
		services.AddSingleton<ReviewsService>();
		services.AddSingleton<SearchService>();
		services.AddSingleton<SitesService>();
		services.AddSingleton<PerpetualSurveysService>();
		services.AddSingleton<ProductsService>();
		services.AddSingleton<UserService>();
		services.AddSingleton<ModeratorProposalsService>();
		services.AddSingleton<ProposalCommentsService>();
		services.AddSingleton<UnpublishedPublicationsService>();
		services.AddSingleton<UnpublishedSiteProductsService>();
		services.AddSingleton<UsersService>();
	}

	private static void RegisterValidators(IServiceCollection services)
	{
		services.AddSingleton<AccountAddressValidator>();
		services.AddSingleton<DepthValidator>();
		services.AddSingleton<AutoIdValidator>();
		services.AddSingleton<LimitValidator>();
		services.AddSingleton<PaginationValidator>();
		services.AddSingleton<SearchQueryValidator>();
		services.AddSingleton<SiteSearchQueryValidator>();
		services.AddSingleton<UserNameValidator>();
		services.AddSingleton<VersionValidator>();
	}
}
