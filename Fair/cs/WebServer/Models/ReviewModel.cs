namespace Uccs.Fair;

public class ReviewModel
{
	public string Id { get; set; }

	public string Text { get; set; }

	public byte Rating { get; set; }

	public int Created { get; set; }

	public string AccountId { get; set; }
	public string AccountAddress { get; set; }
	public string AccountNickname { get; set; }

	public ReviewModel(Review review, FairAccount account)
	{
		Id = review.Id.ToString();
		Text = review.Text;
		Rating = review.Rating;
		Created = review.Created.Days;

		AccountId = account.Id.ToString();
		AccountAddress = account.Address.ToString();
		AccountNickname = account.Nickname;
	}
}
