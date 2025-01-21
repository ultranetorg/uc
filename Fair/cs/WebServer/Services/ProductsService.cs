using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsService : IProductsService
{
	private readonly ILogger<ProductsService> _logger;
	private readonly FairMcv _mcv;

	private readonly IList<ProductModel> _mockedProducts = new List<ProductModel>();

	public ProductsService(ILogger<ProductsService> logger, FairMcv mcv)
	{
		_logger = logger;
		_mcv = mcv;

		var random = new Random();
		_mockedProducts = Enumerable.Range(1, 51).Select(i => new ProductModel
		{
			Id = new EntityId(random.Next(0, 1000000), random.Next(0,10)).ToString(),
			Name = "Name" + new EntityId(random.Next(0, 1000000), random.Next(0, 10)).ToString(),
			AuthorId = new EntityId(random.Next(0, 1000000), random.Next(0, 10)).ToString(),
			AuthorName = "AuthorName" + new EntityId(random.Next(0, 1000000), random.Next(0, 10)).ToString(),
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
			Updated = new Time(random.Next(0, 1000)).Days,
		}).ToList();
	}

	public ProductModel GetProduct(string productId)
	{
		_logger.LogDebug($"GET {nameof(ProductsService)}.{nameof(GetProduct)} method called with {{ProductId}}", productId);

		Guard.Against.NullOrEmpty(productId);

		EntityId entityId = EntityId.Parse(productId);

		ProductEntry product = null;
		string authorName = null;
		lock (_mcv.Lock)
		{
			product = _mcv.Products.Find(entityId, _mcv.Tail.Count);
			if (product == null)
			{
				return null;
			}

			AuthorEntry author = _mcv.Authors.Find(product.AuthorId, _mcv.Tail.Count);
			authorName = "AuthorName" + author.Id.ToString();
		}

		return BuildModel(product, authorName);
	}

	private static ProductModel BuildModel(ProductEntry entry, string authorName)
	{
		string id = entry.Id.ToString();
		return new ProductModel
		{
			Id = id,
			Name = "Name" + id,
			AuthorId = entry.AuthorId.ToString(),
			AuthorName = authorName,
			Flags = entry.Flags,
			Fields = entry.Fields,
			Updated = entry.Updated.Days,
		};
	}

	public TotalItemsResult<ProductModel> GetProducts(string name, int page, int pageSize)
	{
		_logger.LogDebug($"GET {nameof(ProductsService)}.{nameof(GetProducts)} method called with {{Name}}, {{Page}}, {{PageSize}}", name, page, pageSize);

		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		IEnumerable<ProductModel> filteredByName = !string.IsNullOrEmpty(name)
			? _mockedProducts.Where(x => x.Fields.Any(x => x.Type == ProductProperty.Description && x.Value.ToString()?.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1))
			: _mockedProducts;

		IEnumerable<ProductModel> result = filteredByName.Skip(page * pageSize).Take(pageSize);

		return new TotalItemsResult<ProductModel>
		{
			Items = result,
			TotalItems = _mockedProducts.Count,
		};
	}
}
