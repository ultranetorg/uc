using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/stores/{storeId}/discussions")]
public class ModeratorDiscussionsController
(
#if DEBUG
	ILogger<ModeratorDiscussionsController> logger,
#endif
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(ModeratorDiscussionsController), nameof(ModeratorDiscussionsController.Get), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetDiscussions(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("{discussionId}")]
	public ProposalDetailsModel Get(string storeId, string discussionId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {DiscussionId}", nameof(ModeratorDiscussionsController), nameof(ModeratorDiscussionsController.Get), storeId, discussionId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(discussionId, nameof(Proposal).ToLower());

		return proposalsService.GetDiscussion(storeId, discussionId);
	}
}
