using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/publications/{publicationId}/[controller]")]
public class ReviewsController
(
	ILogger<ReviewsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	IReviewsService reviewsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ReviewModel> Get(string publicationId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ReviewsController)}.{nameof(ReviewsController.Get)} method called with {{PublicationId}}, {{Pagination}}", publicationId, pagination);

		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewModel> reviews = reviewsService.GetPublicationReviewsNotOptimized(publicationId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
