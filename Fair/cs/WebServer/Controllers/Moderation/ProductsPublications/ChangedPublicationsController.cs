using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/publications/changed")]
public class ChangedPublicationsController
(
#if DEBUG
	ILogger<ChangedPublicationsController> logger,
#endif
	PublicationsService publicationsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ChangedPublicationModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(ChangedPublicationsController), nameof(ChangedPublicationsController.GetAll), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ChangedPublicationModel> products = publicationsService.GetChangedPublications(storeId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}

	[HttpGet("{changedPublicationId}")]
	public ChangedPublicationDetailsModel GetDetails(string storeId, string changedPublicationId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ChangedPublicationId}", nameof(ChangedPublicationsController), nameof(ChangedPublicationsController.GetDetails), storeId, changedPublicationId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(changedPublicationId, nameof(EntityNames.ChangedPublicationEntityName).ToLower());

		return publicationsService.GetChangedPublicationDetails(storeId, changedPublicationId);
	}
}