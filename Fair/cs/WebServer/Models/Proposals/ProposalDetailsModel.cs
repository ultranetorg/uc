namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal proposal, FairUser account, int votesRequiredToWin) : ProposalModel(proposal, account)
{
	public IEnumerable<ProposalOptionModel> Options { get; init; } = null!;

	public int VotesRequiredToWin { get; } = votesRequiredToWin;
}
