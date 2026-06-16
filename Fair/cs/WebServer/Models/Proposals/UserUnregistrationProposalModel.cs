namespace Uccs.Fair;

public class UserUnregistrationProposalModel(Proposal proposal, FairUser by) : ProposalModel(proposal, by)
{
	public string UserId { get; init; }
	public string UserName { get; init; }
}
