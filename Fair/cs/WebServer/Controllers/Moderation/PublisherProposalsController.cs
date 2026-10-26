using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/publishers")]
public class PublisherProposalsController
(
#if DEBUG
	ILogger<ModeratorProposalsController> logger,
#endif
	ModeratorProposalsService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PublisherProposalModel> GetAll(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Search}, {Pagination}", nameof(PublisherProposalsController), nameof(GetAll), storeId, search, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherProposalModel> discussions = proposalsService.GetPublisherProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}
}
