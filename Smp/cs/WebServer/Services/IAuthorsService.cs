using System.Diagnostics.CodeAnalysis;

namespace Uccs.Smp;

public interface IAuthorsService
{
	public AuthorModel Find([NotEmpty] string authorId);
}
