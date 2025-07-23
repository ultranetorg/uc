namespace Uccs.Fair;

public class PublicationPublishModel(PublicationPublish operation) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();

	public string CategoryId { get; set; } = operation.Category.ToString();
}
