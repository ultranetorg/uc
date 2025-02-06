using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
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

		IEnumerable<AccountModel> moderators = site.Moderators.Length > 0 ? LoadModerators(site.Moderators) : null;
		IEnumerable<CategoryBaseModel> categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : null;

		return new SiteModel(site)
		{
			Moderators = moderators,
			Categories = categories,
		};
	}

	private IEnumerable<AccountModel> LoadModerators(EntityId[] moderatorsIds)
	{
		lock (mcv.Lock)
		{
			return moderatorsIds.Select(id =>
			{
				Account account = mcv.Accounts.Find(id, mcv.LastConfirmedRound.Id);
				return new AccountModel(account);
			}).ToArray();
		}
	}

	private IEnumerable<CategoryBaseModel> LoadCategories(EntityId[] categoriesIds)
	{
		lock (mcv.Lock)
		{
			return categoriesIds.Select(id =>
			{
				Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
				return new CategoryBaseModel(category);
			}).ToArray();
		}
	}

	public SiteAuthorModel FindAuthorNonOptimized(string siteId, string authorId)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.FindAuthorNonOptimized)} method called with {{SiteId}}, {{AuthorId}}", siteId, authorId);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(authorId);

		EntityId siteEntityId = EntityId.Parse(siteId);
		EntityId authorEntitiId = EntityId.Parse(authorId);

		Site site = null;
		AuthorEntry author = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				return null;
			}

			author = mcv.Authors.Find(authorEntitiId, mcv.LastConfirmedRound.Id);
			if (author == null)
			{
				return null;
			}
		}

		IEnumerable<PublicationBaseModel> publication = author.Products.Length > 0 ? LoadPublications(site.Categories, authorEntitiId) : null;

		return new SiteAuthorModel(author)
		{
			OwnerId = author.Owner.ToString(),
			Publications = publication,
		};
	}

	private IEnumerable<PublicationBaseModel> LoadPublications(EntityId[] categoriesIds, EntityId authorId)
	{
		LinkedList<PublicationBaseModel> result = new LinkedList<PublicationBaseModel>();

		SearchInCategories(categoriesIds, authorId, ref result);

		return result;
	}

	private void SearchInCategories(EntityId[] categoriesIds, EntityId authorId, ref LinkedList<PublicationBaseModel> result)
	{
		foreach (EntityId categoryId in categoriesIds)
		{
			Category category = null;

			lock (mcv.Lock)
			{
				category = mcv.Categories.Find(categoryId, mcv.LastConfirmedRound.Id);
			}

			SearchInPublications(category.Publications, authorId, ref result);
			SearchInCategories(category.Categories, authorId, ref result);
		}
	}

	private void SearchInPublications(EntityId[] publicationsIds, EntityId authorId, ref LinkedList<PublicationBaseModel> result)
	{
		foreach (EntityId publicationId in publicationsIds)
		{
			Publication publication = null;
			Product product = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Find(publicationId, mcv.LastConfirmedRound.Id);
				product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			}

			if (product.AuthorId != authorId)
			{
				continue;
			}

			var resultItem = new PublicationBaseModel(publicationId, product);
			result.AddLast(resultItem);
		}
	}

	public TotalItemsResult<SitePublicationModel> SearchPublicationsNonOptimized(string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.SearchPublicationsNonOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Name}}", siteId, page, pageSize, name);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NegativeOrZero(pageSize);
		Guard.Against.Negative(page);

		EntityId id = EntityId.Parse(siteId);

		Site site = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				return TotalItemsResult<SitePublicationModel>.Empty;
			}
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
			if (string.IsNullOrEmpty(productTitle))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(context.SearchName) && productTitle.IndexOf(context.SearchName, StringComparison.OrdinalIgnoreCase) == -1)
			{
				continue;
			}

			SitePublicationModel resultItem = new SitePublicationModel(publication.Id, category, author, product);
			context.Result.Add(resultItem);

			++context.TotalItems;
		}
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
