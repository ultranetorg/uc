namespace Uccs.Fair;

public class PublicationAuthorModel(Publication publication, Product product)
	: PublicationBaseModel(publication, product)
{
	public string ProductId { get; init; }

	public string? LogoId { get; init; }

	public int PublicationsCount { get; init; } = product.Publications.Length;

	public string CategoryId { get; init; }
	public string CategoryTitle { get; init; }
}
