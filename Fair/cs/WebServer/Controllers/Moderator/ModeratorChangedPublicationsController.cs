using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/publications/changed")]
public class ModeratorChangedPublicationsController
(
	ILogger<ModeratorChangedPublicationsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	PublicationsService publicationsService
) : BaseController
{
	[HttpGet("{changedPublicationId}")]
	public ChangedPublicationDetailsModel GetDetails(string siteId, string changedPublicationId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {ChangedPublicationId}", nameof(ModeratorChangedPublicationsController), nameof(GetDetails), siteId, changedPublicationId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		autoIdValidator.Validate(changedPublicationId, nameof(EntityNames.ChangedPublicationEntityName).ToLower());

		return publicationsService.GetChangedPublicationDetails(siteId, changedPublicationId);
	}

	[HttpGet]
	public IEnumerable<ChangedPublicationModel> GetAll(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {Pagination}", nameof(ModeratorChangedPublicationsController), nameof(GetAll), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ChangedPublicationModel> products = publicationsService.GetChangedPublicationsAll(siteId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}