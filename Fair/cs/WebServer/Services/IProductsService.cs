using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IProductsService
{
	ProductModel GetProduct([NotEmpty] string productId);

	TotalItemsResult<ProductModel> GetProducts(string name, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize);
}
