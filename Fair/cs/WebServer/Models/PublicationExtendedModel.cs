using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationExtendedModel(Publication publication, Product product, Author author, Category category) : PublicationModel(publication, product, category)
{
	public AutoId AuthorId { get; set; } = author.Id;
	public string AuthorTitle { get; set; } = author.Title;
	public AutoId? AuthorFileId { get; } = author.Avatar;

	public ProductType? Type { get; init; }
}
