using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/discussions/{discussionId}/comments")]
public class ModeratorDiscussionCommentsController
(
	ILogger<ModeratorDiscussionCommentsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ProposalCommentsService proposalCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalCommentModel> GetDiscussionComments(string siteId, string discussionId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionCommentsController)}.{nameof(ModeratorDiscussionCommentsController.GetDiscussionComments)} method called with {{DiscussionId}}, {{Pagination}}", discussionId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalCommentModel> reviews = proposalCommentsService.GetProposalComments(siteId, discussionId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
