using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class UnpublishedStoreProductsService
(
	ILogger<UnpublishedStoreProductsService> logger,
	FairMcv mcv
)
{
	public ProductDetailsModel GetDetails([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ProductId}", nameof(UnpublishedStoreProductsService), nameof(GetDetails), storeId, productId);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(productId);

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
