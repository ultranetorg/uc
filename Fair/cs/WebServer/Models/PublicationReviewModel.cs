namespace Uccs.Fair;

public class PublicationReviewModel
{
	public string Id { get; set; }

	public string Text { get; set; }

	public byte Rating { get; set; }

	public int Created { get; set; }

	// public EntityId User { get; set; }
	public string AccountId { get; set; }
	public string AccountAddress { get; set; }

	public PublicationReviewModel(Review review, Account account)
	{
		Id = review.Id.ToString();
		Text = review.Text;
		Rating = review.Rating;
		Created = review.Created.Days;

		AccountId = account.Id.ToString();
		AccountAddress = account.Address.ToString();
	}
}
