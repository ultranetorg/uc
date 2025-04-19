using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ICategoriesService
{
	public CategoryModel GetCategory([NotNull][NotEmpty] string categoryId, CancellationToken cancellationToken);

	IEnumerable<CategoryParentBaseModel> GetCategories([NotEmpty] string siteId, [NonNegativeValue, NonZeroValue] int depth, CancellationToken cancellationToken);
}
