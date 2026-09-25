using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public class UnpublishedStoreProductsService
(
#if DEBUG
	ILogger<UnpublishedStoreProductsService> logger,
#endif
	FairMcv mcv
)
{
	public ProductDetailsModel GetDetails([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string productId)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(productId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ProductId}", nameof(UnpublishedStoreProductsService), nameof(UnpublishedStoreProductsService.GetDetails), storeId, productId);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		AutoId entityProductId = AutoId.Parse(productId);
		Product product = mcv.Products.Latest(entityProductId);
		if(product == null)
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		if(product.Publications.Any(i => mcv.Publications.Latest(i).Store == store.Id))
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}
		if (HasProductCreationProposalForProduct(store, product.Id))
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		Author author = mcv.Authors.Latest(product.Author);
		IEnumerable<FieldValueModel> mappedFields = ProductFieldsUtils.GetLatestMappedFields(product);

		return new ProductDetailsModel
		{
			Id = product.Id,
			Type = product.Type,
			Title = PublicationUtils.GetLatestTitle(product),
			LogoId = PublicationUtils.GetLatestLogo(product),
			Updated = product.Updated,
			Fields = mappedFields,
			AuthorId = author.Id,
			AuthorTitle = author.Title,
			AuthorLogoId = author.Avatar
		};

		bool HasProductCreationProposalForProduct(Store store, AutoId productId)
		{
			return store.Proposals.Any(x =>
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
