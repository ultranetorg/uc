using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	IAutoIdValidator autoIdValidator,
	ProductsService productsService
) : BaseController
{
	[HttpGet("{productId}/fields")]
	public IActionResult GetFields(string productId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {ProductId}", nameof(ProductsController), nameof(GetFields), productId);

		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return Ok(productsService.GetFields(productId));
	}
}