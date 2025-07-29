namespace Uccs.Fair;

public class ReviewStatusChangeModel(ReviewStatusChange operation) : BaseVotableOperationModel(operation)
{
	public string ReviewId { get; set; } = operation.Review.ToString();

	public ReviewStatus Status { get; set; } = operation.Status;
}
