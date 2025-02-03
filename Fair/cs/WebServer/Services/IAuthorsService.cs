using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAuthorsService
{
	public AuthorModel Find([NotEmpty] string authorId);
}
