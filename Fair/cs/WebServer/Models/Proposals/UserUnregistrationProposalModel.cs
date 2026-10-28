namespace Uccs.Fair;

public class UserUnregistrationProposalModel(Proposal proposal, CreatorModel creator) : ProposalModel(proposal, creator)
{
	public AutoId UserId { get; init; }
	public string UserName { get; init; }
}
