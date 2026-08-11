using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class LimitValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(int? limit)
	{
		if(limit != null && !Pagination.AllowedLimits.Contains(limit.Value))
		{
			throw new InvalidPaginationParametersException();
		}
	}
}
