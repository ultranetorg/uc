namespace Uccs.Fair;

public class ProposalModel(Proposal proposal, FairAccount account)
{
	public string Id { get; set; } = proposal.Id.ToString();

	public string ById { get; set; } = account.Id.ToString();
	public string ByNickname { get; set; } = account.Nickname;
	public string ByAddress { get; set; } = account.Address.ToString();
	public byte[]? ByAvatar { get; set; } = account.Avatar;

	/// TODO
	///public int YesCount { get; set; } = proposal.Yes.Count();
	///public int NoCount { get; set; } = proposal.No.Count();
	public int AbsCount { get; set; } = proposal.Abs.Count();

	public int Expiration { get; set; } = proposal.CreationTime.Days;

	public string Text { get; set; } = proposal.Text;

	public BaseVotableOperationModel Option { get; set; }

	public int CommentsCount { get; set; } = proposal.Comments.Count();
}
