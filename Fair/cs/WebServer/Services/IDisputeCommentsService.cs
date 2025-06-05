using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IDisputeCommentsService
{
	TotalItemsResult<DisputeCommentModel> GetDisputeComments([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string disputeId,
		[NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken);
}
