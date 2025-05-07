using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationBaseModel
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public PublicationBaseModel(string id, string title)
	{
		Id = id;
		Title = title;
	}

	public PublicationBaseModel(Publication publication, Product product)
	{
		Id = publication.Id.ToString();
		Title = PublicationUtils.GetTitle(publication, product);
	}
}
