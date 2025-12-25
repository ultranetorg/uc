using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

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

	public UnpublishedProductDetailsModel GetUnpublishedProduct([NotNull][NotEmpty] string unpublishedProductId, [NotEmpty] string siteId = null)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UnpublishedProductId}, {SiteId}", nameof(ProductsService), nameof(GetUnpublishedProduct), unpublishedProductId, siteId);

		if(siteId != null)
		{
			Guard.Against.NullOrEmpty(siteId);
		}
		Guard.Against.NullOrEmpty(unpublishedProductId);

		AutoId entityUnpublishedProductId = AutoId.Parse(unpublishedProductId);

		lock(mcv.Lock)
		{
			if(siteId != null)
			{
				AutoId entitySiteId = AutoId.Parse(siteId);
				Site site = mcv.Sites.Latest(entitySiteId);
				if(site == null)
				{
					throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
				}
				if(!site.UnpublishedPublications.Contains(entityUnpublishedProductId))
				{
					throw new EntityNotFoundException(nameof(EntityNames.UnpublishedProductEntityName).ToLower(), unpublishedProductId);
				}
			}

			Product product = mcv.Products.Latest(entityUnpublishedProductId);
			if(siteId == null && product == null)
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), unpublishedProductId);
			}

			FairAccount account = (FairAccount) mcv.Accounts.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLatestLogo(product);

			FieldValue[] fields = GetFieldsLastVersion(product);
			IEnumerable<ProductFieldValueModel> mappedFields = fields != null ? MapValues(fields, Product.Software) : null;
			return new UnpublishedProductDetailsModel(product, account, fileId)
			{
				Versions = mappedFields
			};
		}
	}


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

			FieldValue[] fields = GetFieldsLastVersion(product);
			return fields != null ? MapValues(fields, Product.Software) : [];
		}
	}

	FieldValue[]? GetFieldsLastVersion(Product product) => product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields;

	public TotalItemsResult<UnpublishedProductModel> GetUnpublishedProducts([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetUnpublishedProducts), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(site.UnpublishedPublications.Length == 0)
			{
				return TotalItemsResult<UnpublishedProductModel>.Empty;
			}

			IEnumerable<AutoId> publicationsIds = site.UnpublishedPublications.Skip(page * pageSize).Take(pageSize);
			List<UnpublishedProductModel> result = LoadUnpublishedProducts(publicationsIds, pageSize, cancellationToken);

			return new TotalItemsResult<UnpublishedProductModel>
			{
				Items = result,
				TotalItems = site.UnpublishedPublications.Length
			};
		}
	}

	List<UnpublishedProductModel> LoadUnpublishedProducts(IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<UnpublishedProductModel> result = new(pageSize);
		foreach(var publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			FairAccount account = (FairAccount)mcv.Accounts.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			UnpublishedProductModel model = new UnpublishedProductModel(product, account, fileId);
			result.Add(model);
		}

		return result;
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

	public static IEnumerable<ProductFieldValueModel> MapValues(FieldValue[] values, Field[] metaFields)
	{
		metaFields ??= [];

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

		if(type == null)
			return field.AsUtf8;

		switch(type)
		{
			case FieldType.Integer:
				return BinaryPrimitives.ReadInt32LittleEndian(field.Value);
			case FieldType.Float:
				return BinaryPrimitives.ReadDoubleLittleEndian(field.Value);
			case FieldType.TextUtf8:
			case FieldType.StringUtf8:
			case FieldType.URI:
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