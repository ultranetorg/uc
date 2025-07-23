namespace Uccs.Fair;

public class PublicationModel(Publication publication, Product product, Category category, byte[]? logo)
	: PublicationBaseModel(publication, product)
{
	public byte[]? Logo { get; set; } = logo;
	public string CategoryId { get; set; } = category.Id.ToString();
	public string CategoryTitle { get; set; } = category.Title;
}