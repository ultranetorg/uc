namespace Uccs.Fair;

public class ProposalCommentModel
{
	public string Id { get; set; }

	public string DisputeId { get; set; }

	public string CreatorId { get; set; }
	public string CreatorAddress { get; set; }
	public string CreatorNickname { get; set; }

	public string Text { get; set; }

	public int Created { get; set; }

	public ProposalCommentModel(ProposalComment dispute, FairAccount account)
	{
		Id = dispute.Id.ToString();
		DisputeId = dispute.Proposal.ToString();
		CreatorId = dispute.Creator.ToString();
		CreatorAddress = account.Address.ToString();
		CreatorNickname = account.Nickname;
		Text = dispute.Text;
		Created = dispute.Created.Days;
	}
}
