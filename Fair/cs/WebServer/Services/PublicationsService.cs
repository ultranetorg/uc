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

		EntityId entityId = EntityId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Find(entityId, mcv.LastConfirmedRound.Id);
			if (publication == null || publication.Status != PublicationStatus.Approved)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			Author author = mcv.Authors.Find(product.Author, mcv.LastConfirmedRound.Id);

			return new PublicationDetailsModel(publication, product, author)
			{
				ProductFields = GetProductFields(publication, product),
			};
		}
	}

	IEnumerable<ProductFieldModel> GetProductFields(Publication publication, Product product)
	{
		return publication.Fields.Select(x => new ProductFieldModel
		{
			Name = x.Name,
			Value = product.Fields.FirstOrDefault(field => field.Name == x.Name)?
						   .Versions.FirstOrDefault(version => version.Version == x.Version)?.Value,
		});
	}

	public TotalItemsResult<PublicationBaseModel> GetAuthorPublicationsNotOptimized(string siteId, string authorId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetAuthorPublicationsNotOptimized)} method called with {{SiteId}}, {{AuthorId}}, {{Page}}, {{PageSize}}", authorId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		EntityId siteEntityId = EntityId.Parse(siteId);
		EntityId authorEntitiId = EntityId.Parse(authorId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			Author author = mcv.Authors.Latest(authorEntitiId);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			var context = new PublicationsContext
			{
				AuthorId = authorEntitiId,
				Page = page,
				PageSize = pageSize,
				Items = new List<PublicationBaseModel>(pageSize)
			};
			SearchInCategories(context, site.Categories, cancellationToken);

			return new TotalItemsResult<PublicationBaseModel>
			{
				TotalItems = context.TotalItems,
				Items = context.Items,
			};
		}
	}

	void SearchInCategories(PublicationsContext context, IEnumerable<EntityId> categoriesIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (EntityId categoryId in categoriesIds)
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

	void SearchInPublications(PublicationsContext context, EntityId[] publicationsIds, CancellationToken cancellationToken)
	{
		foreach (EntityId publicationId in publicationsIds)
		{
			Publication publication = null;
			Product product = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Latest(publicationId);
				product = mcv.Products.Latest(publication.Product);
			}

			if (publication.Status != PublicationStatus.Approved)
			{
				continue;
			}
			if (product.Author != context.AuthorId)
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				var resultItem = new PublicationBaseModel(publication, product);
				context.Items.Add(resultItem);
			}

			++context.TotalItems;
		}
	}

	public TotalItemsResult<ModeratorPublicationModel> GetModeratorPublicationsNonOptimized(string siteId, int page, int pageSize, string? search, CancellationToken canellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublicationsNonOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		EntityId siteEntityId = EntityId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
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
			LoadReviewsRecursively(site.Categories, context, canellationToken);

			return new TotalItemsResult<ModeratorPublicationModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems
			};
		}
	}

	void LoadReviewsRecursively(IEnumerable<EntityId> categoriesIds, FilteredContext<ModeratorPublicationModel> context, CancellationToken canellationToken)
	{
		if (canellationToken.IsCancellationRequested)
			return;

		foreach (var categoryId in categoriesIds)
		{
			if (canellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Find(categoryId, mcv.LastConfirmedRound.Id);
			foreach (var publicationId in category.Publications)
			{
				if (canellationToken.IsCancellationRequested)
					return;

				Publication publication = mcv.Publications.Find(publicationId, mcv.LastConfirmedRound.Id);
				if (publication.Status != PublicationStatus.Pending)
				{
					continue;
				}
				if (!SearchUtils.IsMatch(publication, context.Search))
				{
					continue;
				}

				if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
				{
					Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
					Author author = mcv.Authors.Find(product.Author, mcv.LastConfirmedRound.Id);
					var model = new ModeratorPublicationModel(publication, category, product, author);
					context.Items.Add(model);
				}

				++context.TotalItems;
			}

			LoadReviewsRecursively(category.Categories, context, canellationToken);
		}
	}

	public ModeratorPublicationModel GetModeratorPublication(string publicationId)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublication)} method called with {{PublicationId}}", publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		EntityId publicationEntityId = EntityId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Find(publicationEntityId, mcv.LastConfirmedRound.Id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Category category = mcv.Categories.Find(publication.Category, mcv.LastConfirmedRound.Id);
			Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			Author author = mcv.Authors.Find(product.Author, mcv.LastConfirmedRound.Id);

			return new ModeratorPublicationModel(publication, category, product, author);
		}
	}

	public TotalItemsResult<PublicationModel> SearchPublicationsNotOptimized(string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string? title, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.SearchPublicationsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Title}}", siteId, page, pageSize, title);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NegativeOrZero(pageSize);
		Guard.Against.Negative(page);

		EntityId entitySiteId = EntityId.Parse(siteId);

		Site site = null;
		lock (mcv.Lock)
		{
			site = mcv.Sites.Find(entitySiteId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
		}

		var context = new FilteredContext<PublicationModel>()
		{
			Page = page,
			PageSize = pageSize,
			Search = title,
			Items = new List<PublicationModel>()
		};
		SearchInCategories(context, site.Categories, cancellationToken);

		return new TotalItemsResult<PublicationModel>
		{
			Items = context.Items.Skip(page * pageSize).Take(pageSize),
			TotalItems = context.TotalItems,
		};
	}

	void SearchInCategories(FilteredContext<PublicationModel> context, EntityId[] categoriesIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (EntityId categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = null;

			lock (mcv.Lock)
			{
				category = mcv.Categories.Find(categoryId, mcv.LastConfirmedRound.Id);
			}

			SearchInPublications(context, category, category.Publications, cancellationToken);
			SearchInCategories(context, category.Categories, cancellationToken);
		}
	}

	void SearchInPublications(FilteredContext<PublicationModel> context, Category category, EntityId[] publicationsIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (EntityId publicationId in publicationsIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Publication publication = null;
			Product product = null;
			Author author = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Find(publicationId, mcv.LastConfirmedRound.Id);
				if (publication.Status != PublicationStatus.Approved)
				{
					continue;
				}

				product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
				author = mcv.Authors.Find(publication.Creator, mcv.LastConfirmedRound.Id);
			}

			string productTitle = ProductUtils.GetTitle(product, publication);
			if (string.IsNullOrEmpty(productTitle))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(context.Search) && productTitle.IndexOf(context.Search, StringComparison.OrdinalIgnoreCase) == -1)
			{
				continue;
			}

			PublicationModel resultItem = new PublicationModel(publication, category, author, product);
			context.Items.Add(resultItem);

			++context.TotalItems;
		}
	}

	private class FilteredContext<T> : SearchContext<T> where T : class
	{
		public string? Search { get; set; }
	}

	private class PublicationsContext : SearchContext<PublicationBaseModel>
	{
		public EntityId AuthorId { get; set; }
	}
}
