using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISitesService
{
	IEnumerable<SiteBaseModel> GetDefaultSites(CancellationToken cancellationToken);

	SiteModel GetSite([NotEmpty] string siteId);
}
