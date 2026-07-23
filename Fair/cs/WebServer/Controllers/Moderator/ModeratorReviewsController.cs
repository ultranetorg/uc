using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/reviews")]
public class ModeratorReviewsController
(
	ILogger<ModeratorReviewsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ReviewProposalModel> Get(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ModeratorReviewsController), nameof(Get), storeId, pagination, search);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewProposalModel> reviews = moderatorProposalsService.GetReviewProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
