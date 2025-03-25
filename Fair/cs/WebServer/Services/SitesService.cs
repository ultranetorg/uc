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
	public TotalItemsResult<SiteBaseModel> SearchNonOptimized(int page, int pageSize, string title)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.SearchNonOptimized)} method called with {{Page}}, {{PageSize}}, {{Title}}", page, pageSize, title);

		IEnumerable<Site> sites = null;
		lock (mcv.Lock)
		{
			sites = mcv.Sites.Clusters.SelectMany(x => x.Buckets.SelectMany(x => x.Entries));
		}

		IEnumerable<Site> matched = sites.Where(x => SearchUtils.IsMatch(x, title));
		IEnumerable<Site> result = matched.Skip(page * pageSize).Take(pageSize);
		IEnumerable<SiteBaseModel> items = result.Select(x => new SiteBaseModel(x));

		return new TotalItemsResult<SiteBaseModel>
		{
			Items = items,
			TotalItems = matched.Count(),
		};
	}

	public TotalItemsResult<AuthorBaseModel> GetAuthors(string siteId, int page, int pageSize)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.GetAuthors)} method called with {{SiteId}}, {{Page}}, {{PageSize}}", siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);

		EntityId id = EntityId.Parse(siteId);

		Site site = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				return null;
			}
		}

		IEnumerable<EntityId> authorsIds = site.Authors.Skip(page * pageSize).Take(pageSize);
		IEnumerable<AuthorBaseModel> items = authorsIds.Count() > 0 ? LoadAuthors(authorsIds) : null;

		return new TotalItemsResult<AuthorBaseModel>
		{
			Items = items,
			TotalItems = site.Authors.Count(),
		};
	}

	public TotalItemsResult<CategoryParentBaseModel> GetCategories(string siteId, int page, int pageSize)
	{
		logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.GetCategories)} method called with {{SiteId}}, {{Page}}, {{PageSize}}", siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);

		EntityId id = EntityId.Parse(siteId);

		IEnumerable<Category> categories = null;
		lock (mcv.Lock)
		{
			categories = mcv.Categories.FindBySiteId(id);
		}

		IEnumerable<Category> skippedAndTaken = categories.Skip(page * pageSize).Take(pageSize);
		IEnumerable<CategoryParentBaseModel> items = skippedAndTaken.Select(x => new CategoryParentBaseModel(x));

		return new TotalItemsResult<CategoryParentBaseModel>
		{
			Items = items,
			TotalItems = categories.Count(),
		};
	}

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

		IEnumerable<AuthorBaseModel> authors = site.Authors.Length > 0 ? LoadAuthors(site.Authors) : null;
		IEnumerable<AccountModel> moderators = site.Moderators.Length > 0 ? LoadModerators(site.Moderators) : null;
		IEnumerable<CategoryBaseModel> categories = site.Categories.Length > 0 ? LoadCategories(site.Categories) : null;

		return new SiteModel(site)
		{
			Authors = authors,
			Moderators = moderators,
			Categories = categories,
		};
	}

	private IEnumerable<AuthorBaseModel> LoadAuthors(IEnumerable<EntityId> authorsIds)
	{
		lock (mcv.Lock)
		{
			return authorsIds.Select(id =>
			{
				Author account = mcv.Authors.Find(id, mcv.LastConfirmedRound.Id);
				return new AuthorBaseModel(account);
			}).ToArray();
		}
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

			if (product.Author != authorId)
			{
				continue;
			}

			var resultItem = new PublicationBaseModel(publication, product);
			result.AddLast(resultItem);
		}
	}
}
