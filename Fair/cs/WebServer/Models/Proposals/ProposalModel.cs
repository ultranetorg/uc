namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairUser account) : BaseProposal(proposal)
{
	public AccountBaseAvatarModel ByAccount { get; set; } = new(account);

	public int CommentsCount { get; set; } = proposal.Comments.Count();
}
