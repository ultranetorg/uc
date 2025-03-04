namespace Uccs.Fair;

public class ProposalModel
{
	public ProposalChange Change { get; set; }

	public object First { get; set; }
	public object Second { get; set; }

	public string Text { get; set; }

	public ProposalModel(Proposal proposal)
	{
		Change = proposal.Change;
		First = proposal.First;
		Second = proposal.Second;
		Text = proposal.Text;
	}
}
