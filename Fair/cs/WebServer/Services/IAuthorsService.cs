using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAuthorsService
{
	AuthorDetailsModel GetAuthor([NotNull][NotEmpty] string authorId);
}
