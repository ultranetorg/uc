using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface ISitesService
{
	TotalItemsResult<SiteBaseModel> SearchNonOptimized([NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize, string name);

	TotalItemsResult<AuthorBaseModel> GetAuthors([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize);

	TotalItemsResult<CategoryParentBaseModel> GetCategories([NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue, NonZeroValue] int pageSize);

	SiteModel Find([NotEmpty] string siteId);

	SiteAuthorModel FindAuthorNonOptimized([NotEmpty] string siteId, [NotEmpty] string authorId);
}
