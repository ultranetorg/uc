using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAuthorsService
{
	AuthorModel GetAuthor([NotNull][NotEmpty] string authorId);
}
