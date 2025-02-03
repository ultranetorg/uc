using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface ICategoriesService
{
	public CategoryModel Find([NotEmpty] string categoryId);
}
