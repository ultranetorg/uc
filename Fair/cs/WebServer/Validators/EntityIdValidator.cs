using System.Runtime.CompilerServices;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class EntityIdValidator : IEntityIdValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string entityId)
	{
		bool isMatch = Regexes.EntityId.IsMatch(entityId);
		If.Value(isMatch).False().Throw(() => new InvalidProductIdException(entityId));
	}
}
