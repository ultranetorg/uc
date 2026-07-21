using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/[controller]")]
public class ProposalsController
(
	ILogger<ProposalsController> logger,
	ModeratorProposalsService proposalsService,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator
) : BaseController
{
	[HttpGet("moderators")]
	public IEnumerable<ModeratorProposalModel> GetModeratorProposals(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ProposalsController)}.{nameof(GetModeratorProposals)} method called with {{StoreId}}, {{Search}}, {{Pagination}}", storeId, search, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ModeratorProposalModel> discussions = proposalsService.GetModeratorProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("publishers")]
	public IEnumerable<PublisherProposalModel> GetPublisherProposals(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ProposalsController)}.{nameof(PublisherProposalModel)} method called with {{StoreId}}, {{Search}}, {{Pagination}}", storeId, search, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherProposalModel> discussions = proposalsService.GetPublisherProposalsNotOptimized(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("user-registrations")]
	public IEnumerable<ProposalModel> GetUserRegistrations(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(ProposalsController), nameof(GetUserRegistrations), storeId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> discussions = proposalsService.GetUserRegistrations(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}

	[HttpGet("user-unregistrations")]
	public IEnumerable<UserUnregistrationProposalModel> GetUserUnregistrations(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(ProposalsController), nameof(GetUserUnregistrations), storeId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UserUnregistrationProposalModel> discussions = proposalsService.GetUserUnregistrations(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
	}
}
