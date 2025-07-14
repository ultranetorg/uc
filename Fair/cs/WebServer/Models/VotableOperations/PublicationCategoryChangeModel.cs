namespace Uccs.Fair;

public class PublicationPublishModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }
	public string CategoryId { get; set; }

	public PublicationPublishModel(PublicationPublish operation)
	{
		PublicationId = operation.Publication.ToString();
		CategoryId = operation.Category.ToString();
	}
}
