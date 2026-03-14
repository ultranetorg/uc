namespace Uccs.Fair;

public abstract class BaseProposalModel(Proposal proposal, FairUser by)
{
	public string Id { get; } = proposal.Id.ToString();

	public FairOperationClass Operation { get; set; } = proposal.OptionClass;

	public IEnumerable<int> OptionsVotesCount { get; set; } = proposal.Options.Select(x => x.Yes.Count());
	public int NeitherCount { get; set; } = proposal.Neither.Count();
	public int AnyCount { get; set; } = proposal.Any.Count();
	public int BanCount { get; set; } = proposal.Ban.Count();
	public int BanishCount { get; set; } = proposal.Banish.Count();

	public int CreationTime { get; } = proposal.CreationTime.Hours;

	public string Title { get; } = proposal.Title;
	public string Text { get; } = proposal.Text;

	public AccountBaseModel By { get; } = new AccountBaseModel(by);
}