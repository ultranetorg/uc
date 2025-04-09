using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IDisputesService
{

	public DisputeDetailsModel GetDispute([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string disputeId);

	public TotalItemsResult<DisputeModel> GetDisputes(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);

	public DisputeDetailsModel GetReferendum([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string disputeId);

	public TotalItemsResult<DisputeModel> GetReferendums(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);
}
