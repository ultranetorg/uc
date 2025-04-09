namespace Uccs.Fair;

public class ReviewStatusChangeModel : BaseVotableOperationModel
{
	public string ReviewId { get; set; }

	public ReviewStatus Status { get; set; }

	public ReviewStatusChangeModel(ReviewStatusChange operation)
	{
		ReviewId = operation.Review.ToString();
		Status = operation.Status;
	}
}
