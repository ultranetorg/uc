namespace Uccs.Fair;

public class ModeratorProposalModel(Proposal proposal, CreatorModel creator) : ProposalModel(proposal, creator)
{
	public IEnumerable<UserModel> Moderators { get; init; }
}
