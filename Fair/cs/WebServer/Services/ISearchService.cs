using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface ISearchService
{
	IEnumerable<PublicationExtendedModel> SearchPublications([NotNull][NotEmpty] string siteId, string query, int page, int pageSize,
		CancellationToken cancellationToken);

	IEnumerable<PublicationBaseModel> SearchLitePublications([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string query,
		[NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);

	IEnumerable<SiteBaseModel> SearchSites(string query, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		CancellationToken cancellationToken);

	IEnumerable<SiteSearchLightModel> SearchLiteSites([NotNull][NotEmpty] string query, [NonNegativeValue] int page,
		[NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);
}
