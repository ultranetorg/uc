using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAuthorsService
{
	SiteAuthorModel GetAuthor([NotNull][NotEmpty] string authorId);
}
