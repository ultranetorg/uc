namespace Uccs.Fair;

public class ProposalCommentModel(ProposalComment proposal, FairUser account)
{
	public AutoId Id { get; set; } = proposal.Id;

	public AutoId ProposalId { get; set; } = proposal.Proposal;

	public UserBaseAvatarModel CreatorUser { get; set; } = new(account);

	public string Text { get; set; } = proposal.Text;

	public Time Created { get; set; } = proposal.Created;
}
