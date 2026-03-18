namespace Uccs.Fair;

public class ProposalDetailsModel(Proposal proposal, FairUser account, int votesRequiredToWin) : BaseProposalOptionsModel(proposal, account)
{
	public int VotesRequiredToWin { get; } = votesRequiredToWin;

	public IEnumerable<string> Neither { get; } = proposal.Neither.Select(x => x.ToString());
	public IEnumerable<string> Any { get; } = proposal.Any.Select(x => x.ToString());
	public IEnumerable<string> Ban { get; } = proposal.Ban.Select(x => x.ToString());
	public IEnumerable<string> Banish { get; } = proposal.Banish.Select(x => x.ToString());
}
