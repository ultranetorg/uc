using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/products/unpublished")]
public class UnpublishedStoreProductsController
(
	ILogger<UnpublishedStoreProductsController> logger,
	AutoIdValidator autoIdValidator,
	UnpublishedStoreProductsService unpublishedStoreProductsService
) : BaseController
{
	/// <summary>
	/// Returns Products that have not been published on the Store.
	/// </summary>
	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string storeId, string productId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ProductId}", nameof(UnpublishedStoreProductsController), nameof(GetDetails), storeId, productId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return unpublishedStoreProductsService.GetDetails(storeId, productId);
	}
}