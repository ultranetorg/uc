using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class ProductsService(
	ILogger<ProductsService> logger,
	FairMcv mcv
)
{
	public IEnumerable<ProductFieldValueModel> GetFields([NotNull] [NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService),
			nameof(GetFields), productId);

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

	public ProductFieldCompareModel GetUpdatedFieldsByPublication([NotNull] [NotEmpty] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(ProductsService),
			nameof(GetUpdatedFieldsByPublication), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		AutoId id = AutoId.Parse(publicationId);

		lock(mcv.Lock)
		{
			id = mcv.Publications.Latest(id).Product;

			Product product = mcv.Products.Latest(id);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), id.ToString());
			}

			if(product.Versions.Length < 2)
			{
				throw new InvalidAutoIdException(nameof(Product).ToLower(), id.ToString());
			}

			var (from, to) = product.Versions
				.OrderBy(x => x.Id)
				.Take(2)
				.Select(x => x.Fields)
				.Select(fields => MapValues(fields, Product.Software))
				.ToArray();

			return new ProductFieldCompareModel { From = from, To = to, };
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
				Metadata =
					valueField?.Fields?.Select(field =>
						new ProductFieldValueMetadataModel { Name = field.Name, Type = field.Type }),
				Value = valueField?.Type == FieldType.FileId ? value.AsAutoId.ToString() : value.Value,
				Children = value.Fields?.Length > 0
					? MapValues(value.Fields, valueField?.Fields)
					: null
			};
	}
}