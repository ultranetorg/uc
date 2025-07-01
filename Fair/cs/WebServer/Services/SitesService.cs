using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class SitesService
(
	ILogger<SitesService> logger,
	FairMcv mcv
) : ISitesService
{
	public IEnumerable<SiteBaseModel> GetDefaultSites(CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetDefaultSites)} method called without parameters");

		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<SiteBaseModel>();

		lock (mcv.Lock)
		{
			var result = new List<SiteBaseModel>(SiteConstants.DEFAULT_SITES_IDS.Length);
			return LoadSites(SiteConstants.DEFAULT_SITES_IDS, result, cancellationToken);
		}
	}

	IList<SiteBaseModel> LoadSites(AutoId[] sitesIds, IList<SiteBaseModel> result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return result;

		foreach (AutoId siteId in sitesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return result;

			Site site = mcv.Sites.Latest(siteId);
			if (site == null)
			{
				continue;
			}

			SiteBaseModel model = new SiteBaseModel(site);
			result.Add(model);
		}

		return result;
	}

	public SiteModel GetSite(string siteId)
	{
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetSite)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			//IEnumerable<AccountBaseModel> moderators = site.Moderators.Length > 0 ? LoadModerators(site.Moderators) : [];
			IEnumerable<CategoryBaseModel> categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : [];

			return new SiteModel(site)
			{
				//Moderators = moderators,
				Categories = categories,
			};
		}
	}

	//IEnumerable<AccountBaseModel> LoadModerators(AutoId[] moderatorsIds)
	//{
	//	return moderatorsIds.Select(id =>
	//	{
	//		FairAccount account = (FairAccount) mcv.Accounts.Latest(id);
	//		return new AccountBaseModel(account);
	//	}).ToArray();
	//}

	IEnumerable<CategoryBaseModel> LoadCategories(AutoId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Latest(id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}
}
