using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IPublicationsService
{
	PublicationDetailsModel GetPublication([NotEmpty] string publicationId);

	TotalItemsResult<PublicationAuthorModel> GetAuthorPublicationsNotOptimized([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);

	TotalItemsResult<PublicationModel> GetCategoryPublicationsNotOptimized([NotNull][NotEmpty] string categoryId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);

	TotalItemsResult<ModeratorPublicationModel> GetModeratorPublicationsNonOptimized(
		[NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken canellationToken);

	ModeratorPublicationModel GetModeratorPublication(string publicationId);

	TotalItemsResult<PublicationSearchModel> SearchPublicationsNotOptimized([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name, CancellationToken cancellationToken);
}
