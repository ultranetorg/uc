using System.Runtime.CompilerServices;
using Uccs.Web.Utilities;

namespace Uccs.Smp;

public class EntityIdValidator : IEntityIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string entityId, string entityName)
	{
		bool isParsed = EntityId.TryParse(entityId, out _);
		If.Value(isParsed).False().Throw(() => new InvalidEntityIdException(entityName, entityId));
	}
}
