namespace Uccs.Fair;

public class PublicationAuthorModel(Publication publication, Product product)
	: PublicationBaseModel(publication, product)
{
	public AutoId ProductId { get; init; }

	public AutoId? LogoId { get; init; }

	public int PublicationsCount { get; init; } = product.Publications.Length;

	public AutoId CategoryId { get; init; }
	public string CategoryTitle { get; init; }
}
