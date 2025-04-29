using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IReviewsService
{
	TotalItemsResult<ModeratorReviewModel> GetModeratorsReviewsNotOptimized(
		[NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken);

	ModeratorReviewDetailsModel GetModeratorReview([NotNull][NotEmpty] string reviewId);

	TotalItemsResult<ReviewModel> GetPublicationReviewsNotOptimized([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);
}
