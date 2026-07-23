using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/publications/unpublished")]
public class UnpublishedPublicationsController
(
	ILogger<UnpublishedPublicationsController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	UnpublishedPublicationsService unpublishedPublicationsService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string storeId, string publicationId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {PublicationId}", nameof(UnpublishedPublicationsController), nameof(GetDetails), storeId, publicationId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return unpublishedPublicationsService.GetDetails(storeId, publicationId);
	}

	[HttpGet]
	public IEnumerable<UnpublishedPublicationModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(UnpublishedPublicationsController), nameof(GetAll), storeId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UnpublishedPublicationModel> products = unpublishedPublicationsService.GetAll(storeId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}