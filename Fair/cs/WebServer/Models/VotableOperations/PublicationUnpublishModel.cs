namespace Uccs.Fair;

public class PublicationUnpublishModel(PublicationUnpublish operation, Product product, Category category) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; set; } = operation.Publication.ToString();
	public string? PublicationTitle { get; } = PublicationUtils.GetLatestTitle(product);

	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;
}
