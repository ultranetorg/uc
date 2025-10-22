using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class ProductsService
(
	ILogger<ProductsService> logger,
	FairMcv mcv
)
{
	public IEnumerable<ProductFieldValueModel> GetFields([NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(GetFields), productId);

		Guard.Against.NullOrEmpty(productId);

		AutoId id = AutoId.Parse(productId);

		lock(mcv.Lock)
		{
			Product product = mcv.Products.Latest(id);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			var fields = product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields;

			return fields != null ? MapValues(fields, Product.Software) : [];
		}
	}

	private IEnumerable<ProductFieldValueModel> MapValues(FieldValue[] values, Field[] metaFields)
	{
		return from value in values
			let valueField = metaFields.FirstOrDefault(d => d.Name == value.Name)
			select new ProductFieldValueModel
			{
				Name = value.Name,
				Type = valueField?.Type,
				Metadata = valueField?.Fields?.Select(field => new ProductFieldValueMetadataModel
				{
					Name = field.Name, Type = field.Type
				}),
				Value = value.Value,
				Children = value.Fields?.Length > 0
					? MapValues(value.Fields, valueField?.Fields)
					: null
			};
	}
}