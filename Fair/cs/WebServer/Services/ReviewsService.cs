using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ReviewsService
(
	ILogger<ReviewsService> logger,
	FairMcv mcv
) : IReviewsService
{
	public TotalItemsResult<ModeratorReviewModel> GetModeratorsReviewsNotOptimized(string siteId, int page, int pageSize, string? search, CancellationToken canellationToken)
	{
		logger.LogDebug($"GET {nameof(ReviewsService)}.{nameof(ReviewsService.GetModeratorsReviewsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

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
				Items = new List<ModeratorReviewModel>(pageSize),
			};
			LoadReviewsRecursively(site.Categories, context, canellationToken);

			return new TotalItemsResult<ModeratorReviewModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems
			};
		}
	}

	private void LoadReviewsRecursively(IEnumerable<EntityId> categoriesIds, Context context, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (var categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Latest(categoryId);
			foreach (var publicationId in category.Publications)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				Publication publication = mcv.Publications.Latest(publicationId);
				foreach (var reviewId in publication.Reviews)
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					Review review = mcv.Reviews.Latest(reviewId);
					if (review.Status == ReviewStatus.Pending && SearchUtils.IsMatch(review, context.Search))
					{
						if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
						{
							ModeratorReviewModel model = new (review);
							context.Items.Add(model);
						}

						++context.TotalItems;
					}
				}
			}

			LoadReviewsRecursively(category.Categories, context, cancellationToken);
		}
	}

	public ModeratorReviewDetailsModel GetModeratorReview(string reviewId)
	{
		logger.LogDebug($"GET {nameof(ReviewsService)}.{nameof(ReviewsService.GetModeratorReview)} method called with {{ReviewId}}", reviewId);

		Guard.Against.NullOrEmpty(reviewId);

		EntityId reviewEntityId = EntityId.Parse(reviewId);

		lock (mcv.Lock)
		{
			Review review = mcv.Reviews.Find(reviewEntityId, mcv.LastConfirmedRound.Id);
			if (review == null)
			{
				throw new EntityNotFoundException(nameof(Review).ToLower(), reviewId);
			}

			FairAccount account = (FairAccount) mcv.Accounts.Latest(review.Creator);
			return new ModeratorReviewDetailsModel(review, account);
		}
	}

	public TotalItemsResult<ReviewModel> GetPublicationReviewsNotOptimized(string publicationId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ReviewsService)}.{nameof(ReviewsService.GetPublicationReviewsNotOptimized)} method called with {{ReviewId}}, {{Page}}, {{PageSize}}", publicationId, page, pageSize);

		Guard.Against.NullOrEmpty(publicationId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		EntityId publicationEntityId = EntityId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Latest(publicationEntityId);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication), publicationId);
			}

			var context = new SearchContext<ReviewModel>
			{
				Page = page,
				PageSize = pageSize,
				Items = new List<ReviewModel>(pageSize)
			};
			LoadReviews(context, publication.Reviews, cancellationToken);

			return new TotalItemsResult<ReviewModel>
			{
				TotalItems = context.TotalItems,
				Items = context.Items,
			};
		}
	}

	private void LoadReviews(SearchContext<ReviewModel> context, IEnumerable<EntityId> reviewsIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (var id in reviewsIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Review review = mcv.Reviews.Latest(id);
			if (review.Status != ReviewStatus.Accepted)
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				FairAccount account = (FairAccount) mcv.Accounts.Latest(review.Creator);
				var model = new ReviewModel(review, account);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}

	private class Context : SearchContext<ModeratorReviewModel>
	{
		public string? Search { get; set; }
	}
}
