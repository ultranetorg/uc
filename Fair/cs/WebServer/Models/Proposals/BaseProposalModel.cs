namespace Uccs.Fair;

public abstract class BaseProposalModel(Proposal proposal, FairUser by)
{
	public string Id { get; } = proposal.Id.ToString();

	public FairOperationClass Operation { get; } = proposal.OptionClass;

	public IEnumerable<int> OptionsVotesCount { get; } = proposal.Options.Select(x => x.Yes.Count());
	public int NeitherCount { get; } = proposal.Neither.Count();
	public int AnyCount { get; } = proposal.Any.Count();
	public int BanCount { get; } = proposal.Ban.Count();
	public int BanishCount { get; } = proposal.Banish.Count();

	public int CreationTime { get; } = proposal.CreationTime.Hours;

	public string Title { get; } = proposal.Title;
	public string Text { get; } = proposal.Text;

	public AccountBaseModel By { get; } = new AccountBaseModel(by);
	public bool MultipleOptions { get; } = proposal.Options.Length > 1;

	public int HoursLeft { get; set; } = -1;
}
