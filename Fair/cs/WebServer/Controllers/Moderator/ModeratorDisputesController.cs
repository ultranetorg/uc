using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/disputes")]
public class ModeratorDisputesController
(
	ILogger<ModeratorDisputesController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	IProposalService disputesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDisputesController)}.{nameof(ModeratorDisputesController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> disputes = disputesService.GetProposals(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(disputes.Items, page, pageSize, disputes.TotalItems);
	}

	[HttpGet("{disputeId}")]
	public ProposalDetailsModel Get(string siteId, string disputeId)
	{
		logger.LogInformation($"GET {nameof(ModeratorDisputesController)}.{nameof(ModeratorDisputesController.Get)} method called with {{SiteId}}, {{DisputeId}}", siteId, disputeId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(disputeId, nameof(Proposal).ToLower());

		return disputesService.GetProposal(siteId, disputeId);
	}
}
