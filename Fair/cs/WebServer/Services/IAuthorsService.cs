using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAuthorsService
{
	AuthorDetailsModel GetDetails([NotNull][NotEmpty] string authorId);
}
