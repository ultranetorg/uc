using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class ProductsController(
	ILogger<ProductsController> logger,
	IAutoIdValidator autoIdValidator,
	ProductsService productsService) : BaseController
{
	[HttpGet("{id}/fields")]
	public IActionResult Get(string id)
	{
		logger.LogInformation(
			$"GET {nameof(ProductsController)}.{nameof(Get)} method called with {{id}}", id);

		autoIdValidator.Validate(id, nameof(Product).ToLower());
		return Ok(productsService.GetFields(id));
	}
}