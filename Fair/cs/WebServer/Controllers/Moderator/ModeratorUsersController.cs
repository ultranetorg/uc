using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/users")]
public class ModeratorUsersController
(
	ILogger<ModeratorUsersController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<UserProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorUsersController)}.{nameof(ModeratorUsersController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserProposalModel> reviews = moderatorProposalsService.GetUserProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}

	[HttpGet("~/api/moderator/sites/{siteId}/users/{proposalId}")]
	public UserProposalModel Get(string siteId, string proposalId)
	{
		logger.LogInformation($"GET {nameof(ModeratorReviewsController)}.{nameof(ModeratorReviewsController.Get)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(proposalId, nameof(User).ToLower());

		return moderatorProposalsService.GetUserProposal(siteId, proposalId);
	}
}
