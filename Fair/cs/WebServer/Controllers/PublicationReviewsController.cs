using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationReviewsController
(
	ILogger<PublicationReviewsController> logger,
	ReviewsService reviewsService
) : BaseController
{
	[HttpGet("~/api/publications/{publicationId}/reviews")]
	public IEnumerable<ReviewModel> Get(string publicationId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationReviewsController)}.{nameof(PublicationReviewsController.Get)} method called with {{PublicationId}}, {{Pagination}}", publicationId, pagination);

		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> reviews = reviewsService.GetPublicationReviewsNotOptimized(publicationId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
