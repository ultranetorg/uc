namespace Uccs.Fair;

public class ReviewDeletionModel(ReviewDeletion operation) : BaseVotableOperationModel(operation)
{
	public AutoId ReviewId { get; set; } = operation.Review;

	//public ReviewStatus Status { get; set; } = operation.Status;
}
