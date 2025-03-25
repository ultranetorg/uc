using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IDisputesService
{
	public DisputeDetailsModel GetDispute([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string disputeId, bool disputesOrReferendums);

	public TotalItemsResult<DisputeModel> GetDisputes(
		[NotEmpty][NotNull] string siteId, bool disputesOrReferendums, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);
}
