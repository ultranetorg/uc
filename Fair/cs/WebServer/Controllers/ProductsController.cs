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
	public IActionResult Get(string productId)
	{
		logger.LogInformation($"GET {nameof(ProductsController)}.{nameof(Get)} method called with {{ProductId}}", productId);

		autoIdValidator.Validate(productId, nameof(Product).ToLower());
		return Ok(productsService.GetFields(productId));
	}
}