namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal proposal, CreatorModel creator, int votesRequiredToWin) : ProposalModel(proposal, creator)
{
	public IEnumerable<ProposalOptionModel> Options { get; init; } = null!;

	public int VotesRequiredToWin { get; } = votesRequiredToWin;
}
