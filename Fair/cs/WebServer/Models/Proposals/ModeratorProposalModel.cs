namespace Uccs.Fair;

public class ModeratorProposalModel(Proposal proposal, FairUser by, IEnumerable<AccountBaseModel>? moderators) : BaseProposalModel(proposal, by)
{
	public IEnumerable<AccountBaseModel>? Moderators { get; } = moderators;
}
