namespace Uccs.Fair;

public class ProposalCommentModel(ProposalComment proposal, FairAccount account)
{
	public string Id { get; set; } = proposal.Id.ToString();

	public string ProposalId { get; set; } = proposal.Proposal.ToString();

	public AccountBaseAvatarModel CreatorAccount { get; set; } = new(account);

	public string Text { get; set; } = proposal.Text;

	public int Created { get; set; } = proposal.Created.Days;
}
