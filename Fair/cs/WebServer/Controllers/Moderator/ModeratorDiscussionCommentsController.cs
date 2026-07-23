using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/discussions/{discussionId}/comments")]
public class ModeratorDiscussionCommentsController
(
	ILogger<ModeratorDiscussionCommentsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ProposalCommentsService proposalCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalCommentModel> GetDiscussionComments(string storeId, string discussionId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionCommentsController)}.{nameof(ModeratorDiscussionCommentsController.GetDiscussionComments)} method called with {{StoreId}}, {{DiscussionId}}, {{Pagination}}", storeId, discussionId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalCommentModel> reviews = proposalCommentsService.GetProposalComments(storeId, discussionId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
