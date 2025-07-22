using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public interface IProposalService
{

	public ProposalDetailsModel GetDiscussion([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string proposalId);

	public TotalItemsResult<ProposalModel> GetDiscussions(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);

	public ProposalDetailsModel GetReferendum([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string proposalId);

	public TotalItemsResult<ProposalModel> GetReferendums(
		[NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize,
		string? search, CancellationToken cancellationToken);
}