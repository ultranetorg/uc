using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Fair;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class SitesService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
) : ISitesService
{
	public SiteModel Find(string siteId)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.Find)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		EntityId id = EntityId.Parse(siteId);

		SiteEntry site = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				return null;
			}
		}

		CategorySubModel[] categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : null;

		return ToSiteModel(site, categories);
	}

	private CategorySubModel[] LoadCategories(EntityId[] categoriesIds)
	{
		lock (mcv.Lock)

		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			return new CategorySubModel(category);
		}).ToArray();
	}

	private static SiteModel ToSiteModel(Site site, CategorySubModel[] categories)
	{
		return new SiteModel
		{
			Id = site.Id.ToString(),
			Type = site.Type,
			Title = site.Title,
			Categories = categories
		};
	}

	public TotalItemsResult<SitePublicationModel> SearchPublications(string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.SearchPublications)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Name}}", siteId, page, pageSize, name);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NegativeOrZero(pageSize);
		Guard.Against.Negative(page);

		EntityId id = EntityId.Parse(siteId);

		Site site = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
		}

		// TODO: optimize search.
		SearchContext context = new SearchContext()
		{
			Page = page,
			PageSize = pageSize,
			SearchName = name,
			Result = new List<SitePublicationModel>()
		};
		SearchInCategories(context, site.Categories);

		return new TotalItemsResult<SitePublicationModel>
		{
			Items = context.Result.Skip(page * pageSize).Take(pageSize),
			TotalItems = context.TotalItems,
		};
	}

	private void SearchInCategories(SearchContext context, EntityId[] categoriesIds)
	{
		foreach (EntityId categoryId in categoriesIds)
		{
			Category category = null;

			lock (mcv.Lock)
			{
				category = mcv.Categories.Find(categoryId, mcv.LastConfirmedRound.Id);
			}

			SearchInPublications(context, category, category.Publications);
			SearchInCategories(context, category.Categories);
		}
	}

	private void SearchInPublications(SearchContext context, Category category, EntityId[] publicationsIds)
	{
		foreach(EntityId publicationId in publicationsIds)
		{
			Publication publication = null;
			Product product = null;
			Author author = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Find(publicationId, mcv.LastConfirmedRound.Id);
				product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
				author = mcv.Authors.Find(publication.Creator, mcv.LastConfirmedRound.Id);
			}

			string productTitle = ProductUtils.GetTitle(product);
			// TODO: is SearchName can be empty?
			if (!string.IsNullOrEmpty(context.SearchName) && (productTitle == null || productTitle.IndexOf(context.SearchName, StringComparison.OrdinalIgnoreCase) == -1))
			{
				continue;
			}


			SitePublicationModel newItem = ToPublicationSearchModel(publication, category, product, author);
			context.Result.Add(newItem);

			++context.TotalItems;
		}
	}

	private static SitePublicationModel ToPublicationSearchModel(Publication publication, Category category, Product product, Author author)
	{
		return new SitePublicationModel
		{
			Id = publication.Id.ToString(),
			CategoryId = category.Id.ToString(),
			CategoryName = category.Title,
			ProductId = product.Id.ToString(),
			ProductName = ProductUtils.GetTitle(product) ?? "TEST NAME + " + publication.Id.ToString(),
			AuthorId = publication.Creator.ToString(),
			AuthorTitle = author.Title,
		};
	}

	class SearchContext
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public string SearchName { get; set; }

		public int TotalItems { get; set; }
		public List<SitePublicationModel> Result { get; set; }
	}
}
