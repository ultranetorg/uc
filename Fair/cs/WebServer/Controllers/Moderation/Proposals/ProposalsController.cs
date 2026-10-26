using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals")]
public class ProposalsController
(
#if DEBUG
	ILogger<ProposalsController> logger,
#endif
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ProposalsController), nameof(ProposalsController.GetAll), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetProposals(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("{proposalId}")]
	public ProposalDetailsModel GetDetails(string storeId, string proposalId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ProposalId}", nameof(ProposalsController), nameof(ProposalsController.GetDetails), storeId, proposalId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(proposalId, nameof(Proposal).ToLower());

		return proposalsService.GetProposalDetails(storeId, proposalId);
	}
}
