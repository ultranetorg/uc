using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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

	public ProductFieldCompareModel GetUpdatedFieldsByPublication([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int version)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(ProductsService), nameof(GetUpdatedFieldsByPublication), publicationId);

		Guard.Against.NullOrEmpty(publicationId);
		Guard.Against.Negative(version);

		AutoId id = AutoId.Parse(publicationId);

		lock(mcv.Lock)
		{
			Publication publication = mcv.Publications.Latest(id);
			if(publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			if(product.Versions.Length < 1 || product.Versions.All(x => x.Id != version))
			{
				throw new InvalidPublicationVersionException(publicationId, version);
			}

			var fieldsFrom = product.Versions.Single(x => x.Id == publication.ProductVersion).Fields;
			var fieldsTo = product.Versions.Single(x => x.Id == version).Fields;
			var mappedFrom = MapValues(fieldsFrom, Product.Software);
			var mappedTo = MapValues(fieldsTo, Product.Software);

			return new ProductFieldCompareModel {From = mappedFrom, To = mappedTo};
		}
	}

	private static IEnumerable<ProductFieldValueModel> MapValues(FieldValue[] values, Field[] metaFields)
	{
		return from value in values
			let valueField = metaFields.FirstOrDefault(d => d.Name == value.Name)
			select new ProductFieldValueModel
			{
				Name = value.Name,
				Type = valueField?.Type,
				Value = ConvertValue(valueField?.Type, value),
				Children = value.Fields?.Length > 0
					? MapValues(value.Fields, valueField?.Fields)
					: null
			};
	}

	private static object ConvertValue(FieldType? type, FieldValue field)
	{
		if(field?.Value == null)
			return null;

		switch(type)
		{
			case FieldType.Integer:
				return BinaryPrimitives.ReadInt32BigEndian(field.Value);
			case FieldType.Float:
				return BinaryPrimitives.ReadDoubleBigEndian(field.Value);
			case FieldType.TextUtf8:
			case FieldType.StringUtf8:
			case FieldType.URI:
			case FieldType.Language:
			case FieldType.License:
			case FieldType.Deployment:
			case FieldType.Platform:
			case FieldType.OS:
			case FieldType.CPUArchitecture:
			case FieldType.Hash:
				return field.AsUtf8;
			case FieldType.Date:
			case FieldType.StringAnsi:
				return Encoding.Default.GetString(field.Value);
			case FieldType.Money:
				return BinaryPrimitives.ReadInt64LittleEndian(field.Value);
			case FieldType.FileId:
				return field.AsAutoId.ToString();
		}

		return null;
	}
}