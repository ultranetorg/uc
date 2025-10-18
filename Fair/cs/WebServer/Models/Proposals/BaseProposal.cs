namespace Uccs.Fair;

public abstract class BaseProposal(Proposal proposal)
{
	public string Id { get; } = proposal.Id.ToString();

	public string Title { get; } = proposal.Title;
	public string Text { get; } = proposal.Text;

	public int CreationTime { get; } = proposal.CreationTime.Days;

	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;

	public IEnumerable<int> OptionVotesCount { get; set; } = proposal.Options.Select(x => x.Yes.Count());
	public int NeitherCount { get; set; } = proposal.Neither.Count();
	public int AbstainedCount { get; set; } = proposal.Any.Count();
	public int BanCount { get; set; } = proposal.Ban.Count();
	public int BanishCount { get; set; } = proposal.Banish.Count();
}
