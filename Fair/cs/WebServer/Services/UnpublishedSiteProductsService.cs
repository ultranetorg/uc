using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class UnpublishedSiteProductsService
(
	ILogger<UnpublishedSiteProductsService> logger,
	FairMcv mcv
)
{
	public ProductDetailsModel GetDetails([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ProductId}", nameof(UnpublishedSiteProductsService), nameof(GetDetails), siteId, productId);

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
			if (HasProductCreationProposalForProduct(site, product.Id))
			{
				throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
			}

			Author author = mcv.Authors.Latest(product.Author);
			IEnumerable<FieldValueModel> mappedFields = ProductFieldsUtils.GetLatestMappedFields(product);

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

		bool HasProductCreationProposalForProduct(Site site, AutoId productId)
		{
			return site.Proposals.Any(x =>
			{
				Proposal proposal = mcv.Proposals.Latest(x);
				if (proposal.OptionClass != FairOperationClass.PublicationCreation)
				{
					return false;
				}

				return (proposal.Options[0].Operation as PublicationCreation).Product == productId;
			});
		}
	}
}
