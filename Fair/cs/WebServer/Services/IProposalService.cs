using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IProposalService
{

	public ProposalDetailsModel GetProposal([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string disputeId);

	public TotalItemsResult<ProposalModel> GetProposals(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);

	public ProposalDetailsModel GetReferendum([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string disputeId);

	public TotalItemsResult<ProposalModel> GetReferendums(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);
}