using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Uccs.Smp;

public interface IEntityIdValidator
{
	void Validate(string entityId, [NotEmpty] string entityName);
}
