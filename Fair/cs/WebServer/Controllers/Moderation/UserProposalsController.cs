using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/user")]
public class UserProposalsController
(
#if DEBUG
	ILogger<UserProposalsController> logger,
#endif
	ModeratorProposalsService proposalsService
) : BaseController
{
	[HttpGet("registrations")]
	public IEnumerable<ProposalModel> GetUserRegistrations(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(UserProposalsController), nameof(UserProposalsController.GetUserRegistrations), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetUserRegistrations(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("unregistrations")]
	public IEnumerable<UserUnregistrationProposalModel> GetUserUnregistrations(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(UserProposalsController), nameof(UserProposalsController.GetUserUnregistrations), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserUnregistrationProposalModel> discussions = proposalsService.GetUserUnregistrations(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}
}
