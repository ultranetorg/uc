using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/publications/changed")]
public class ModeratorChangedPublicationsController
(
#if DEBUG
	ILogger<ModeratorChangedPublicationsController> logger,
#endif
	PublicationsService publicationsService
) : BaseController
{
	[HttpGet("{changedPublicationId}")]
	public ChangedPublicationDetailsModel GetDetails(string storeId, string changedPublicationId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ChangedPublicationId}", nameof(ModeratorChangedPublicationsController), nameof(ModeratorChangedPublicationsController.GetDetails), storeId, changedPublicationId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(changedPublicationId, nameof(EntityNames.ChangedPublicationEntityName).ToLower());

		return publicationsService.GetChangedPublicationDetails(storeId, changedPublicationId);
	}

	[HttpGet]
	public IEnumerable<ChangedPublicationModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(ModeratorChangedPublicationsController), nameof(ModeratorChangedPublicationsController.GetAll), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ChangedPublicationModel> products = publicationsService.GetChangedPublicationsAll(storeId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}