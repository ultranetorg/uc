using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/{proposalId}/comments")]
public class ProposalCommentsController
(
#if DEBUG
	ILogger<ProposalCommentsController> logger,
#endif
	ProposalCommentsService proposalCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalCommentModel> GetAll(string storeId, string proposalId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ProposalId}, {Pagination}", nameof(ProposalCommentsController), nameof(GetAll), storeId, proposalId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(proposalId, nameof(Proposal).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalCommentModel> reviews = proposalCommentsService.GetProposalComments(storeId, proposalId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
