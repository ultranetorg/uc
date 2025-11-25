namespace Uccs.Fair;

public class ReviewModel(Review review, FairAccount account)
{
	public string Id { get; set; } = review.Id.ToString();

	public string Text { get; set; } = review.Text;

	public byte Rating { get; set; } = review.Rating;

	public int Created { get; set; } = review.Created.Days;

	public AccountBaseAvatarModel CreatorAccount { get; set; } = new(account);
}
