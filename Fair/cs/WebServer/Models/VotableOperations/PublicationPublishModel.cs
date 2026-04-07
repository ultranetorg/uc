namespace Uccs.Fair;

public class PublicationPublishModel(PublicationPublish operation, Product product, Category category) : BaseVotableOperationModel(operation)
{
	public string PublicationId { get; } = operation.Publication.ToString();
	public string? PublicationTitle { get; } = PublicationUtils.GetLatestTitle(product);


	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;
}
