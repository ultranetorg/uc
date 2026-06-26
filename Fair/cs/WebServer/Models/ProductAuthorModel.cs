namespace Uccs.Fair;

public class ProductAuthorModel(Product product)
{
	public string Id { get; init; }
	public string? Title { get; init; }
	public string? LogoId { get; init; }

	public int PublicationsCount { get; init; } = product.Publications.Length;
}
