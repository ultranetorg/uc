using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/moderators")]
public class ModeratorProposalsController
(
#if DEBUG
	ILogger<ModeratorProposalsController> logger,
#endif
	ModeratorProposalsService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ModeratorProposalModel> GetAll(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Search}, {Pagination}", nameof(ModeratorProposalsController), nameof(GetAll), storeId, search, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ModeratorProposalModel> discussions = proposalsService.GetModeratorProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}
}
