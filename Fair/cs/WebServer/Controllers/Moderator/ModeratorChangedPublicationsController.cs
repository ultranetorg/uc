using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/publications/changed")]
public class ModeratorChangedPublicationsController
(
	ILogger<ModeratorChangedPublicationsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	PublicationsService publicationsService
) : BaseController
{
	[HttpGet("{changedPublicationId}")]
	public ChangedPublicationDetailsModel GetDetails(string storeId, string changedPublicationId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ChangedPublicationId}", nameof(ModeratorChangedPublicationsController), nameof(GetDetails), storeId, changedPublicationId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(changedPublicationId, nameof(EntityNames.ChangedPublicationEntityName).ToLower());

		return publicationsService.GetChangedPublicationDetails(storeId, changedPublicationId);
	}

	[HttpGet]
	public IEnumerable<ChangedPublicationModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(ModeratorChangedPublicationsController), nameof(GetAll), storeId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ChangedPublicationModel> products = publicationsService.GetChangedPublicationsAll(storeId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}