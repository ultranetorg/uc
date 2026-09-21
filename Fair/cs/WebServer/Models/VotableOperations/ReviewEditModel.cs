namespace Uccs.Fair;

public class ReviewEditModel(ReviewEdit operation) : BaseVotableOperationModel(operation)
{
	public AutoId ReviewId { get; set; } = operation.Review;

	public string Text { get; set; } = operation.Text;
}