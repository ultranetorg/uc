using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/store")]
public class StoreProposalsController
(
#if DEBUG
	ILogger<StoreProposalsController> logger,
#endif
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet("renamings")]
	public IEnumerable<ProposalModel> GetRenamings(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(StoreProposalsController), nameof(StoreProposalsController.GetRenamings), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetStoreRenamingProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("info-updations")]
	public IEnumerable<ProposalModel> GetInfoUpdations(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(StoreProposalsController), nameof(StoreProposalsController.GetInfoUpdations), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetStoreInfoUpdationProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("avatar-changes")]
	public IEnumerable<ProposalModel> GetAvatarChanges(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(StoreProposalsController), nameof(StoreProposalsController.GetAvatarChanges), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetStoreAvatarChangeProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	IEnumerable<ProposalModel> GetPaged(string storeId, PaginationRequest pagination, Func<int, int, TotalItemsResult<ProposalModel>> load)
	{
		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> result = load(page, pageSize);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}
