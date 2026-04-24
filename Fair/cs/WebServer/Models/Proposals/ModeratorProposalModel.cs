namespace Uccs.Fair;

public class ModeratorProposalModel(Proposal proposal, FairUser by) : BaseProposalModel(proposal, by)
{
	public IEnumerable<AccountBaseModel>? Moderators { get; init; }
}
