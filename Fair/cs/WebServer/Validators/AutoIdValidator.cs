using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class AutoIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string autoId, string entityName)
	{
		bool isParsed = AutoId.TryParse(autoId, out _);
		if (!isParsed)
		{
			throw new InvalidAutoIdException(entityName, autoId);
		}
	}
}
