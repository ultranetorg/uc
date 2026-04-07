using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ModeratorProposalsService
(
	ILogger<ModeratorProposalsService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<ReviewProposalModel> GetReviewProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}", nameof(ModeratorProposalsService), nameof(GetReviewProposalsNotOptimized), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsReviewOperation, CreateReviewProposalModel<ReviewProposalModel>, cancellationToken);
	}

	T CreateReviewProposalModel<T>(Proposal proposal) where T : ReviewProposalModel
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		if(proposal.Options[0].Operation is ReviewCreation reviewCreation)
		{
			Publication publication = mcv.Publications.Latest(reviewCreation.Publication);
			return CreateReviewModel<T>(proposal, by, publication, reviewCreation.Text);
		}
		if(proposal.Options[0].Operation is ReviewEdit reviewEdit)
		{
			Review review = mcv.Reviews.Latest(reviewEdit.Review);
			Publication publication = mcv.Publications.Latest(review.Publication);
			return CreateReviewModel<T>(proposal, by, publication, reviewEdit.Text);
		}

		return null;
	}

	T CreateReviewModel<T>(Proposal proposal, FairUser by, Publication publication, string reviewText) where T : ReviewProposalModel
	{
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(publication.Category);
		AutoId? fileId = PublicationUtils.GetLogo(publication, product);

		PublicationImageBaseModel model = new PublicationImageBaseModel(publication, product, category.Title, fileId);

		T instance = (T) Activator.CreateInstance(typeof(T), proposal, by, model, reviewText);
		return instance;
	}

	public ProposalModel GetUserProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ProposalId}", nameof(ModeratorProposalsService), nameof(GetUserProposal), siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		return GetProposalByType<ProposalModel>(siteId, proposalId, EntityNames.UserEntityName, ProposalUtils.IsUserOperation, CreateUserProposalModel);
	}

	public TotalItemsResult<ProposalModel> GetUserProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}",
			nameof(ModeratorProposalsService), nameof(GetUserProposalsNotOptimized), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsUserOperation, CreateUserProposalModel, cancellationToken);
	}

	ProposalModel CreateUserProposalModel(Proposal proposal)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
		return new ProposalModel(proposal, by);
	}

	public PublicationProposalModel GetPublicationProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ProposalId}", nameof(ModeratorProposalsService), nameof(GetPublicationProposal), siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		return GetProposalByType<PublicationProposalModel>(siteId, proposalId, EntityNames.UserEntityName, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel);
	}

	public TotalItemsResult<PublicationProposalModel> GetPublicationsProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}",
			nameof(ModeratorProposalsService), nameof(GetPublicationsProposalsNotOptimized), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel, cancellationToken);
	}

	PublicationProposalModel CreatePublicationProposalModel(Proposal proposal)
	{
		if(proposal.Options[0].Operation is PublicationCreation publicationCreation)
		{
			return CreatePublicationModelFromProduct(proposal, publicationCreation.Product);
		}
		if(proposal.Options[0].Operation is PublicationDeletion publicationDeletion)
		{
			return CreatePublicationModel(proposal, publicationDeletion.Publication);
		}
		if(proposal.Options[0].Operation is PublicationPublish publicationPublish)
		{
			return CreatePublicationModel(proposal, publicationPublish.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUpdation publicationUpdation)
		{
			return CreatePublicationModel(proposal, publicationUpdation.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUnpublish publicationUnpublish)
		{
			return CreatePublicationModel(proposal, publicationUnpublish.Publication);
		}

		return null;
	}

	PublicationProposalModel CreatePublicationModel(Proposal proposal, AutoId publicationId)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
		Publication publication = mcv.Publications.Latest(publicationId);
		Product product = mcv.Products.Latest(publication.Product);
		FairUser author = (FairUser) mcv.Users.Latest(product.Author);

		AutoId? fileId = PublicationUtils.GetLogo(publication, product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(publication, product, null, fileId);

		return new PublicationProposalModel(proposal, by, product, author, publicationImage);
	}

	PublicationProposalModel CreatePublicationModelFromProduct(Proposal proposal, AutoId productId)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
		Product product = mcv.Products.Latest(productId);
		FairUser author = (FairUser) mcv.Users.Latest(product.Author);
		AutoId? fileId = PublicationUtils.GetLatestLogo(product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(product, fileId);

		return new PublicationProposalModel(proposal, by, product, author, publicationImage);
	}

	T GetProposalByType<T>(string siteId, string proposalId, string entityName, Predicate<Proposal> checkFunc, Func<Proposal, T> createFunc) where T : BaseProposalModel
	{
		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if (site.Proposals.All(x => x != proposalEntityId))
			{
				throw new EntityNotFoundException(entityName, proposalId);
			}

			Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
			if (!ProposalUtils.IsDiscussion(site, proposal) || !checkFunc(proposal))
			{
				throw new EntityNotFoundException(entityName, proposalId);
			}

			return createFunc(proposal);
		}
	}

	TotalItemsResult<T> GetProposalsByTypeNotOptimized<T>
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, Predicate<Proposal> checkFunc, Func<Proposal, T> createFunc, CancellationToken cancellationToken)
		where T : BaseProposalModel
	{
		AutoId siteEntityId = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadProposalsByType<T>(site, page, pageSize, search, checkFunc, createFunc, cancellationToken);
		}
	}

	TotalItemsResult<T> LoadProposalsByType<T>
		(Site site, int page, int pageSize, string? query, Predicate<Proposal> checkFunc, Func<Proposal, T> createFunc, CancellationToken cancellationToken) where T : BaseProposalModel
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<T>.Empty;

		var items = new List<T>(pageSize);
		int totalItems = 0;

		foreach (AutoId proposalId in site.Proposals)
		{
			if (cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<T>{Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if (!ProposalUtils.IsDiscussion(site, proposal))
			{
				continue;
			}
			if (!checkFunc(proposal) || !SearchUtils.IsMatch(proposal, query))
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				T model = createFunc(proposal);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<T>{Items = items, TotalItems = totalItems};
	}

	public TotalItemsResult<ModeratorProposalModel> GetModeratorProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}", nameof(ModeratorProposalsService), nameof(GetModeratorProposalsNotOptimized), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsModeratorOperation, CreateModeratorProposalModel, cancellationToken);
	}

	ModeratorProposalModel CreateModeratorProposalModel(Proposal proposal)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		if(proposal.Options[0].Operation is SiteModeratorAddition addition)
		{
			// NOTE: if there are multiple options, we won't load moderators.
			IEnumerable<AccountBaseModel> moderators = proposal.Options.Length == 1 ? McvUtils.LoadAccounts(mcv, addition.Candidates, CancellationToken.None) : null;
			return new ModeratorProposalModel(proposal, by, moderators);
		}
		if(proposal.Options[0].Operation is SiteModeratorRemoval removal)
		{
			// NOTE: if there are multiple options, we won't load moderators.
			IEnumerable<AccountBaseModel> moderators = proposal.Options.Length == 1 ? McvUtils.LoadAccounts(mcv, [removal.Moderator], CancellationToken.None) : null;
			return new ModeratorProposalModel(proposal, by, moderators);
		}

		return null;
	}

	public TotalItemsResult<PublisherProposalModel> GetPublisherProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}, {Search}", nameof(ModeratorProposalsService), nameof(GetPublisherProposalsNotOptimized), siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsPublisherOperation, CreatePublisherProposalModel, cancellationToken);
	}

	PublisherProposalModel CreatePublisherProposalModel(Proposal proposal)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		if(proposal.Options[0].Operation is SiteAuthorsRemoval removal)
		{
			// NOTE: if there are multiple options, we won't load publishers.
			IEnumerable<AccountBaseModel> removals = proposal.Options.Length == 1 ? McvUtils.LoadAccounts(mcv, removal.Authors, CancellationToken.None) : null;
			return new PublisherProposalModel(proposal, by, removals);
		}

		return null;
	}
}
