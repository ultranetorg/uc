namespace Uccs.Fair;

public class BaseOptionsProposalModel(Proposal proposal, FairUser by) : BaseProposalModel(proposal, by)
{
	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;
}
