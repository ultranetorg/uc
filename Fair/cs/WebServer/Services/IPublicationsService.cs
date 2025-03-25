using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IPublicationsService
{
	PublicationModel GetPublication([NotEmpty] string publicationId);

	TotalItemsResult<ModeratorPublicationModel> GetModeratorPublicationsNonOptimized(
		[NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken canellationToken);

	ModeratorPublicationModel GetModeratorPublication(string publicationId);

	TotalItemsResult<SitePublicationModel> SearchPublicationsNonOptimized([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name, CancellationToken cancellationToken);
}
