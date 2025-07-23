namespace Uccs.Fair;

public class ReviewModel(Review review, FairAccount account)
{
	public string Id { get; set; } = review.Id.ToString();

	public string Text { get; set; } = review.Text;

	public byte Rating { get; set; } = review.Rating;

	public int Created { get; set; } = review.Created.Days;

	public string AccountId { get; set; } = account.Id.ToString();
	public string AccountAddress { get; set; } = account.Address.ToString();
	public string AccountNickname { get; set; } = account.Nickname;
	public byte[] AccountAvatar { get; set; } = account.Avatar;
}
