using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsService
(
	ILogger<PublicationsService> logger,
	FairMcv mcv
) : IPublicationsService
{
	public PublicationDetailsModel GetPublication(string publicationId)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetPublication)} method called with {{PublicationId}}", publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		AutoId id = AutoId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Latest(id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);

			return new PublicationDetailsModel(publication, product, author, category)
			{
				// TODO: calculate average rating.
				AverageRating = 31,
			};
		}
	}

	public TotalItemsResult<PublicationAuthorModel> GetAuthorPublicationsNotOptimized(string siteId, string authorId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetAuthorPublicationsNotOptimized)} method called with {{SiteId}}, {{AuthorId}}, {{Page}}, {{PageSize}}", siteId, authorId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId siteAutoId = AutoId.Parse(siteId);
		AutoId authorAutoId = AutoId.Parse(authorId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteAutoId);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			Author author = mcv.Authors.Latest(authorAutoId);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			var context = new PublicationsContext
			{
				AuthorId = authorAutoId,
				Page = page,
				PageSize = pageSize,
				Items = new List<PublicationAuthorModel>(pageSize)
			};
			SearchInCategories(context, site.Categories, cancellationToken);

			return new TotalItemsResult<PublicationAuthorModel>
			{
				TotalItems = context.TotalItems,
				Items = context.Items,
			};
		}
	}

	void SearchInCategories(PublicationsContext context, IEnumerable<AutoId> categoriesIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (AutoId categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = null;

			lock (mcv.Lock)
			{
				category = mcv.Categories.Latest(categoryId);
			}

			SearchInPublications(context, category.Publications, cancellationToken);
			SearchInCategories(context, category.Categories, cancellationToken);
		}
	}

	void SearchInPublications(PublicationsContext context, AutoId[] publicationsIds, CancellationToken cancellationToken)
	{
		foreach (AutoId publicationId in publicationsIds)
		{
			Publication publication = null;
			Product product = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Latest(publicationId);
				product = mcv.Products.Latest(publication.Product);
			}

			if (product.Author != context.AuthorId)
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				var resultItem = new PublicationAuthorModel(publication, product);
				context.Items.Add(resultItem);
			}

			++context.TotalItems;
		}
	}

	public TotalItemsResult<PublicationModel> GetCategoryPublicationsNotOptimized(string categoryId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetCategoryPublicationsNotOptimized)} method called with {{CategoryId}}", categoryId);

		Guard.Against.NullOrEmpty(categoryId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(categoryId);

		lock (mcv.Lock)
		{
			Category category = mcv.Categories.Latest(id);
			if (category == null)
			{
				throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
			}

			if (category.Publications.Length == 0)
			{
				return TotalItemsResult<PublicationModel>.Empty;
			}

			var context = new SearchContext<PublicationModel>
			{
				Page = page,
				PageSize = pageSize,
				Items = new List<PublicationModel>(pageSize)
			};
			LoadPublications(category, context, cancellationToken);

			return new TotalItemsResult<PublicationModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems
			};
		}
	}

	void LoadPublications(Category category, SearchContext<PublicationModel> context, CancellationToken cancellationToken)
	{
		foreach (AutoId publicationId in category.Publications)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				Publication publication = mcv.Publications.Latest(publicationId);
				Product product = mcv.Products.Latest(publication.Product);
				var model = new PublicationModel(publication, product, category);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}

	public TotalItemsResult<ModeratorPublicationModel> GetModeratorPublicationsNotOptimized(string siteId, int page, int pageSize, string? search, CancellationToken canellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublicationsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			var context = new FilteredContext<ModeratorPublicationModel>
			{
				Page = page,
				PageSize = pageSize,
				Search = search,
				Items = new List<ModeratorPublicationModel>(pageSize),
			};

			LoadModeratorsPendingPublications(site.UnpublishedPublications, context, canellationToken);

			return new TotalItemsResult<ModeratorPublicationModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems
			};
		}
	}

	void LoadModeratorsPendingPublications(IEnumerable<AutoId> pendingPublicationsIds, FilteredContext<ModeratorPublicationModel> context, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (var publicationId in pendingPublicationsIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;


			Publication publication = mcv.Publications.Latest(publicationId);

			if (!SearchUtils.IsMatch(publication, context.Search))
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				Product product = mcv.Products.Latest(publication.Product);
				Author author = mcv.Authors.Latest(product.Author);
				Category category = mcv.Categories.Latest(publication.Category);
				var model = new ModeratorPublicationModel(publication, category, product, author);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}

	public ModeratorPublicationModel GetModeratorPublication(string publicationId)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublication)} method called with {{PublicationId}}", publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		AutoId id = AutoId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Latest(id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Category category = mcv.Categories.Latest(publication.Category);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);

			return new ModeratorPublicationModel(publication, category, product, author);
		}
	}

	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublicationsNotOptimized([NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetCategoriesPublicationsNotOptimized)} method called with {{SiteId}}", siteId);

		Guard.Against.NullOrEmpty(siteId);

		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<CategoryPublicationsModel>();

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site), siteId);
			}
			if (site.Categories.Length == 0)
			{
				return Enumerable.Empty<CategoryPublicationsModel>();
			}

			var result = new List<CategoryPublicationsModel>(site.Categories.Length);
			LoadCategoriesPublications(site.Categories, result, cancellationToken);

			return result;
		}
	}

	void LoadCategoriesPublications(AutoId[] categoriesIds, IList<CategoryPublicationsModel> result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (AutoId categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Latest(categoryId);

			var resultCategory = new CategoryPublicationsModel(category)
			{
				Publications = new List<PublicationExtendedModel>(CategoriesPublications.DefaultPublicationsCount)
			};

			LoadPublicationsFromCategory(category, resultCategory, cancellationToken);

			if (cancellationToken.IsCancellationRequested)
				return;

			if (resultCategory.Publications.Count > 0)
			{
				result.Add(resultCategory);
			}
		}
	}

	void LoadPublicationsFromCategory(Category category, CategoryPublicationsModel result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		if (result.Publications.Count >= CategoriesPublications.DefaultPublicationsCount)
		{
			return;
		}

		foreach (AutoId publicationId in category.Publications)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			PublicationExtendedModel model = new PublicationExtendedModel(publication, product, author, category);
			result.Publications.Add(model);

			if (result.Publications.Count >= CategoriesPublications.DefaultPublicationsCount)
			{
				return;
			}
		}

		foreach (AutoId subCategoryId in category.Categories)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category subCategory = mcv.Categories.Latest(subCategoryId);
			LoadPublicationsFromCategory(subCategory, result, cancellationToken);
		}
	}

	private class FilteredContext<T> : SearchContext<T> where T : class
	{
		public string? Search { get; set; }
	}

	private class PublicationsContext : SearchContext<PublicationAuthorModel>
	{
		public AutoId AuthorId { get; set; }
	}
}
