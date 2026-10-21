using Uccs.Fair;

namespace Uccs.Fair;

public class ProposalCommentModel : BaseCommentModel
{
	public required AutoId ProposalId { get; init; }
}
