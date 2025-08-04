namespace Uccs.Fair;

public abstract class BaseProposal(Proposal proposal)
{
	public string Id { get; } = proposal.Id.ToString();

	public int CreationTime { get; } = proposal.CreationTime.Days;
}
