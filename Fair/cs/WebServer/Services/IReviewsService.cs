using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IReviewsService
{
	TotalItemsResult<ReviewModel> GetPublicationReviewsNotOptimized([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);
}
