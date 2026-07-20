namespace Uccs.Fair;

public class ReviewModel(Review review, FairUser user)
{
	public string Id { get; set; } = review.Id.ToString();

	public string Text { get; set; } = review.Text;

	public byte Rating { get; set; } = review.Rating;

	public int Created { get; set; } = review.Created.Days;

	public UserBaseAvatarModel CreatorUser { get; set; } = new(user);

	public string PublicationId { get; init; }
	public string PublicationTitle { get; init; }
}
