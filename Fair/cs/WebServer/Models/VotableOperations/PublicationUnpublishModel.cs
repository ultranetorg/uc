namespace Uccs.Fair;

public class PublicationUnpublishModel(PublicationUnpublish operation, Product product, Category category) : BaseVotableOperationModel(operation)
{
	public AutoId PublicationId { get; set; } = operation.Publication;
	public string? PublicationTitle { get; } = PublicationUtils.GetLatestTitle(product);

	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;
}
