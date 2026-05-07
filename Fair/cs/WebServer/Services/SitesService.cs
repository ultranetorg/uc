using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

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

	public SiteModel GetDetails([NotEmpty] string siteId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(SitesService), nameof(GetDetails), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			IEnumerable<string> moderatorsIds = site.Moderators.Where(x => x.BannedTill.Days == 0).Select(x => x.User.ToString());
			IEnumerable<string> authorsIds = site.Publishers.Where(x => x.BannedTill.Days == 0).Select(x => x.Author.ToString());

			return new SiteModel(site)
			{
				ModeratorsIds = moderatorsIds,
				AuthorsIds = authorsIds,
			};
		}
	}

	public TotalItemsResult<PublisherModel> GetPublishers([NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}", nameof(SitesService), nameof(GetPublishers), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);

		lock(mcv.Lock)
		{
			AutoId id = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			if(!string.IsNullOrEmpty(search) && AutoId.TryParse(search, out AutoId parsedId))
			{
				Publisher publisher = site.Publishers.FirstOrDefault(x => x.Author == parsedId);
				if(publisher == null)
				{
					return TotalItemsResult<PublisherModel>.Empty;
				}

				Author author = mcv.Authors.Latest(publisher.Author);
				var model = new PublisherModel(author, publisher);
				return new TotalItemsResult<PublisherModel> {Items = [model], TotalItems = site.Publishers.Length};
			}

			return LoadPublishers(site.Publishers, page, pageSize, search, cancellationToken);
		}
	}

	TotalItemsResult<PublisherModel> LoadPublishers(IEnumerable<Publisher> publishers, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<PublisherModel>.Empty;

		List<PublisherModel> items = new(pageSize);
		int totalItems = 0;

		foreach(Publisher publisher in publishers)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };

			Author author = mcv.Authors.Latest(publisher.Author);
			if (!SearchUtils.IsMatch(author, search))
				continue;

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				var model = new PublisherModel(author, publisher);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };
	}

	public IEnumerable<ModeratorModel> GetModerators([NotEmpty] string siteId, CancellationToken cancellationToken)
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

			return LoadModerators(site.Moderators, cancellationToken);
		}
	}

	public IEnumerable<ModeratorModel> LoadModerators(Moderator[] moderators, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<ModeratorModel> result = new(moderators.Length);

		foreach(Moderator moderator in moderators)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			FairUser account = (FairUser) mcv.Users.Latest(moderator.User);
			var model = new ModeratorModel(account, moderator.BannedTill);
			result.Add(model);
		}

		return result;
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

			return site.Policies.Select(x => new PolicyModel(x));
		}
	}
}
