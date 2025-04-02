namespace Uccs.Fair;

public class ModeratorReviewDetailsModel : ModeratorReviewModel
{
	public string AccountAddress { get; set; }
	public string AccountNickname { get; set; }

	public ModeratorReviewDetailsModel(Review review, FairAccount account) : base(review)
	{
		AccountAddress = account.Address.ToString();
		AccountNickname = account.Nickname;
	}
}
