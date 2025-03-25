using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IReviewsService
{
	TotalItemsResult<ReviewModel> GetModeratorsReviewsNonOptimized(
		[NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken);

	ReviewDetailsModel GetModeratorReview([NotNull][NotEmpty] string reviewId);
}
