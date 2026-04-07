using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UnpublishedPublicationsService
(
	ILogger<UnpublishedPublicationsService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<UnpublishedProductModel> GetAll([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(UnpublishedPublicationsService), nameof(GetAll), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock(mcv.Lock)
		{
			AutoId id = AutoId.Parse(siteId);
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
			List<UnpublishedProductModel> result = LoadUnpublishedPublications(publicationsIds, pageSize, cancellationToken);

			return new TotalItemsResult<UnpublishedProductModel>
			{
				Items = result,
				TotalItems = site.UnpublishedPublications.Length
			};
		}
	}

	List<UnpublishedProductModel> LoadUnpublishedPublications(IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
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
			FairUser account = (FairUser)mcv.Users.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			UnpublishedProductModel model = new UnpublishedProductModel(publicationId, product, account, fileId);
			result.Add(model);
		}

		return result;
	}

	public PublicationDetailsModel GetDetails([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {PublicationId}", nameof(UnpublishedPublicationsService), nameof(GetDetails), siteId, publicationId);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(publicationId);

		lock(mcv.Lock)
		{
			AutoId entitySiteId = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(entitySiteId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId entityPublicationId = AutoId.Parse(publicationId);
			if(!site.UnpublishedPublications.Contains(entityPublicationId))
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Publication publication = mcv.Publications.Latest(entityPublicationId);
			if(publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);
			IEnumerable<FieldValueModel> mappedFields = GetMappedValue(product, publication);

			return new PublicationDetailsModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetTitle(publication, product),
				LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
				Updated = product.Updated.Hours,
				Fields = mappedFields,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString(),
			};
		}

		IEnumerable<FieldValueModel> GetMappedValue(Product product, Publication publication)
		{
			FieldValue[] fields = GetFieldsVersion(product, publication.ProductVersion);
			if(fields != null)
			{
				Field[] declaration = Product.FindDeclaration(product.Type);
				return ProductsService.MapValues(declaration, fields);
			}

			return null;
		}

		FieldValue[]? GetFieldsVersion(Product product, int publicationProductVersion) => product.Versions.First(x => x.Id == publicationProductVersion)?.Fields;
	}
}
