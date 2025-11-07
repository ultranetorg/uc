using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISearchService
{
	TotalItemsResult<PublicationExtendedModel> SearchPublications([NotNull][NotEmpty] string siteId, string query, int page, int pageSize,
		CancellationToken cancellationToken);

	IEnumerable<PublicationBaseModel> SearchLitePublications([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string query,
		[NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);

	TotalItemsResult<SiteBaseModel> SearchSites(string query, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		CancellationToken cancellationToken);

	IEnumerable<SiteSearchLiteModel> SearchLiteSites([NotNull][NotEmpty] string query, [NonNegativeValue] int page,
		[NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);

	IEnumerable<AccountBaseModel> SearchAccount([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken);
	IEnumerable<AccountSearchLiteModel> SearchLiteAccounts([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken);
}
