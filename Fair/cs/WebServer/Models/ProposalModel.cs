namespace Uccs.Fair;

public class ProposalModel
{
	public FairOperationClass Change { get; set; }

	public object First { get; set; }
	public object Second { get; set; }

	public string Text { get; set; }

	public ProposalModel(FairOperation proposal)
	{
// 		Change = proposal.Change;
// 		First = proposal.First;
// 		Second = proposal.Second;
// 		Text = proposal.Text;
	}
}
