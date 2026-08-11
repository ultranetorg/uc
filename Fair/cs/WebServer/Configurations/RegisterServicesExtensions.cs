namespace Uccs.Fair;

public static class RegisterServicesExtensions
{
	public static IServiceCollection RegisterServices(this IServiceCollection services, FairNode node)
	{
		RegisterFairNode(services, node);
		RegisterServicesInternal(services, node);

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
		services.AddSingleton<StoresService>();
		services.AddSingleton<PerpetualSurveysService>();
		services.AddSingleton<ProductsService>();
		services.AddSingleton<UserService>();
		services.AddSingleton<ModeratorProposalsService>();
		services.AddSingleton<ProposalCommentsService>();
		services.AddSingleton<UnpublishedPublicationsService>();
		services.AddSingleton<UnpublishedStoreProductsService>();
		services.AddSingleton<UsersService>();
	}
}
