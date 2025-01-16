using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	IProductsService productsService,
	IEntityIdValidator entityIdValidator
) : BaseController
{
	[HttpGet("{id}")]
	public ActionResult<ProductEntry> Get(string id, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ProductsController)}.{nameof(Get)} method called with {{Id}}", id);

		entityIdValidator.Validate(id);

		ProductEntry product = productsService.GetProduct(id);
		If.Value(product).IsNull().Throw(() => new ProductNotFoundException(id));

		return product;
	}

	[HttpGet]
	public ActionResult<IEnumerable<ProductEntry>> Index([FromQuery] string name, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(ProductsController)}.{nameof(Index)} method called with {{Name}}, {{Pagination}}", name, pagination);

		return null;
	}
}
