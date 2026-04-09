using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class ProductsService
(
	ILogger<ProductsService> logger,
	FairMcv mcv
)
{
	public IEnumerable<FieldValueModel>? GetFields([NotNull][NotEmpty] string productId)
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

			return ProductFieldsUtils.GetLatestMappedFields(product);
		}
	}

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

			IEnumerable<FieldValueModel>? productFields = ProductFieldsUtils.GetLatestMappedFields(product);

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

	public PublicationDetailsDiffModel GetDiff([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int version)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(ProductsService), nameof(GetDiff), publicationId);

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

			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);

			var fields = ProductFieldsUtils.GetMappedFieldsVersion(product, publication.ProductVersion);
			var fieldsTo = ProductFieldsUtils.GetMappedFieldsVersion(product, version);

			return new PublicationDetailsDiffModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetTitle(publication, product),
				LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
				Updated = product.Updated.Hours,
				Fields = fields,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString(),
				CategoryId = category?.Id.ToString(),
				CategoryTitle = category?.Title,
				Rating = publication.Rating,
				FieldsTo = fieldsTo
			};
		}
	}
}