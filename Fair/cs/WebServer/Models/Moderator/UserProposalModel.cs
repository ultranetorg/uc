namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal)
{
	public string Id { get; } = proposal.Id.ToString();

	public int CreationTime { get; } = proposal.CreationTime.Days;

	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;
}
