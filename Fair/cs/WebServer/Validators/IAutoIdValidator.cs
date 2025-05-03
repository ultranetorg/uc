using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAutoIdValidator
{
	void Validate(string autoId, [NotEmpty] string entityName);
}
