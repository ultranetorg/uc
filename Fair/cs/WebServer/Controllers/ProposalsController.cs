using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/[controller]")]
public class ProposalsController
(
	ILogger<ProposalsController> logger,
	ModeratorProposalsService proposalsService,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator
) : BaseController
{
	[HttpGet("moderators")]
	public IEnumerable<ModeratorProposalModel> GetModeratorProposals(string siteId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ProposalsController)}.{nameof(GetModeratorProposals)} method called with {{SiteId}}, {{Search}}, {{Pagination}}", siteId, search, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ModeratorProposalModel> discussions = proposalsService.GetModeratorProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("publishers")]
	public IEnumerable<PublisherProposalModel> GetPublisherProposals(string siteId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ProposalsController)}.{nameof(PublisherProposalModel)} method called with {{SiteId}}, {{Search}}, {{Pagination}}", siteId, search, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherProposalModel> discussions = proposalsService.GetPublisherProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}
}
