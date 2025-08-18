namespace Uccs.Fair;

public class ReviewEditModel(ReviewEdit operation) : BaseVotableOperationModel(operation)
{
	public string ReviewId { get; set; } = operation.Review.ToString();

	public string Text { get; set; } = operation.Text;
}