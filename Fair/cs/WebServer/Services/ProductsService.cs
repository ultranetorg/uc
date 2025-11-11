using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using System.Text;

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
			Publication publication = mcv.Publications.Latest(id);
			if(publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			id = publication.Product;
			int currentVersion = publication.ProductVersion;

			Product product = mcv.Products.Latest(id);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), id.ToString());
			}

			if(product.Versions.Length < 2)
			{
				throw new InvalidEntityException(nameof(Product).ToLower(), id.ToString());
			}

			ProductVersion[] compareVersions =
			[
				product.Versions.Single(x => x.Id == currentVersion),
				product.Versions
					.OrderBy(x => x.Id)
					.Last(x => x.Id != currentVersion)
			];

			var (from, to) = compareVersions
				.Select(x => x.Fields)
				.Select(fields => MapValues(fields, Product.Software))
				.ToArray();

			return new ProductFieldCompareModel { From = from, To = to, };
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
		if(field?.Value == null || type == null)
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
			case FieldType.Date:
				return field.AsUtf8;
			case FieldType.StringAnsi:
				return Encoding.Default.GetString(field.Value);
			case FieldType.Money:
				return BinaryPrimitives.ReadInt64LittleEndian(field.Value);
			case FieldType.None:
			case FieldType.FileId:
				return field.AsAutoId.ToString();
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
		}
	}
}