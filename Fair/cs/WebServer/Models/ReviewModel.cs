namespace Uccs.Fair;

public class ReviewModel
{
	public string Id { get; set; }

	public string Text { get; set; }
	public string TextNew { get; set; }

	public byte Rating { get; set; }

	public int Created { get; set; }

	public string PublicationId { get; set; }

	public string AccountId { get; set; }

	public ReviewModel(Review review)
	{
		Id = review.Id.ToString();
		Text = review.Text;
		TextNew = review.TextNew;
		Rating = review.Rating;
		Created = review.Created.Days;

		PublicationId = review.Publication.ToString();

		AccountId = review.Creator.ToString();
	}
}
