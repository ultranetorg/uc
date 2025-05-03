using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class AutoIdValidator : IAutoIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string autoId, string entityName)
	{
		bool isParsed = AutoId.TryParse(autoId, out _);
		if (!isParsed)
		{
			throw new InvalidAutoIdException(entityName, autoId);
		}
	}
}
