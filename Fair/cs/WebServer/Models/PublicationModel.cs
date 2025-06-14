namespace Uccs.Fair;

public class PublicationModel(Publication publication, Product product, Category category)
	: PublicationBaseModel(publication, product)
{
	public string CategoryId { get; set; } = category.Id.ToString();
	public string CategoryTitle { get; set; } = category.Title;
}