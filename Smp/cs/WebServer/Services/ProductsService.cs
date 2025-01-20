using Ardalis.GuardClauses;

namespace Uccs.Smp;

public class ProductsService(ILogger<ProductsService> logger, SmpMcv mcv) : IProductsService
{
	public ProductEntry GetProduct(string productId)
	{
		Guard.Against.Empty(productId);

		logger.LogInformation($"GET {nameof(ProductsService)}.{nameof(GetProduct)} method called with {{ProductId}}", productId);

		EntityId entityId = EntityId.Parse(productId);

		lock (mcv.Lock)
		{
			return mcv.Products.Find(entityId, mcv.Tail.Count);
		}
	}
}
