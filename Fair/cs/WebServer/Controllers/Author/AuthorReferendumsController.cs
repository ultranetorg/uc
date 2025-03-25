using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/author/sites/{siteId}/referendums")]
public class AuthorReferendumsController
(
	ILogger<AuthorReferendumsController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	IDisputesService disputesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<DisputeModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<DisputeModel> referendums = disputesService.GetDisputes(siteId, false, page, pageSize, search, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}

	[HttpGet("{referendumId}")]
	public DisputeModel Get(string siteId, string referendumId)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.Get)} method called with {{SiteId}}, {{ReferendumId}}", siteId, referendumId);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(referendumId, nameof(Dispute).ToLower());

		return disputesService.GetDispute(siteId, referendumId, false);
	}
}
