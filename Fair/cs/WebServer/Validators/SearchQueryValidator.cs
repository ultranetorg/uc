using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class SearchQueryValidator : ISearchQueryValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string searchQuery)
	{
		if (string.IsNullOrEmpty(searchQuery))
		{
			throw new InvalidSearchQueryException();
		}
	}
}
