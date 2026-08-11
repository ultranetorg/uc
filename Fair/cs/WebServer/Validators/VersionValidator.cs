using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class VersionValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string publicationId, int version)
	{
		if(version < 0)
		{
			throw new InvalidPublicationVersionException(publicationId, version);
		}
	}
}
