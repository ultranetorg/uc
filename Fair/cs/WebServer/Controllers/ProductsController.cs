using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class ProductsController
(
	ILogger<ProductsController> logger,
	IProductsService productsService,
	IPaginationValidator paginationValidator,
	IEntityIdValidator entityIdValidator
) : BaseController
{
	[HttpGet("{id}")]
	public ProductEntry Get(string id)
	{
		logger.LogInformation($"GET {nameof(ProductsController)}.{nameof(Get)} method called with {{Id}}", id);

		entityIdValidator.Validate(id);

		ProductEntry product = productsService.GetProduct(id);
		If.Value(product).IsNull().Throw(() => new ProductNotFoundException(id));

		return product;
	}

	[HttpGet]
	public IEnumerable<ProductEntry> Index([FromQuery] string name, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(ProductsController)}.{nameof(Index)} method called with {{Name}}, {{Pagination}}", name, pagination);

		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<ProductEntry> products = productsService.GetProducts(name, page, pageSize);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
