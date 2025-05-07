using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface ISiteSearchQueryValidator
{
	void Validate(string value);
}
