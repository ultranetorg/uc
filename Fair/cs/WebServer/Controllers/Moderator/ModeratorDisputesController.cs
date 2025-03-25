using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/disputes")]
public class ModeratorDisputesController
(
	ILogger<ModeratorDisputesController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	IDisputesService disputesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<DisputeModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDisputesController)}.{nameof(ModeratorDisputesController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<DisputeModel> disputes = disputesService.GetDisputes(siteId, true, page, pageSize, search, cancellationToken);

		return this.OkPaged(disputes.Items, page, pageSize, disputes.TotalItems);
	}

	[HttpGet("{disputeId}")]
	public DisputeModel Get(string siteId, string disputeId)
	{
		logger.LogInformation($"GET {nameof(ModeratorDisputesController)}.{nameof(ModeratorDisputesController.Get)} method called with {{SiteId}}, {{DisputeId}}", siteId, disputeId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(disputeId, nameof(Dispute).ToLower());

		return disputesService.GetDispute(siteId, disputeId, true);
	}
}
