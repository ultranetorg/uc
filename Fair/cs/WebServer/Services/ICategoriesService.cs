using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ICategoriesService
{
	public CategoryModel Find([NotEmpty] string categoryId);
}
