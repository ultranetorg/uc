using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ReviewsService
(
	ILogger<ReviewsService> logger,
	FairMcv mcv
) : IReviewsService
{
	public TotalItemsResult<ReviewModel> GetPublicationReviewsNotOptimized(string publicationId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ReviewsService)}.{nameof(ReviewsService.GetPublicationReviewsNotOptimized)} method called with {{ReviewId}}, {{Page}}, {{PageSize}}", publicationId, page, pageSize);

		Guard.Against.NullOrEmpty(publicationId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId publicationEntityId = AutoId.Parse(publicationId);

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

	private void LoadReviews(SearchContext<ReviewModel> context, IEnumerable<AutoId> reviewsIds, CancellationToken cancellationToken)
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
				FairUser account = (FairUser) mcv.Users.Latest(review.Creator);
				var model = new ReviewModel(review, account);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}
}
