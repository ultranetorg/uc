using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISitesService
{
	SiteModel Find([NotEmpty] string siteId);

	SiteAuthorModel FindAuthorNonOptimized([NotEmpty] string siteId, [NotEmpty] string authorId);

	TotalItemsResult<SitePublicationModel> SearchPublicationsNonOptimized([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name);
}
