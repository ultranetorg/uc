using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/reviews")]
public class ModeratorReviewsController
(
	ILogger<ModeratorReviewsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	IReviewsService reviewsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ModeratorReviewModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ModeratorReviewModel> reviews = reviewsService.GetModeratorReviewProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}

	[HttpGet("~/api/moderator/reviews/{reviewId}")]
	public ModeratorReviewModel Get(string reviewId)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{ReviewId}}", reviewId);

		autoIdValidator.Validate(reviewId, nameof(Review).ToLower());

		return reviewsService.GetModeratorReview(reviewId);
	}
}
