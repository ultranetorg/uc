using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/reviews")]
public class ReviewProposalsController
(
#if DEBUG
	ILogger<ReviewProposalsController> logger,
#endif
	ModeratorReviewProposalsService moderatorReviewProposalsService
) : BaseController
{
	[HttpGet("creations")]
	public IEnumerable<ReviewProposalModel> GetCreations(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ReviewProposalsController), nameof(ReviewProposalsController.GetCreations), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewProposalModel> reviews = moderatorReviewProposalsService.GetCreationsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}

	[HttpGet("deletions")]
	public IEnumerable<ReviewProposalModel> GetDeletions(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ReviewProposalsController), nameof(ReviewProposalsController.GetDeletions), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewProposalModel> reviews = moderatorReviewProposalsService.GetDeletionsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
