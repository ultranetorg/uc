namespace Uccs.Fair;

public class PublicationCategoryChangeModel : BaseVotableOperationModel
{
	public string PublicationId { get; set; }
	public string CategoryId { get; set; }

	public PublicationCategoryChangeModel(PublicationCategoryChange operation)
	{
		PublicationId = operation.Publication.ToString();
		CategoryId = operation.Category.ToString();
	}
}
