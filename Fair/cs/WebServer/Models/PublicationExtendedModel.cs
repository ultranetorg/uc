using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationExtendedModel(Publication publication, Product product, Author author, Category category, byte[]? logo)
	: PublicationModel(publication, product, category, logo)
{
	public string AuthorId { get; set; } = author.Id.ToString();
	public string AuthorTitle { get; set; } = author.Title;
	public string? AuthorFileId { get; } = author.Avatar?.ToString();
}
