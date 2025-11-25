using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationImageBaseModel
{
	[JsonPropertyOrder(-3)]
	public string? Id { get; }

	[JsonPropertyOrder(-2)]
	public string Title { get; }

	public string? CategoryTitle { get; }

	public string? ImageId { get; }

	public PublicationImageBaseModel(Product product, AutoId? imageId)
	{
		Title = PublicationUtils.GetLatestTitle(product);
		ImageId = imageId?.ToString();
	}

	public PublicationImageBaseModel(Publication publication, Product product, string categoryTitle, AutoId? imageId)
	{
		Id = publication.Id.ToString();
		Title = PublicationUtils.GetTitle(publication, product);
		CategoryTitle = categoryTitle;
		ImageId = imageId?.ToString();
	}
}
