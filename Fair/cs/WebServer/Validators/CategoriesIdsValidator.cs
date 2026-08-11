using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class CategoriesIdsValidator
{
	const int MaxCategoriesCount = 6;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string[]? categoriesIds)
	{
		if(categoriesIds != null)
		{
			if((categoriesIds.Length > MaxCategoriesCount) || categoriesIds.Any(x => AutoId.TryParse(x, out _) == false))
			{
				throw new InvalidCategoryException();
			}
		}
	}
}
