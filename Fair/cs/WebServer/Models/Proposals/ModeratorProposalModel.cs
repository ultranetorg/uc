namespace Uccs.Fair;

public class ModeratorProposalModel(Proposal proposal, FairUser by) : ProposalModel(proposal, by)
{
	public IEnumerable<UserModel> Moderators { get; init; }
}
