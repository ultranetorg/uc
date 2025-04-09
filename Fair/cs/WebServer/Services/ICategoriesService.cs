using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ICategoriesService
{
	public CategoryModel GetCategory([NotNull][NotEmpty] string categoryId);

	TotalItemsResult<CategoryParentBaseModel> GetCategories([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize);
}
