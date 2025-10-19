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
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetDefaultSites)} method called without parameters");

		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<SiteBaseModel>();

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

			byte[]? avatar = site.Avatar != null ? mcv.Files.Latest(site.Avatar).Data : null;

			SiteBaseModel model = new SiteBaseModel(site, avatar);
			result.Add(model);
		}

		return result;
	}

	public SiteModel GetSite([NotEmpty] string siteId)
	{
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetSite)} method called with {{SiteId}}", siteId);

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

			IEnumerable<string> moderatorsIds = site.Moderators.Where(x => x.BannedTill.Days == 0).Select(x => x.Account.ToString());
			IEnumerable<string> authorsIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(x => x.Author.ToString());
			(IEnumerable<FairOperationClass> referendumOperations, IEnumerable<FairOperationClass> discussionOperations) =
				GetReferendumDiscussionOperations(site.Policies);

			byte[]? avatar = site.Avatar != null ? mcv.Files.Latest(site.Avatar).Data : null;

			return new SiteModel(site, avatar)
			{
				Categories = categories,
				ModeratorsIds = moderatorsIds,
				AuthorsIds = authorsIds,
				ReferendumOperations = referendumOperations,
				DiscussionOperations = discussionOperations
			};
		}
	}

	(IEnumerable<FairOperationClass> referendumOperations, IEnumerable<FairOperationClass> discussionOperations)
		GetReferendumDiscussionOperations(Policy[] policies)
	{
		List<FairOperationClass> referendumsResult = new (policies.Length);
		List<FairOperationClass> discussionsResult = new (policies.Length);

		foreach (var policy in policies)
		{
			if (policy.Approval == ApprovalRequirement.PublishersMajority)
			{
				referendumsResult.Add(policy.Operation);
			}
			else
			{
				discussionsResult.Add(policy.Operation);
			}
		}

		return (referendumsResult, discussionsResult);
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

	public IEnumerable<AccountBaseModel> GetPublishers([NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetPublishers)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			var publisherIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(p => p.Author).ToArray();
		}

		return null;
	}

	public IEnumerable<AccountBaseModel> GetModerators([NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SitesService)}.{nameof(SitesService.GetModerators)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			var moderatorsIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(p => p.Author).ToArray();

		}

		return null;
	}
}
