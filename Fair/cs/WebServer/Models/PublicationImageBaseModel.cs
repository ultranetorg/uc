using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationImageBaseModel(Publication publication, Product product, string categoryTitle, byte[]? image)
{
	[JsonPropertyOrder(-3)]
	public string Id { get; } = publication.Id.ToString();

	[JsonPropertyOrder(-2)]
	public string Title { get; } = PublicationUtils.GetTitle(publication, product);

	public string CategoryTitle { get; } = categoryTitle;

	public byte[]? Image { get; } = image;
}
