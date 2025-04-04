namespace Uccs.Fair;

public class ReviewModel
{
	public string Id { get; set; }

	public string Text { get; set; }
	public string TextNew { get; set; }

	public byte Rating { get; set; }

	public ReviewStatus Status { get; set; }

	public int Created { get; set; }

	public string PublicationId { get; set; }

	// public EntityId User { get; set; }
	public string AccountId { get; set; }
	public string AccountAddress { get; set; }

	public ReviewModel(Review review, Account account)
	{
		Id = review.Id.ToString();
		Text = review.Text;
		TextNew = review.TextNew;
		Status = review.Status;
		Created = review.Created.Days;

		PublicationId = review.Publication.ToString();

		AccountId = account.Id.ToString();
		AccountAddress = account.Address.ToString();
	}
}
