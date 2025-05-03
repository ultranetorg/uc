using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationExtendedModel(Publication publication, Product product, Author author, Category category)
	: PublicationModel(publication, product)
{
	public string AuthorId { get; set; } = author.Id.ToString();
	public string AuthorTitle { get; set; } = author.Title;

	public string CategoryId { get; set; } = category.Id.ToString();
	public string CategoryTitle { get; set; } = category.Title;
}
