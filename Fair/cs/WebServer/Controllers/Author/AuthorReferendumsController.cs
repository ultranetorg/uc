using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/author/stores/{storeId}/referendums")]
public class AuthorReferendumsController
(
	ILogger<AuthorReferendumsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.GetAll)} method called with {{StoreId}}, {{Pagination}}, {{Search}}", storeId, pagination, search);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> referendums = proposalsService.GetReferendums(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}

	[HttpGet("{referendumId}")]
	public ProposalDetailsModel Get(string storeId, string referendumId)
	{
		logger.LogInformation($"GET {nameof(AuthorReferendumsController)}.{nameof(AuthorReferendumsController.Get)} method called with {{StoreId}}, {{ReferendumId}}", storeId, referendumId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(referendumId, nameof(Proposal).ToLower());

		return proposalsService.GetReferendum(storeId, referendumId);
	}
}
