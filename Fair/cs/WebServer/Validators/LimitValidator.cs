using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class LimitValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(int? limit)
	{
		if(limit.HasValue && !Pagination.AllowedLimits.Contains(limit.Value))
		{
			throw new InvalidPaginationParametersException();
		}
	}
}
