namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairUser by) : BaseOptionsProposalModel(proposal, by)
{
	public AccountBaseAvatarModel ByAccount { get; set; } = new(by);

	public int CommentsCount { get; set; } = proposal.Comments.Count();
}
