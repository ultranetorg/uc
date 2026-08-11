using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class DepthValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(int? depth)
	{
		if (depth.HasValue && (depth < 0 || depth > 16))
		{
			throw new InvalidDepthException(depth.Value);
		}
	}
}
