namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal) : BaseProposal(proposal)
{
	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;
}
