namespace Uccs.Fair;

public class ReviewCreationModel(ReviewCreation operation) : BaseVotableOperationModel(operation)
{
	public AutoId PublicationId { get; set; } = operation.Publication;

	public string Text { get; set; } = operation.Text;
	public byte Rating { get; set; } = operation.Rating;
}
