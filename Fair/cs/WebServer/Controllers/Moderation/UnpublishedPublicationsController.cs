using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/publications/unpublished")]
public class UnpublishedPublicationsController
(
#if DEBUG
	ILogger<UnpublishedPublicationsController> logger,
#endif
	UnpublishedPublicationsService unpublishedPublicationsService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string storeId, string publicationId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {PublicationId}", nameof(UnpublishedPublicationsController), nameof(UnpublishedPublicationsController.GetDetails), storeId, publicationId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return unpublishedPublicationsService.GetDetails(storeId, publicationId);
	}

	[HttpGet]
	public IEnumerable<UnpublishedPublicationModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(UnpublishedPublicationsController), nameof(UnpublishedPublicationsController.GetAll), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UnpublishedPublicationModel> products = unpublishedPublicationsService.GetAll(storeId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}