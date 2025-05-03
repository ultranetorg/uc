using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class DepthUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetDepth(int depth)
	{
		return depth == 0 ? Depth.DefaultDepth : depth;
	}
}
