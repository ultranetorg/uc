namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairAccount account)
{
	public string Id { get; set; } = proposal.Id.ToString();

	public string Title { get; set; } = proposal.Title;
	public string Text { get; set; } = proposal.Text;

	public AccountBaseModel ByAccount { get; set; } = new(account);

	public int CreationTime { get; set; } = proposal.CreationTime.Days;
	public int ExpirationTime { get; set; } = proposal.CreationTime.Days + 30; // TODO: fix.

	public IEnumerable<int> OptionsVotesCount { get; set; } = proposal.Options.Select(x => x.Yes.Count());
	public int NeitherCount { get; set; } = proposal.Neither.Count();
	public int AbsCount { get; set; } = proposal.Abs.Count();
	public int BanCount { get; set; } = proposal.Ban.Count();
	public int BanishCount { get; set; } = proposal.Banish.Count();

	public int CommentsCount { get; set; } = proposal.Comments.Count();
}
