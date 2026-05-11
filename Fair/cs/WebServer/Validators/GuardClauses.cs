using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public static class GuardClauses
{
	public static int? DepthValid(this IGuardClause guardClause, int? input, [CallerArgumentExpression("input")] string? parameterName = null)
	{
		if(input is null || input >= 1 && input <= 16)
		{
			return input;
		}

		throw new ArgumentOutOfRangeException(parameterName, "Depth must be null or a number between 1 and 16.");
	}
}
