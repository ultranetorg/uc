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
	public PublicationModel GetPublication(string publicationId)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetPublication)} method called with {{PublicationId}}", publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		EntityId entityId = EntityId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Find(entityId, mcv.LastConfirmedRound.Id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			Author author = mcv.Authors.Find(product.Author, mcv.LastConfirmedRound.Id);

			var reviews = publication.Reviews.Length > 0 ? LoadReviews(publication.Reviews) : null;

			return new PublicationModel(publication, product, author)
			{
				ProductFields = GetProductFields(publication, product),
				Reviews = reviews
			};
		}
	}

	private IEnumerable<PublicationReviewModel> LoadReviews(EntityId[] reviewsIds)
	{
		return reviewsIds.Select(id =>
		{
			Review review = mcv.Reviews.Find(id, mcv.LastConfirmedRound.Id);
			Account account = mcv.Accounts.Find(review.Creator, mcv.LastConfirmedRound.Id);
			return new PublicationReviewModel(review, account);
		}).ToArray();
	}

	private IEnumerable<ProductFieldModel> GetProductFields(Publication publication, Product product)
	{
		return publication.Fields.Select(x => new ProductFieldModel
		{
			Name = x.Name,
			Value = product.Fields.FirstOrDefault(field => field.Name == x.Name)?
						   .Versions.FirstOrDefault(version => version.Version == x.Version)?.Value,
		});
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

			var context = new Context
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

	private void LoadReviewsRecursively(IEnumerable<EntityId> categoriesIds, Context context, CancellationToken canellationToken)
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

	public TotalItemsResult<SitePublicationModel> SearchPublicationsNonOptimized(string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string? title, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.SearchPublicationsNonOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Title}}", siteId, page, pageSize, title);

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
				return TotalItemsResult<SitePublicationModel>.Empty;
			}
		}

		SearchContext context = new SearchContext()
		{
			Page = page,
			PageSize = pageSize,
			SearchTitle = title,
			Result = new List<SitePublicationModel>()
		};
		SearchInCategories(context, site.Categories, cancellationToken);

		return new TotalItemsResult<SitePublicationModel>
		{
			Items = context.Result.Skip(page * pageSize).Take(pageSize),
			TotalItems = context.TotalItems,
		};
	}

	private void SearchInCategories(SearchContext context, EntityId[] categoriesIds, CancellationToken cancellationToken)
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

	private void SearchInPublications(SearchContext context, Category category, EntityId[] publicationsIds, CancellationToken cancellationToken)
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
				//if (publication.Status != PublicationStatus.Approved)
				//{
				//	continue;
				//}

				product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
				author = mcv.Authors.Find(publication.Creator, mcv.LastConfirmedRound.Id);
			}

			string productTitle = ProductUtils.GetTitle(product, publication);
			if (string.IsNullOrEmpty(productTitle))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(context.SearchTitle) && productTitle.IndexOf(context.SearchTitle, StringComparison.OrdinalIgnoreCase) == -1)
			{
				continue;
			}

			SitePublicationModel resultItem = new SitePublicationModel(publication, category, author, product);
			context.Result.Add(resultItem);

			++context.TotalItems;
		}
	}

	class SearchContext
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public string SearchTitle { get; set; }

		public int TotalItems { get; set; }
		public List<SitePublicationModel> Result { get; set; }
	}

	private class Context
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public string? Search { get; set; }

		public int TotalItems { get; set; }
		public IList<ModeratorPublicationModel> Items { get; set; }
	}
}
