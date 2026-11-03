using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/categories")]
public class CategoriesProposalsController
(
#if DEBUG
	ILogger<CategoriesProposalsController> logger,
#endif
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet("creations")]
	public IEnumerable<ProposalModel> GetCreations(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(CategoriesProposalsController), nameof(CategoriesProposalsController.GetCreations), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetCategoryCreationProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("deletions")]
	public IEnumerable<ProposalModel> GetDeletions(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(CategoriesProposalsController), nameof(CategoriesProposalsController.GetDeletions), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetCategoryDeletionProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("movements")]
	public IEnumerable<ProposalModel> GetMovements(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(CategoriesProposalsController), nameof(CategoriesProposalsController.GetMovements), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetCategoryMovementProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("type-changes")]
	public IEnumerable<ProposalModel> GetTypeChanges(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(CategoriesProposalsController), nameof(CategoriesProposalsController.GetTypeChanges), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetCategoryTypeChangeProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("avatar-changes")]
	public IEnumerable<ProposalModel> GetAvatarChanges(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(CategoriesProposalsController), nameof(CategoriesProposalsController.GetAvatarChanges), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetCategoryAvatarChangeProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
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
