using System.Text.Json.Serialization;

namespace Uccs.Fair;

public abstract class PublicationBaseModel(Publication publication, Product product)
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; } = publication.Id.ToString();

	[JsonPropertyOrder(-1)]
	public string Title { get; set; } = PublicationUtils.GetTitle(publication, product);
}
