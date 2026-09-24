namespace Uccs.Fair;

public class PublicationPublishModel(PublicationPublish operation, Product product, Category category) : BaseVotableOperationModel(operation)
{
	public AutoId PublicationId { get; } = operation.Publication;
	public string? PublicationTitle { get; } = PublicationUtils.GetLatestTitle(product);

	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;
}
