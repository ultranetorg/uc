using System.Runtime.CompilerServices;

namespace Ardalis.GuardClauses;

public static class GuardClauseExtensions
{
	public static string Empty(this IGuardClause guardClause, string input,
		[CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
	{
		if (input == string.Empty)
		{
			throw new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
		}

		return input;
	}

	public static T[] Empty<T>(this IGuardClause guardClause, T[] input,
		[CallerArgumentExpression("input")] string? parameterName = null, string? message = null)
	{
		if (input.Length == 0)
		{
			throw new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
		}

		return input;
	}
}
