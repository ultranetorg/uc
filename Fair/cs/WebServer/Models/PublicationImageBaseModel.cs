using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationImageBaseModel
{
	[JsonPropertyOrder(-3)]
	public AutoId? Id { get; }

	[JsonPropertyOrder(-2)]
	public string Title { get; }

	public string? CategoryTitle { get; }

	public AutoId? ImageId { get; }

	public PublicationImageBaseModel(Product product, AutoId? imageId)
	{
		Title = PublicationUtils.GetLatestTitle(product);
		ImageId = imageId;
	}

	public PublicationImageBaseModel(Publication publication, Product product, string? categoryTitle, AutoId? imageId)
	{
		Id = publication.Id;
		Title = product.Title;
		CategoryTitle = categoryTitle;
		ImageId = imageId;
	}
}
