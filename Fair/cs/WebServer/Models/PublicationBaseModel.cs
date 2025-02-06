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

	public PublicationBaseModel(EntityId id, Product product)
	{
		Id = id.ToString();

		ProductId = product.Id.ToString();
		ProductTitle = ProductUtils.GetTitle(product);
		ProductDescription = ProductUtils.GetDescription(product);
	}
}
