using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesService
(
	ILogger<SitesService> logger,
	FairMcv mcv
) : ISitesService
{
	public TotalItemsResult<SiteBaseModel> SearchNonOptimized(int page, int pageSize, string? search)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.SearchNonOptimized)} method called with {{Page}}, {{PageSize}}, {{Title}}", page, pageSize, search);

		IEnumerable<Site> sites = null;
		lock (mcv.Lock)
		{
			sites = mcv.Sites.TailBaseIntities;
		}

		IEnumerable<Site> matched = sites.Where(x => SearchUtils.IsMatch(x, search));
		IEnumerable<Site> result = matched.Skip(page * pageSize).Take(pageSize);
		IEnumerable<SiteBaseModel> items = result.Select(x => new SiteBaseModel(x));

		return new TotalItemsResult<SiteBaseModel>
		{
			Items = items,
			TotalItems = matched.Count(),
		};
	}

	public SiteModel GetSite(string siteId)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.GetSite)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		EntityId id = EntityId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			IEnumerable<AccountBaseModel> moderators = site.Moderators.Length > 0 ? LoadModerators(site.Moderators) : [];
			IEnumerable<CategoryBaseModel> categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : [];

			return new SiteModel(site)
			{
				Moderators = moderators,
				Categories = categories,
			};
		}
	}

	IEnumerable<AccountBaseModel> LoadModerators(EntityId[] moderatorsIds)
	{
		return moderatorsIds.Select(id =>
		{
			FairAccount account = (FairAccount) mcv.Accounts.Latest(id);
			return new AccountBaseModel(account);
		}).ToArray();
	}

	IEnumerable<CategoryBaseModel> LoadCategories(EntityId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Latest(id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}
}
