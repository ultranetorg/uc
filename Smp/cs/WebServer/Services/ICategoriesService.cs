using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Smp;

public interface ICategoriesService
{
	public CategoryModel Find([NotEmpty] string categoryId);
}
