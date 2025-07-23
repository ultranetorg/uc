namespace Uccs.Fair;

public class ReviewEditModerationModel(ReviewEditModeration operation) : BaseVotableOperationModel(operation)
{
	public string ReviewId { get; set; } = operation.Review.ToString();

	public byte[] Hash { get; set; } = operation.Hash;

	public bool Resolution { get; set; } = operation.Resolution;
}
