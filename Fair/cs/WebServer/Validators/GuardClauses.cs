using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public static class GuardClauses
{
	public static int? DepthValid(this IGuardClause guardClause, int? input, [CallerArgumentExpression("input")] string? parameterName = null)
	{
		if(input is null || input is 0 or 1 or 2)
		{
			return input;
		}

		throw new ArgumentOutOfRangeException(parameterName, "Depth must be null, 0, 1, or 2.");
	}
}
