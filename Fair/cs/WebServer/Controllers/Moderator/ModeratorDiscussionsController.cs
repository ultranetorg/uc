using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/discussions")]
public class ModeratorDiscussionsController
(
	ILogger<ModeratorDiscussionsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetDiscussions(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("{discussionId}")]
	public ProposalDetailsModel Get(string siteId, string discussionId)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{SiteId}}, {{DiscussionId}}", siteId, discussionId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());

		return proposalsService.GetDiscussion(siteId, discussionId);
	}
}
