namespace Uccs.Fair;

public class PublicationAuthorModel(Publication publication, Product product, byte[]? logo)
	: PublicationBaseModel(publication, product)
{
	public byte[]? Logo { get; set; } = logo;
	public int PublicationsCount { get; set; } = product.Publications.Length;
}
