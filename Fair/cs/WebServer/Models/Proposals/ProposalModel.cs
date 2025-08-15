namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairAccount account) : BaseProposal(proposal)
{
	public AccountBaseModel ByAccount { get; set; } = new(account);

	public int CommentsCount { get; set; } = proposal.Comments.Count();
}
