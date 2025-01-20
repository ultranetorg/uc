using System.Runtime.CompilerServices;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class EntityIdValidator : IEntityIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string entityId)
	{
		bool isParsed = EntityId.TryParse(entityId, out _);
		If.Value(isParsed).False().Throw(() => new InvalidProductIdException(entityId));
	}
}
