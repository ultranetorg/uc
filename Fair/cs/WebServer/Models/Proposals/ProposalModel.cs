namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairUser by)
{
	public AutoId Id { get; } = proposal.Id;

	public bool IsStalled { get; set; } = false;

	public FairOperationClass Operation { get; } = proposal.OptionClass;

	public IEnumerable<IEnumerable<AutoId>> Yes { get; } = proposal.Options.Select(x => x.Yes);
	public IEnumerable<AutoId> Neither { get; } = proposal.Neither;
	public IEnumerable<AutoId> Any { get; } = proposal.Any;
	public IEnumerable<AutoId> Ban { get; } = proposal.Ban;
	public IEnumerable<AutoId> Banish { get; } = proposal.Banish;

	public Time CreationTime { get; } = proposal.CreationTime;

	public string Title { get; } = proposal.Title;
	public string Text { get; } = proposal.Text;

	public UserModel By { get; } = new UserModel(by);
	public bool MultipleOptions { get; } = proposal.Options.Length > 1;
}
