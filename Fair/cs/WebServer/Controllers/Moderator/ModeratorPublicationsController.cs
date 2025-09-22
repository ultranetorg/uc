using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/publications")]
public class ModeratorPublicationsController
(
	ILogger<ModeratorPublicationsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PublicationProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorPublicationsController)}.{nameof(ModeratorPublicationsController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationProposalModel> result = moderatorProposalService.GetPublicationsProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}

	//[HttpGet("{publicationId}")]
	//public PublicationProposalModel Get(string siteId, string publicationId)
	//{
	//	logger.LogInformation($"GET {nameof(ModeratorPublicationsController)}.{nameof(ModeratorPublicationsController.Get)} method called with {{SiteId}}, {{PublicationId}}", siteId, publicationId);

	//	autoIdValidator.Validate(siteId, nameof(Site).ToLower());
	//	autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

	//	return moderatorProposalService.GetPublicationProposal(siteId, publicationId);
	//}
}
