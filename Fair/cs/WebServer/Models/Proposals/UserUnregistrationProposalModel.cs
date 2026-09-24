namespace Uccs.Fair;

public class UserUnregistrationProposalModel(Proposal proposal, FairUser by) : ProposalModel(proposal, by)
{
	public AutoId UserId { get; init; }
	public string UserName { get; init; }
}
