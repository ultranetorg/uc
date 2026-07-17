using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationReviewsController
(
	ILogger<PublicationReviewsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ReviewsService reviewsService
) : BaseController
{
	[HttpGet("~/api/publications/{publicationId}/reviews")]
	public IEnumerable<ReviewModel> Get(string publicationId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationReviewsController)}.{nameof(PublicationReviewsController.Get)} method called with {{PublicationId}}, {{Pagination}}", publicationId, pagination);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> reviews = reviewsService.GetPublicationReviewsNotOptimized(publicationId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
