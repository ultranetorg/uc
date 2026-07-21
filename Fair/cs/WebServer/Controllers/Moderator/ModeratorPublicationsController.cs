using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/publications")]
public class ModeratorPublicationsController
(
	ILogger<ModeratorPublicationsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PublicationProposalModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ModeratorPublicationsController), nameof(GetAll), storeId, pagination, search);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationProposalModel> result = moderatorProposalService.GetPublicationsProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}
