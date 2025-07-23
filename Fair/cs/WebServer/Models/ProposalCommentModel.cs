namespace Uccs.Fair;

public class ProposalCommentModel
{
	public string Id { get; set; }

	public string ProposalId { get; set; }

	public string CreatorId { get; set; }
	public string CreatorAddress { get; set; }
	public string CreatorNickname { get; set; }

	public string Text { get; set; }

	public int Created { get; set; }

	public ProposalCommentModel(ProposalComment proposal, FairAccount account)
	{
		Id = proposal.Id.ToString();
		ProposalId = proposal.Proposal.ToString();
		CreatorId = proposal.Creator.ToString();
		CreatorAddress = account.Address.ToString();
		CreatorNickname = account.Nickname;
		Text = proposal.Text;
		Created = proposal.Created.Days;
	}
}
