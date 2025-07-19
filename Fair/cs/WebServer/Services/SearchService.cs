using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SearchService
(
	ILogger<SearchService> logger,
	FairMcv mcv
) : ISearchService
{
	public TotalItemsResult<PublicationExtendedModel> SearchPublications(string siteId, string query, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchPublications)} method called with {{SiteId}}, {{Query}}, {{PageSize}}", siteId, query, pageSize);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			SearchResult[] searchResult = mcv.Publications.Search(id, query, page * pageSize, pageSize);
			if (searchResult.Length == 0)
			{
				return TotalItemsResult<PublicationExtendedModel>.Empty;
			}

			List<PublicationExtendedModel> result = new List<PublicationExtendedModel>(searchResult.Length);
			LoadPublications(searchResult, result, cancellationToken);

			return new TotalItemsResult<PublicationExtendedModel>
			{
				Items = result,
				TotalItems = Math.Min(site.PublicationsCount, Pagination.PageSize30 * Pagination.PagesCountMax)
			};
		}
	}

	void LoadPublications(SearchResult[] searchResult, IList<PublicationExtendedModel> result, CancellationToken cancellationToken)
	{
		foreach (SearchResult search in searchResult)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			Publication publication = mcv.Publications.Latest(search.Entity);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);
			byte[]? logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;

			PublicationExtendedModel model = new PublicationExtendedModel(publication, product, author, category, logo);
			result.Add(model);
		}
	}

	public IEnumerable<PublicationBaseModel> SearchLitePublications(string siteId, string query, int page, int pageSize, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<PublicationBaseModel>();

		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLitePublications)} method called with {{SiteId}}, {{Query}}, {{Page}}, {{PageSize}}", siteId, query, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			SearchResult[] result = mcv.Publications.Search(id, query, page * pageSize, pageSize);
			return result.Select(x => new PublicationBaseModel(x.Entity.ToString(), x.Text));
		}
	}

	public TotalItemsResult<SiteBaseModel> SearchSites(string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<SiteBaseModel>.Empty;

		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchSites)} method called with{{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock (mcv.Lock)
		{
			SearchResult[] searchResult = mcv.Sites.Search(query ?? "", page * pageSize, pageSize);
			if (searchResult.Length == 0)
			{
				return TotalItemsResult<SiteBaseModel>.Empty;
			}

			List<SiteBaseModel> result = new List<SiteBaseModel>(searchResult.Length);
			LoadSites(searchResult, result, cancellationToken);

			return new TotalItemsResult<SiteBaseModel>
			{
				Items = result,
				TotalItems = BitConverter.ToInt32(mcv.Metas.Latest(new MetaId((int) FairMetaEntityType.SitesCount)).Value)
			};
		}
	}

	void LoadSites(SearchResult[] searchResult, IList<SiteBaseModel> result, CancellationToken cancellationToken)
	{
		foreach (SearchResult search in searchResult)
		{
			if (cancellationToken.IsCancellationRequested)
				break;

			Site site = mcv.Sites.Latest(search.Entity);
			byte[]? avatar = site.Avatar != null ? mcv.Files.Latest(site.Avatar).Data : null;

			SiteBaseModel model = new SiteBaseModel(site, avatar);
			result.Add(model);
		}
	}

	public IEnumerable<SiteSearchLiteModel> SearchLiteSites([NotEmpty, NotNull] string query, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<SiteSearchLiteModel>();

		logger.LogDebug($"{nameof(SearchService)}.{nameof(SearchService.SearchLiteSites)} method called with{{Query}}, {{Page}}, {{PageSize}}", query, page, pageSize);

		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock (mcv.Lock)
		{
			SearchResult[] result = mcv.Sites.Search(query, page * pageSize, pageSize);
			return result.Select(x => new SiteSearchLiteModel(x.Entity.ToString(), x.Text));
		}
	}
}