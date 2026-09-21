namespace Uccs.Fair;

public class ProductAuthorModel(Product product)
{
	public AutoId Id { get; init; }
	public string? Title { get; init; }
	public AutoId? LogoId { get; init; }

	public int PublicationsCount { get; init; } = product.Publications.Length;
}
