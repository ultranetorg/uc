using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Smp;

public interface ISitesService
{
	public SiteModel Find([NotEmpty] string siteId);

	TotalItemsResult<SitePublicationModel> SearchPublications([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name);
}
