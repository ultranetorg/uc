using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/publications")]
public class PublicationsProposalsController
(
#if DEBUG
	ILogger<PublicationsProposalsController> logger,
#endif
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PublicationProposalModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetAll), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationProposalModel> result = moderatorProposalService.GetPublicationProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}
