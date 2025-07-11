using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/disputes/{disputeId}/comments")]
public class ModeratorDisputeCommentsController
(
	ILogger<ModeratorDisputeCommentsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	IDisputeCommentsService disputeCommentsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<DisputeCommentModel> GetDisputeComments(string siteId, string disputeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorDisputeCommentsController)}.{nameof(ModeratorDisputeCommentsController.GetDisputeComments)} method called with {{DisputeId}}, {{Pagination}}", disputeId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(disputeId, nameof(Proposal).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<DisputeCommentModel> reviews = disputeCommentsService.GetDisputeComments(siteId, disputeId, page, pageSize, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}
}
