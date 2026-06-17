namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairUser by)
{
	public string Id { get; } = proposal.Id.ToString();

	public bool IsStalled { get; set; } = false;

	public FairOperationClass Operation { get; } = proposal.OptionClass;

	public IEnumerable<IEnumerable<string>> Yes { get; } = proposal.Options.Select(x => x.Yes.Select(y => y.ToString()));
	public IEnumerable<string> Neither { get; } = proposal.Neither.Select(x => x.ToString());
	public IEnumerable<string> Any { get; } = proposal.Any.Select(x => x.ToString());
	public IEnumerable<string> Ban { get; } = proposal.Ban.Select(x => x.ToString());
	public IEnumerable<string> Banish { get; } = proposal.Banish.Select(x => x.ToString());

	public int CreationTime { get; } = proposal.CreationTime.Hours;

	public string Title { get; } = proposal.Title;
	public string Text { get; } = proposal.Text;

	public UserModel By { get; } = new UserModel(by);
	public bool MultipleOptions { get; } = proposal.Options.Length > 1;
}
