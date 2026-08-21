using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class DepthValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(int? depth)
	{
		if (depth.HasValue && (depth < 0 || depth > Depth.MaxDepth))
		{
			throw new InvalidDepthException(depth.Value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ValidateResolved(int? depth, [CallerArgumentExpression(nameof(depth))] string parameterName = null)
	{
		if (depth.HasValue)
		{
			ArgumentOutOfRangeException.ThrowIfLessThan(depth.Value, Depth.MinDepth, parameterName);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(depth.Value, Depth.MaxDepth, parameterName);
		}
	}
}
