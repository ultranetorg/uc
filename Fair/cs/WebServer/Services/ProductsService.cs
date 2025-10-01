using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class ProductsService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
)
{
	public IEnumerable<ProductFieldValueModel> GetFields([NotNull] [NotEmpty] string productId)
	{
		logger.LogDebug($"{nameof(ProductsService)}.{nameof(GetFields)} method called with {{productId}}", productId);

		Guard.Against.NullOrEmpty(productId);
		AutoId id = AutoId.Parse(productId);

		lock(mcv.Lock)
		{
			Product product = mcv.Products.Latest(id);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			var fields = product.Versions
				.OrderByDescending(x => x.Id)
				.FirstOrDefault()
				?.Fields;

			return fields == null ? null : MapValues(fields);
		}
	}

	private IEnumerable<ProductFieldValueModel> MapValues(FieldValue[] values)
	{
		return from value in values
			let valueFields = Product.Software.FirstOrDefault(d => d.Name == value.Name)
			select new ProductFieldValueModel
			{
				Type = valueFields?.Type,
				Metadata = valueFields?.Fields.Select(field => new ProductFieldValueMetadataModel
				{
					Name = field.Name, Type = field.Type
				}),
				Value = value.Value,
				Children = value.Fields?.Length > 0
					? MapValues(value.Fields)
					: null
			};
	}
}