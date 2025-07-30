using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Uccs.Fair;

public class ModeratorReviewModel
{
	public string Id { get; set; }

	public string Text { get; set; }
	public string TextNew { get; set; }

	public byte Rating { get; set; }

	public int Created { get; set; }

	public string PublicationId { get; set; }

	public AccountBaseModel Creator { get; set; }

	public ModeratorReviewModel(ReviewEdit operation, Review review, FairAccount creator)
	{
		Id = operation.Id.ToString();

		Text = review.Text;
		TextNew = operation.Text;
		Rating = review.Rating;
		Created = review.Created.Days;
		PublicationId = review.Publication.ToString();

		Creator = new AccountBaseModel(creator);
	}

	public ModeratorReviewModel(Proposal proposal, ReviewCreation operation, FairAccount creator)
	{
		Id = operation.Id.ToString();

		TextNew = operation.Text;
		Rating = operation.Rating;
		Created = proposal.CreationTime.Days;
		PublicationId = operation.Publication.ToString();

		Creator = new AccountBaseModel(creator);
	}
}
