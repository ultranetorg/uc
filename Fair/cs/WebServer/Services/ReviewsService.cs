using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ReviewsService
(
	ILogger<ReviewsService> logger,
	FairMcv mcv
) : IReviewsService
{
	public TotalItemsResult<ReviewModel> GetModeratorsReviewsNonOptimized(string siteId, int page, int pageSize, string? search, CancellationToken canellationToken)
	{
		logger.LogDebug($"GET {nameof(ReviewsService)}.{nameof(ReviewsService.GetModeratorsReviewsNonOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

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
				Items = new List<ReviewModel>(pageSize),
			};
			LoadReviewsRecursively(site.Categories, context, canellationToken);

			return new TotalItemsResult<ReviewModel>
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

			Category category = mcv.Categories.Find(categoryId, mcv.LastConfirmedRound.Id);
			foreach (var publicationId in category.Publications)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				Publication publication = mcv.Publications.Find(publicationId, mcv.LastConfirmedRound.Id);
				foreach (var reviewId in publication.Reviews)
				{
					if (cancellationToken.IsCancellationRequested)
						return;

					Review review = mcv.Reviews.Find(reviewId, mcv.LastConfirmedRound.Id);
					if (review.Status == ReviewStatus.Pending && SearchUtils.IsMatch(review, context.Search))
					{
						if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
						{
							var model = new ReviewModel(review);
							context.Items.Add(model);
						}

						++context.TotalItems;
					}
				}
			}

			LoadReviewsRecursively(category.Categories, context, cancellationToken);
		}
	}

	public ReviewDetailsModel GetModeratorReview(string reviewId)
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
			Account account = mcv.Accounts.Find(review.Creator, mcv.LastConfirmedRound.Id);

			return new ReviewDetailsModel(review, account);
		}
	}

	private class Context
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public string? Search { get; set; }

		public int TotalItems { get; set; }
		public IList<ReviewModel> Items { get; set; }
	}
}
