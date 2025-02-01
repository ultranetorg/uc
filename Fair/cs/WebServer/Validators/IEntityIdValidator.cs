using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public interface IEntityIdValidator
{
	void Validate(string entityId, [NotEmpty] string entityName);
}
