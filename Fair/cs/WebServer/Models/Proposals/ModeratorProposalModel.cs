namespace Uccs.Fair;

public class ModeratorProposalModel(Proposal proposal, FairUser by) : ProposalModel(proposal, by)
{
	public IEnumerable<AccountBaseModel>? Moderators { get; init; }
}
