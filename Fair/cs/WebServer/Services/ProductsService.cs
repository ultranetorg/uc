using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsService : IProductsService
{
	private readonly ILogger<ProductsService> _logger;
	private readonly FairMcv _mcv;

	private readonly IList<ProductEntry> _mockedProducts = new List<ProductEntry>();

	public ProductsService(ILogger<ProductsService> logger, FairMcv mcv)
	{
		_logger = logger;
		_mcv = mcv;

		var random = new Random();
		_mockedProducts = Enumerable.Range(1, 51).Select(i => new ProductEntry
		{
			Id = new EntityId(random.Next(0, 1000000), random.Next(0,10)),
			AuthorId = new EntityId(random.Next(0, 1000000), random.Next(0, 10)),
			Flags = ProductFlags.None,
			Fields = new[]
			{
				new ProductField
				{
					Type = ProductProperty.Description,
					Value = $"Description of product {i}"
				},
				new ProductField
				{
					Type = ProductProperty.Description,
					Value = $"Extra Description of product {i}"
				}
			},
			Updated = new Time(random.Next(0, 1000)),
		}).ToList();
	}

	public ProductEntry GetProduct(string productId)
	{
		_logger.LogDebug($"GET {nameof(ProductsService)}.{nameof(GetProduct)} method called with {{ProductId}}", productId);

		Guard.Against.NullOrEmpty(productId);

		EntityId entityId = EntityId.Parse(productId);

		lock (_mcv.Lock)
		{
			return _mcv.Products.Find(entityId, _mcv.Tail.Count);
		}
	}

	public TotalItemsResult<ProductEntry> GetProducts(string name, int page, int pageSize)
	{
		_logger.LogDebug($"GET {nameof(ProductsService)}.{nameof(GetProducts)} method called with {{Name}}, {{Page}}, {{PageSize}}", name, page, pageSize);

		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		IEnumerable<ProductEntry> filteredByName = !string.IsNullOrEmpty(name)
			? _mockedProducts.Where(x => x.Fields.Any(x => x.Type == ProductProperty.Description && x.Value.ToString()?.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1))
			: _mockedProducts;

		IEnumerable<ProductEntry> result = filteredByName.Skip(page * pageSize).Take(pageSize);

		return new TotalItemsResult<ProductEntry>
		{
			Items = result,
			TotalItems = _mockedProducts.Count,
		};
	}
}
