using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/reviews")]
public class ModeratorReviewsController
(
	ILogger<ModeratorReviewsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ReviewProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ReviewProposalModel> reviews = moderatorProposalsService.GetReviewProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}

	//[HttpGet("~/api/moderator/sites/{siteId}/reviews/{proposalId}")]
	//public ReviewProposalModel Get(string siteId, string proposalId)
	//{
	//	logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

	//	autoIdValidator.Validate(siteId, nameof(Site).ToLower());
	//	autoIdValidator.Validate(proposalId, nameof(Review).ToLower());

	//	return moderatorProposalsService.GetReviewProposal(siteId, proposalId);
	//}
}
