using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ModeratorProposalsService
(
#if DEBUG
	ILogger<ModeratorProposalsService> logger,
#endif
	FairMcv mcv
)
{
	public PublicationProposalModel GetPublicationProposal([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string proposalId)
	{
		ArgumentException.ThrowIfNullOrEmpty(proposalId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ProposalId}", nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetPublicationProposal), storeId, proposalId);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		return GetProposalByType<PublicationProposalModel>(storeId, proposalId, EntityNames.UserEntityName, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel);
	}

	public TotalItemsResult<PublicationProposalModel> GetPublicationProposalsNotOptimized
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}",
			nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetPublicationProposalsNotOptimized), storeId, page, pageSize, search);
#endif

		return GetProposalsByTypeNotOptimized(storeId, page, pageSize, search, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel, cancellationToken);
	}

	PublicationProposalModel CreatePublicationProposalModel(Proposal proposal, Store store)
	{
		if(proposal.Options[0].Operation is PublicationCreation publicationCreation)
		{
			return CreatePublicationModelFromProduct(proposal, store, publicationCreation.Product);
		}
		if(proposal.Options[0].Operation is PublicationDeletion publicationDeletion)
		{
			return CreatePublicationModel(proposal, store, publicationDeletion.Publication);
		}
		if(proposal.Options[0].Operation is PublicationPublish publicationPublish)
		{
			return CreatePublicationModel(proposal, store, publicationPublish.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUpdation publicationUpdation)
		{
			return CreatePublicationModel(proposal, store, publicationUpdation.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUnpublish publicationUnpublish)
		{
			return CreatePublicationModel(proposal, store, publicationUnpublish.Publication);
		}

		return null;
	}

	PublicationProposalModel CreatePublicationModelFromProduct(Proposal proposal, Store store, AutoId productId)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);
		Product product = mcv.Products.Latest(productId);
		Author author = mcv.Authors.Latest(product.Author);
		AutoId? fileId = PublicationUtils.GetLatestLogo(product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(product, fileId);

		return new PublicationProposalModel(proposal, creator, product, author, publicationImage);
	}

	PublicationProposalModel CreatePublicationModel(Proposal proposal, Store store, AutoId publicationId)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);
		Publication publication = mcv.Publications.Latest(publicationId);
		Product product = mcv.Products.Latest(publication.Product);
		Author author = mcv.Authors.Latest(product.Author);

		AutoId? fileId = PublicationUtils.GetLogo(publication, product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(publication, product, null, fileId);

		return new PublicationProposalModel(proposal, creator, product, author, publicationImage);
	}

	T GetProposalByType<T>(string storeId, string proposalId, string entityName, Predicate<Proposal> checkFunc, Func<Proposal, Store, T> createFunc) where T : ProposalModel
	{
		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if (store.Proposals.All(x => x != proposalEntityId))
		{
			throw new EntityNotFoundException(entityName, proposalId);
		}

		Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
		if (!ProposalUtils.IsDiscussion(store, proposal) || !checkFunc(proposal))
		{
			throw new EntityNotFoundException(entityName, proposalId);
		}

		return createFunc(proposal, store);
	}

	internal TotalItemsResult<T> GetProposalsByTypeNotOptimized<T>
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, Predicate<Proposal> checkFunc, Func<Proposal, Store, T> createFunc, CancellationToken cancellationToken)
		where T : ProposalModel
	{
		AutoId storeEntityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadProposalsByType<T>(store, page, pageSize, search, checkFunc, createFunc, cancellationToken);
	}

	TotalItemsResult<T> LoadProposalsByType<T>
		(Store store, int page, int pageSize, string? query, Predicate<Proposal> checkFunc, Func<Proposal, Store, T> createFunc, CancellationToken cancellationToken) where T : ProposalModel
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<T>.Empty;

		var items = new List<T>(pageSize);
		int totalItems = 0;

		IEnumerable<AutoId> reversed = store.Proposals.Reverse();
		foreach (AutoId proposalId in reversed)
		{
			if (cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<T>{Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if (!checkFunc(proposal) || !SearchUtils.IsMatch(proposal, query))
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				T model = createFunc(proposal, store);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<T>{Items = items, TotalItems = totalItems};
	}

	public TotalItemsResult<ModeratorProposalModel> GetModeratorProposalsNotOptimized
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetModeratorProposalsNotOptimized), storeId, page, pageSize, search);
#endif

		return GetProposalsByTypeNotOptimized(storeId, page, pageSize, search, ProposalUtils.IsModeratorOperation, CreateModeratorProposalModel, cancellationToken);
	}

	ModeratorProposalModel CreateModeratorProposalModel(Proposal proposal, Store store)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);

		if(proposal.Options[0].Operation is StoreModeratorAddition addition)
		{
			IEnumerable<UserModel> moderators = McvUtils.LoadUsers(mcv, addition.Candidates, CancellationToken.None);
			return new ModeratorProposalModel(proposal, creator)
			{
				Moderators = moderators,
			};
		}

		return null;
	}

	public TotalItemsResult<PublisherProposalModel> GetPublisherProposalsNotOptimized
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetPublisherProposalsNotOptimized), storeId, page, pageSize, search);
#endif

		return GetProposalsByTypeNotOptimized(storeId, page, pageSize, search, ProposalUtils.IsPublisherOperation, CreatePublisherProposalModel, cancellationToken);
	}

	PublisherProposalModel CreatePublisherProposalModel(Proposal proposal, Store store)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);

		if(proposal.Options[0].Operation is StoreAuthorsRemoval removal)
		{
			IEnumerable<AuthorBaseAvatarModel> removals = McvUtils.LoadAuthors(mcv, removal.Authors, CancellationToken.None);
			return new PublisherProposalModel(proposal, creator, removals);
		}

		return null;
	}

	public TotalItemsResult<ProposalModel> GetUserRegistrations
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetUserRegistrations), storeId, page, pageSize);
#endif

		return GetProposalsByTypeNotOptimized(storeId, page, pageSize, null, ProposalUtils.IsUserRegistrationOperation, CreateUserProposalModel, cancellationToken);
	}

	public TotalItemsResult<UserUnregistrationProposalModel> GetUserUnregistrations
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(ModeratorProposalsService), nameof(ModeratorProposalsService.GetUserUnregistrations), storeId, page, pageSize);
#endif

		return GetProposalsByTypeNotOptimized(storeId, page, pageSize, null, ProposalUtils.IsUserUnregistrationOperation, CreateUserUnregistrationModel, cancellationToken);
	}

	ProposalModel CreateUserProposalModel(Proposal proposal, Store store)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);
		return new ProposalModel(proposal, creator);
	}

	UserUnregistrationProposalModel CreateUserUnregistrationModel(Proposal proposal, Store store)
	{
		UserUnregistration operation = proposal.Options[0].Operation as UserUnregistration;
		AutoId userId = operation.User;
		FairUser userToUnregister = (FairUser) mcv.Users.Latest(userId);

		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);

		return new UserUnregistrationProposalModel(proposal, creator)
		{
			UserId = userToUnregister.Id,
			UserName = userToUnregister.Name
		};
	}
}
