using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class SitesService
(
	ILogger<SitesService> logger,
	FairMcv mcv
)
{
	public IEnumerable<SiteBaseModel> GetDefaultSites(CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called", nameof(SitesService), nameof(SitesService.GetDefaultSites));

		if (cancellationToken.IsCancellationRequested)
			return [];

		lock (mcv.Lock)
		{
			var result = new List<SiteBaseModel>(SiteConstants.DefaultSitesIds.Length);
			return LoadSites(SiteConstants.DefaultSitesIds, result, cancellationToken);
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

			var model = new SiteBaseModel(site);
			result.Add(model);
		}

		return result;
	}

	public SiteModel GetSite([NotEmpty] string siteId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(SitesService), nameof(GetSite), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			IEnumerable<SiteCategoryModel> categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : [];

			IEnumerable<string> moderatorsIds = site.Moderators.Where(x => x.BannedTill.Days == 0).Select(x => x.User.ToString());
			IEnumerable<string> authorsIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(x => x.Author.ToString());

			return new SiteModel(site)
			{
				Categories = categories,
				ModeratorsIds = moderatorsIds,
				AuthorsIds = authorsIds,
			};
		}
	}

	IEnumerable<SiteCategoryModel> LoadCategories(AutoId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Latest(id);
			byte[]? avatar = category.Avatar != null ? mcv.Files.Latest(category.Avatar).Data : null;
			return new SiteCategoryModel(category, avatar);
		}).ToArray();
	}

	public IEnumerable<AuthorBaseAvatarModel> GetPublishers([NotEmpty][NotNull] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(SitesService), nameof(GetPublishers), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId[] publishersIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(p => p.Author).ToArray();
			return McvUtils.LoadAuthors(mcv, publishersIds, cancellationToken);
		}
	}

	public IEnumerable<AccountBaseModel> GetModerators([NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(SitesService), nameof(GetModerators), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId[] moderatorsIds = site.Moderators.Where(x => x.BannedTill.Days == 0).Select(p => p.User).ToArray();
			return McvUtils.LoadAccounts(mcv, moderatorsIds, cancellationToken);
		}
	}

	public IEnumerable<PolicyModel> GetPolicies(string siteId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(SitesService), nameof(GetPolicies), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			List<PolicyModel> result = new List<PolicyModel>(site.Policies.Length);
			foreach(Policy policy in site.Policies)
			{
				if(policy.OperationClass == FairOperationClass.SiteModeratorRemoval)
					continue;

				var model = new PolicyModel(policy);
				result.Add(model);
			}

			return result;
		}
	}
}
