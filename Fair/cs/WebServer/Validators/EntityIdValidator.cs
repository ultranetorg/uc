using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class EntityIdValidator : IEntityIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string entityId, string entityName)
	{
		bool isParsed = EntityId.TryParse(entityId, out _);
		if (!isParsed)
		{
			throw new InvalidEntityIdException(entityName, entityId);
		}
	}
}
