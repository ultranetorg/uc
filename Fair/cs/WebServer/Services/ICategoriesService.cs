using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface ICategoriesService
{
	public CategoryModel GetCategory([NotNull][NotEmpty] string categoryId);
}
