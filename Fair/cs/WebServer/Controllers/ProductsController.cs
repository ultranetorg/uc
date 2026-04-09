using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	IAutoIdValidator autoIdValidator,
	ProductsService productsService
) : BaseController
{
	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string productId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {ProductId}", nameof(ProductsController), nameof(GetDetails), productId);

		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return productsService.GetDetails(productId);
	}
}