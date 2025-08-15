using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationImageBaseModel
{
	[JsonPropertyOrder(-3)]
	public string? Id { get; }

	[JsonPropertyOrder(-2)]
	public string Title { get; }

	public string? CategoryTitle { get; }

	public byte[]? Image { get; }

	public PublicationImageBaseModel(Product product, byte[]? image)
	{
		Title = PublicationUtils.GetLatestTitle(product);
		Image = image;
	}

	public PublicationImageBaseModel(Publication publication, Product product, string categoryTitle, byte[]? image)
	{
		Id = publication.Id.ToString();
		Title = PublicationUtils.GetTitle(publication, product);
		CategoryTitle = categoryTitle;
		Image = image;
	}
}
