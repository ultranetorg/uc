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
	public ChangedPublicationDetailsModel Get(string siteId, string changedPublicationId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {ChangedPublicationId}", nameof(ModeratorChangedPublicationsController), nameof(Get), siteId, changedPublicationId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(changedPublicationId, nameof(EntityNames.ChangedPublicationEntityName).ToLower());

		return publicationsService.GetChangedPublication(siteId, changedPublicationId);
	}

	[HttpGet]
	public IEnumerable<ChangedPublicationModel> GetChangedPublications(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {Pagination}", nameof(ModeratorChangedPublicationsController), nameof(GetChangedPublications), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ChangedPublicationModel> products = publicationsService.GetChangedPublications(siteId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}