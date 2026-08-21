using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationReviewsController
(
#if DEBUG
	ILogger<PublicationReviewsController> logger,
#endif
	ReviewsService reviewsService
) : BaseController
{
	[HttpGet("~/api/publications/{publicationId}/reviews")]
	public IEnumerable<ReviewModel> Get(string publicationId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {PublicationId}, {Pagination}", nameof(PublicationReviewsController), nameof(PublicationReviewsController.Get), publicationId, pagination);
#endif

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> reviews = reviewsService.GetPublicationReviewsNotOptimized(publicationId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
