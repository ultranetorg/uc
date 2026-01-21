using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/author/sites/{siteId}/referendums")]
public class AuthorReferendumsController
(
	ILogger<AuthorReferendumsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> referendums = proposalsService.GetReferendums(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}

	[HttpGet("{referendumId}")]
	public ProposalDetailsModel Get(string siteId, string referendumId)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.Get)} method called with {{SiteId}}, {{ReferendumId}}", siteId, referendumId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(referendumId, nameof(Proposal).ToLower());

		return proposalsService.GetReferendum(siteId, referendumId);
	}
}
