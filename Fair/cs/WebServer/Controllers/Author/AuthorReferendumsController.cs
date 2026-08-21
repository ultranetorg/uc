using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/author/stores/{storeId}/referendums")]
public class AuthorReferendumsController
(
#if DEBUG
	ILogger<AuthorReferendumsController> logger,
#endif
	ProposalService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ProposalModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(AuthorReferendumsController), nameof(AuthorReferendumsController.GetAll), storeId, pagination, search);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> referendums = proposalsService.GetReferendums(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}

	[HttpGet("{referendumId}")]
	public ProposalDetailsModel Get(string storeId, string referendumId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ReferendumId}", nameof(AuthorReferendumsController), nameof(AuthorReferendumsController.Get), storeId, referendumId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(referendumId, nameof(Proposal).ToLower());

		return proposalsService.GetReferendum(storeId, referendumId);
	}
}
