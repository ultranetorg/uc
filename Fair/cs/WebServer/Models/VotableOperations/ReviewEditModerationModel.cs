namespace Uccs.Fair;

public class ReviewEditModerationModel : BaseVotableOperationModel
{
	public string ReviewId { get; set; }

	public byte[] Hash { get; set; }

	public bool Resolution { get; set; }

	public ReviewEditModerationModel(ReviewEditModeration operation)
	{
		ReviewId = operation.Review.ToString();
		Hash = operation.Hash;
		Resolution = operation.Resolution;
	}
}
