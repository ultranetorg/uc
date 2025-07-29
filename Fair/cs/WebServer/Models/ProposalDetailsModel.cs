namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal proposal, FairAccount account) : ProposalModel(proposal, account)
{
	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;
}
