using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Ardalis.GuardClauses;
using NativeImport;

namespace Uccs.Fair;

public class ProductsService
(
	ILogger<ProductsService> logger,
	FairMcv mcv
)
{
	public bool UnpublishedProductExists([NotNull][NotEmpty] string unpublishedProductId)
	{
		Guard.Against.NullOrEmpty(unpublishedProductId);

		AutoId id = AutoId.Parse(unpublishedProductId);

		lock(mcv.Lock)
		{
			Product product = mcv.Products.Latest(id);
			return product != null;
		}
	}

	public ProductDetailsModel GetUnpublishedSiteProduct([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ProductId}", nameof(ProductsService), nameof(GetUnpublishedSiteProduct), siteId, productId);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(productId);

		lock(mcv.Lock)
		{
			AutoId entitySiteId = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(entitySiteId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId entityProductId = AutoId.Parse(productId);
			Product product = mcv.Products.Latest(entityProductId);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			if(product.Publications.Any(i => mcv.Publications.Latest(i).Site == site.Id))
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			Author author = mcv.Authors.Latest(product.Author);
			IEnumerable<FieldValueModel> mappedFields = GetMappedValue(product);

			return new ProductDetailsModel
			{
				Id = product.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetLatestTitle(product),
				LogoId = PublicationUtils.GetLatestLogo(product)?.ToString(),
				Updated = product.Updated.Hours,
				Fields = mappedFields,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString()
			};
		}
	}

	IEnumerable<FieldValueModel> GetMappedValue(Product product)
	{
		FieldValue[] fields = GetFieldsLastVersion(product);
		if(fields != null)
		{
			Field[] declaration = Product.FindDeclaration(product.Type);
			return MapValues(declaration, fields);
		}

		return null;
	}

	public IEnumerable<FieldValueModel> GetFields([NotNull][NotEmpty] string productId)
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

			FieldValue[] fields = GetFieldsLastVersion(product);
			return fields != null ? MapValues(Product.Software, fields) : [];
		}
	}

	FieldValue[]? GetFieldsLastVersion(Product product) => product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields;

	public ProductDetailsModel GetDetails([NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(GetDetails), productId);

		Guard.Against.NullOrEmpty(productId);

		AutoId id = AutoId.Parse(productId);

		lock(mcv.Lock)
		{
			Product product = mcv.Products.Latest(id);
			if(product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			Author author = mcv.Authors.Latest(product.Author);

			IEnumerable<FieldValueModel>? productFields = null;
			FieldValue[]? fields = GetFieldsLastVersion(product);
			if (fields != null)
			{
				Field[] declaration = Product.FindDeclaration(product.Type);
				productFields = ProductsService.MapValues(declaration, fields);
			}

			return new ProductDetailsModel
			{
				Id = product.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetLatestTitle(product),
				LogoId = PublicationUtils.GetLatestLogo(product)?.ToString(),
				Updated = product.Updated.Hours,
				Fields = productFields,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString()
			};
		}
	}

	public FieldValueCompareModel GetUpdatedFieldsByPublication([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int version)
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
			var mappedFrom = MapValues(Product.Software, fieldsFrom);
			var mappedTo = MapValues(Product.Software, fieldsTo);

			return new FieldValueCompareModel {From = mappedFrom, To = mappedTo};
		}
	}

	public static IEnumerable<FieldValueModel> MapValues(Field[] declarationFields, FieldValue[] productFields)
	{
		var result = new List<FieldValueModel>();

		foreach(FieldValue value in productFields)
		{
			var declarationField = declarationFields.FirstOrDefault(d => d.Name == value.Name);
			if(declarationField == null)
				continue;

			var model = new FieldValueModel
			{
				Name = value.Name,
				Type = declarationField?.Type,
				Value = ConvertValue(declarationField?.Type, value),
				Children = value.Fields != null && value.Fields.Length > 0
					? MapValues(declarationField?.Fields ?? [], value.Fields)
					: null
			};

			result.Add(model);
		}

		return result;
	}

	static object ConvertValue(FieldType? declarationType, FieldValue field)
	{
		if(field?.Value == null)
			return null;

		switch(declarationType)
		{
			case FieldType.Integer:
				return BinaryPrimitives.ReadInt32LittleEndian(field.Value);
			case FieldType.Float:
				return BinaryPrimitives.ReadDoubleLittleEndian(field.Value);

			case FieldType.TextUtf8:
			case FieldType.StringUtf8:
			case FieldType.URI:
			case FieldType.URL:
			case FieldType.LanguageCode:
			case FieldType.License:
			case FieldType.DistributionType:
			case FieldType.Platform:
			case FieldType.OS:
			case FieldType.CPUArchitecture:
			case FieldType.Hash:
				return field.AsUtf8;

			case FieldType.StringAnsi:
				return Encoding.Default.GetString(field.Value);

			case FieldType.Money:
				return BinaryPrimitives.ReadInt64LittleEndian(field.Value);

			case FieldType.FileId:
				return field.AsAutoId.ToString();

			case FieldType.Date:
				return BinaryPrimitives.ReadInt32LittleEndian(field.Value);
		}

		return null;
	}
}