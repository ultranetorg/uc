using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/discussions/{discussionId}/comments")]
public class ModeratorDiscussionCommentsController
(
#if DEBUG
	ILogger<ModeratorDiscussionCommentsController> logger,
#endif
	ProposalCommentsService proposalCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalCommentModel> GetDiscussionComments(string storeId, string discussionId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {DiscussionId}, {Pagination}", nameof(ModeratorDiscussionCommentsController), nameof(ModeratorDiscussionCommentsController.GetDiscussionComments), storeId, discussionId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalCommentModel> reviews = proposalCommentsService.GetProposalComments(storeId, discussionId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
