namespace Uccs.Fair;

public class ReviewTextModerationModel : BaseVotableOperationModel
{
	public string ReviewId { get; set; }

	public byte[] Hash { get; set; }

	public bool Resolution { get; set; }

	public ReviewTextModerationModel(ReviewEditModeration operation)
	{
		ReviewId = operation.Review.ToString();
		Hash = operation.Hash;
		Resolution = operation.Resolution;
	}
}
