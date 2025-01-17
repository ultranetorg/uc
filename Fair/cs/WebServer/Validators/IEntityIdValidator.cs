using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public interface IEntityIdValidator
{
	void Validate(string entityId);
}
