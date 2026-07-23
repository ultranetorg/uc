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
	public TotalItemsResult<UnpublishedPublicationModel> GetAll([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(UnpublishedPublicationsService), nameof(GetAll), storeId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if(store.UnpublishedPublications.Length == 0)
		{
			return TotalItemsResult<UnpublishedPublicationModel>.Empty;
		}

		IEnumerable<AutoId> publicationsIds = store.UnpublishedPublications.Skip(page * pageSize).Take(pageSize);
		IEnumerable<AutoId> reversed = publicationsIds.Reverse();
		List<UnpublishedPublicationModel> result = LoadUnpublishedPublications(store, reversed, pageSize, cancellationToken);

		return new TotalItemsResult<UnpublishedPublicationModel>
		{
			Items = result,
			TotalItems = store.UnpublishedPublications.Length
		};
	}

	List<UnpublishedPublicationModel> LoadUnpublishedPublications(Store store, IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<UnpublishedPublicationModel> result = new(pageSize);
		foreach(var publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			if(HasProductPublicationProposalForPublication(store, publicationId))
			{
				continue;
			}

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			UnpublishedPublicationModel model = new UnpublishedPublicationModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetLatestTitle(product),
				LogoId = PublicationUtils.GetLatestLogo(product)?.ToString(),
				Updated = product.Updated.Hours,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString()
			};
			result.Add(model);
		}

		return result;
	}

	public PublicationDetailsModel GetDetails([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {PublicationId}", nameof(UnpublishedPublicationsService), nameof(GetDetails), storeId, publicationId);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(publicationId);

		AutoId storeEntityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		AutoId entityPublicationId = AutoId.Parse(publicationId);
		if(!store.UnpublishedPublications.Contains(entityPublicationId))
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		Publication publication = mcv.Publications.Latest(entityPublicationId);
		if(publication == null)
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		//if(HasProductPublicationProposalForPublication(store, entityPublicationId))
		//{
		//	throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		//}

		Product product = mcv.Products.Latest(publication.Product);
		Author author = mcv.Authors.Latest(product.Author);
		AutoId? fileId = PublicationUtils.GetLogo(publication, product);
		IEnumerable<FieldValueModel> mappedFields = ProductFieldsUtils.GetMappedFields(product, publication);

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

	bool HasProductPublicationProposalForPublication(Store store, AutoId publicationId)
	{
		return store.Proposals.Any(x =>
		{
			Proposal proposal = mcv.Proposals.Latest(x);
			if(proposal.OptionClass != FairOperationClass.PublicationPublish)
			{
				return false;
			}

			return (proposal.Options[0].Operation as PublicationPublish).Publication == publicationId;
		});
	}
}
