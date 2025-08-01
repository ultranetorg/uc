namespace Uccs.Fair;

public class ReviewCreationModel(ReviewCreation operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();

	public string Text { get; set; } = operation.Text;
	public byte Rating { get; set; } = operation.Rating;
}
