using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/discussions")]
public class ModeratorDiscussionsController
(
	ILogger<ModeratorDiscussionsController> logger,
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{StoreId}}, {{Pagination}}, {{Search}}", storeId, pagination, search);

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetDiscussions(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("{discussionId}")]
	public ProposalDetailsModel Get(string storeId, string discussionId)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{StoreId}}, {{DiscussionId}}", storeId, discussionId);

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());

		return proposalsService.GetDiscussion(storeId, discussionId);
	}
}
