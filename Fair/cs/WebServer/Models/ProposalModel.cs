namespace Uccs.Fair;

public class ProposalModel(Proposal dispute, FairAccount account)
{
	public string Id { get; set; } = dispute.Id.ToString();

	public string ById { get; set; } = account.Id.ToString();
	public string ByNickname { get; set; } = account.Nickname;
	public string ByAddress { get; set; } = account.Address.ToString();
	public byte[]? ByAvatar { get; set; } = account.Avatar;

	public int YesCount { get; set; } = dispute.Yes.Count();
	public int NoCount { get; set; } = dispute.No.Count();
	public int AbsCount { get; set; } = dispute.Abs.Count();

	public int Expiration { get; set; } = dispute.Expiration.Days;

	public string Text { get; set; } = dispute.Text;

	public BaseVotableOperationModel Option { get; set; }

	public int CommentsCount { get; set; } = dispute.Comments.Count();
}
