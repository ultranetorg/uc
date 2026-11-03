using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/proposals/publications")]
public class PublicationsProposalsController
(
#if DEBUG
	ILogger<PublicationsProposalsController> logger,
#endif
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet("creations")]
	public IEnumerable<PublicationProposalModel> GetCreations(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetCreations), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetPublicationCreationProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("deletions")]
	public IEnumerable<PublicationProposalModel> GetDeletions(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetDeletions), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetPublicationDeletionProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("publishes")]
	public IEnumerable<PublicationProposalModel> GetPublishes(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetPublishes), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetPublicationPublishProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("unpublishes")]
	public IEnumerable<PublicationProposalModel> GetUnpublishes(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetUnpublishes), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetPublicationUnpublishProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	[HttpGet("updations")]
	public IEnumerable<PublicationProposalModel> GetUpdations(string storeId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}, {Search}", nameof(PublicationsProposalsController), nameof(PublicationsProposalsController.GetUpdations), storeId, pagination, search);
#endif

		return GetPaged(storeId, pagination, (page, pageSize) => moderatorProposalService.GetPublicationUpdationProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken));
	}

	IEnumerable<PublicationProposalModel> GetPaged(string storeId, PaginationRequest pagination, Func<int, int, TotalItemsResult<PublicationProposalModel>> load)
	{
		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationProposalModel> result = load(page, pageSize);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}
