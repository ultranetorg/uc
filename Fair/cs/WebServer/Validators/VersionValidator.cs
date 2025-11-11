using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class VersionValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string publicationId, int version)
	{
		if(version < 0)
		{
			throw new InvalidPublicationVersionException(publicationId, version);
		}
	}
}
