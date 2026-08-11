using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class SearchParamsValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string[]? categoriesIds, ProductType? productType)
	{
		if (categoriesIds != null && categoriesIds.Length > 0 && productType != null)
		{
			throw new InvalidSearchParamsException();
		}
	}
}
