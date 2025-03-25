using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationBaseModel
{
	[JsonPropertyOrder(-4)]
	public string Id { get; set; }

	[JsonPropertyOrder(-3)]
	public string ProductId { get; set; }
	[JsonPropertyOrder(-2)]
	public string ProductTitle { get; set; }
	[JsonPropertyOrder(-1)]
	public string ProductDescription { get; set; }

	public PublicationBaseModel(Publication publication, Product product)
	{
		Id = publication.Id.ToString();

		ProductId = product.Id.ToString();
		ProductTitle = ProductUtils.GetTitle(product, publication);
		ProductDescription = ProductUtils.GetDescription(product, publication);
	}
}
