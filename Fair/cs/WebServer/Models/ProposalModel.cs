namespace Uccs.Fair;

public class ProposalModel
{
	public SiteChange Change { get; set; }

	public object Value { get; set; }

	public ProposalModel(Proposal proposal)
	{
		Change = proposal.Change;
		Value = proposal.Value;
	}
}
