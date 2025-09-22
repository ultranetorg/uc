using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class DepthUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int? GetDepth(int? depth)
	{
		if (depth == null)
		{
			return null;
		}

		return depth == 0 ? Depth.DefaultDepth : depth;
	}
}
