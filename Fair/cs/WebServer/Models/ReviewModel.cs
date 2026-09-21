namespace Uccs.Fair;

public class ReviewModel(Review review, FairUser user)
{
	public AutoId Id { get; set; } = review.Id;

	public string Text { get; set; } = review.Text;

	public byte Rating { get; set; } = review.Rating;

	public Time Created { get; set; } = review.Created;

	public UserBaseAvatarModel CreatorUser { get; set; } = new(user);

	public AutoId PublicationId { get; init; }
	public string PublicationTitle { get; init; }
}
