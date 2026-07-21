using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/discussions")]
public class ModeratorDiscussionsController
(
	ILogger<ModeratorDiscussionsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{StoreId}}, {{Pagination}}, {{Search}}", storeId, pagination, search);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetDiscussions(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("{discussionId}")]
	public ProposalDetailsModel Get(string storeId, string discussionId)
	{
		logger.LogInformation($"GET {nameof(ModeratorDiscussionsController)}.{nameof(ModeratorDiscussionsController.Get)} method called with {{StoreId}}, {{DiscussionId}}", storeId, discussionId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());

		return proposalsService.GetDiscussion(storeId, discussionId);
	}
}
