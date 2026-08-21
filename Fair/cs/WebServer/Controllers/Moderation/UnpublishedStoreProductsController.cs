using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/products/unpublished")]
public class UnpublishedStoreProductsController
(
#if DEBUG
	ILogger<UnpublishedStoreProductsController> logger,
#endif
	UnpublishedStoreProductsService unpublishedStoreProductsService
) : BaseController
{
	/// <summary>
	/// Returns Products that have not been published on the Store.
	/// </summary>
	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string storeId, string productId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {ProductId}", nameof(UnpublishedStoreProductsController), nameof(UnpublishedStoreProductsController.GetDetails), storeId, productId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(productId, nameof(Product).ToLower());

		return unpublishedStoreProductsService.GetDetails(storeId, productId);
	}
}