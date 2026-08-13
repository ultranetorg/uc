using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class SearchQueryValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string searchQuery)
	{
		if (string.IsNullOrEmpty(searchQuery))
		{
			throw new InvalidSearchQueryException();
		}
	}
}
