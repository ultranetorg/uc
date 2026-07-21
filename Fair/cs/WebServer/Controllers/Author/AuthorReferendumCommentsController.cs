using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/author/stores/{storeId}/referendums/{referendumId}/comments")]
public class AuthorReferendumCommentsController
(
	ILogger<AuthorReferendumCommentsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ProposalCommentsService proposalCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalCommentModel> GetDiscussionComments(string storeId, string referendumId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ReferendumId}, {Pagination}", nameof(AuthorReferendumCommentsController), nameof(AuthorReferendumCommentsController.GetDiscussionComments), storeId, referendumId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(referendumId, nameof(Proposal).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalCommentModel> reviews = proposalCommentsService.GetProposalComments(storeId, referendumId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
