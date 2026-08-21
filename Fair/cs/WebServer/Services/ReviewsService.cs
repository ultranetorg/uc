using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ReviewsService
(
#if DEBUG
	ILogger<ReviewsService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<ReviewModel> GetPublicationReviewsNotOptimized([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(publicationId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}, {Page}, {PageSize}", nameof(ReviewsService), nameof(ReviewsService.GetPublicationReviewsNotOptimized), publicationId, page, pageSize);
#endif

		AutoId entityId = AutoId.Parse(publicationId);

		Publication publication = mcv.Publications.Latest(entityId);
		if (publication == null)
		{
			throw new EntityNotFoundException(nameof(Publication), publicationId);
		}

		return LoadReviews(publication.Reviews, page, pageSize, cancellationToken);
	}

	public TotalItemsResult<ReviewModel> GetUserReviewsNotOptimized([NotNull][NotEmpty] string userId, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(userId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}, {Page}, {PageSize}", nameof(ReviewsService), nameof(ReviewsService.GetUserReviewsNotOptimized), userId, page, pageSize);
#endif

		AutoId entityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(entityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		return LoadReviews(user.Reviews, page, pageSize, cancellationToken);
	}

	TotalItemsResult<ReviewModel> LoadReviews(IEnumerable<AutoId> reviewsIds, int page, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return new TotalItemsResult<ReviewModel>{Items = [], TotalItems = reviewsIds.Count()};

		var result = new List<ReviewModel>(pageSize);

		int loadedItems = 0;

		foreach(var id in reviewsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<ReviewModel>{Items = result, TotalItems = reviewsIds.Count()};

			Review review = mcv.Reviews.Latest(id);
			if(review.Status != ReviewStatus.Accepted)
				continue;

			if(loadedItems >= page * pageSize && loadedItems < (page + 1) * pageSize)
			{
				FairUser account = (FairUser)mcv.Users.Latest(review.Creator);
				Publication publication = mcv.Publications.Latest(review.Publication);
				Product product = mcv.Products.Latest(publication.Product);

				var model = new ReviewModel(review, account)
				{
					PublicationId = publication.Id.ToString(),
					PublicationTitle = product.Title
				};
				result.Add(model);
			}

			++loadedItems;
		}

		return new TotalItemsResult<ReviewModel>
		{
			TotalItems = reviewsIds.Count(),
			Items = result
		};
	}
}
