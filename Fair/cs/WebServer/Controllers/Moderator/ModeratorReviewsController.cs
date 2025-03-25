using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/reviews")]
public class ModeratorReviewsController
(
	ILogger<ModeratorReviewsController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	IReviewsService reviewsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ReviewModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<ReviewModel> disputes = reviewsService.GetModeratorsReviewsNonOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(disputes.Items, page, pageSize, disputes.TotalItems);
	}

	[HttpGet("~/api/moderator/reviews/{reviewId}")]
	public ReviewModel Get(string reviewId)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{ReviewId}}", reviewId);

		entityIdValidator.Validate(reviewId, nameof(Review).ToLower());

		return reviewsService.GetModeratorReview(reviewId);
	}
}
