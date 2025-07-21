namespace Uccs.Fair;

public class ReviewStatusChangeModel(ReviewStatusChange operation) : BaseVotableOperationModel(operation)
{
	public string ReviewId { get; set; }

	public ReviewStatus Status { get; set; }
}
