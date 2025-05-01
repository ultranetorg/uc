using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class DepthValidator : IDepthValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(int depth)
	{
		if (depth < 0 || depth > 2)
		{
			throw new InvalidDepthException(depth);
		}
	}
}
