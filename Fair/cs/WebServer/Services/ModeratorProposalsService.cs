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

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsReviewOperation, CreateReviewProposalModel, cancellationToken);
	}

	ReviewProposalModel CreateReviewProposalModel(Proposal proposal, Site site)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		if(proposal.Options[0].Operation is ReviewCreation reviewCreation)
		{
			Publication? publication = mcv.Publications.Latest(reviewCreation.Publication);
			return CreateReviewModel(proposal, site, by, publication, reviewCreation.Text);
		}
		if(proposal.Options[0].Operation is ReviewEdit reviewEdit)
		{
			Review review = mcv.Reviews.Latest(reviewEdit.Review);
			Publication publication = mcv.Publications.Latest(review.Publication);
			return CreateReviewModel(proposal, site, by, publication, reviewEdit.Text);
		}

		throw new InvalidOperationException("Unexpected operation type for review proposal");
	}

	ReviewProposalModel CreateReviewModel(Proposal proposal, Site site, FairUser by, Publication? publication, string reviewText)
	{
		PublicationImageBaseModel model = null;

		if (publication != null)
		{
			Product product = mcv.Products.Latest(publication.Product);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			model = new PublicationImageBaseModel(publication, product, category.Title, fileId);
		}

		return new ReviewProposalModel(proposal, by, model, reviewText)
		{
			IsStalled = model == null
		};
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

	PublicationProposalModel CreatePublicationProposalModel(Proposal proposal, Site site)
	{
		if(proposal.Options[0].Operation is PublicationCreation publicationCreation)
		{
			return CreatePublicationModelFromProduct(proposal, site, publicationCreation.Product);
		}
		if(proposal.Options[0].Operation is PublicationDeletion publicationDeletion)
		{
			return CreatePublicationModel(proposal, site, publicationDeletion.Publication);
		}
		if(proposal.Options[0].Operation is PublicationPublish publicationPublish)
		{
			return CreatePublicationModel(proposal, site, publicationPublish.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUpdation publicationUpdation)
		{
			return CreatePublicationModel(proposal, site, publicationUpdation.Publication);
		}
		if(proposal.Options[0].Operation is PublicationUnpublish publicationUnpublish)
		{
			return CreatePublicationModel(proposal, site, publicationUnpublish.Publication);
		}

		return null;
	}

	PublicationProposalModel CreatePublicationModelFromProduct(Proposal proposal, Site site, AutoId productId)
	{
		FairUser by = (FairUser)mcv.Users.Latest(proposal.By);
		Product product = mcv.Products.Latest(productId);
		Author author = mcv.Authors.Latest(product.Author);
		AutoId? fileId = PublicationUtils.GetLatestLogo(product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(product, fileId);

		return new PublicationProposalModel(proposal, by, product, author, publicationImage);
	}

	PublicationProposalModel CreatePublicationModel(Proposal proposal, Site site, AutoId publicationId)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
		Publication publication = mcv.Publications.Latest(publicationId);
		Product product = mcv.Products.Latest(publication.Product);
		Author author = mcv.Authors.Latest(product.Author);

		AutoId? fileId = PublicationUtils.GetLogo(publication, product);

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(publication, product, null, fileId);

		return new PublicationProposalModel(proposal, by, product, author, publicationImage);
	}

	T GetProposalByType<T>(string siteId, string proposalId, string entityName, Predicate<Proposal> checkFunc, Func<Proposal, Site, T> createFunc) where T : ProposalModel
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

			return createFunc(proposal, site);
		}
	}

	TotalItemsResult<T> GetProposalsByTypeNotOptimized<T>
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, Predicate<Proposal> checkFunc, Func<Proposal, Site, T> createFunc, CancellationToken cancellationToken)
		where T : ProposalModel
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
		(Site site, int page, int pageSize, string? query, Predicate<Proposal> checkFunc, Func<Proposal, Site, T> createFunc, CancellationToken cancellationToken) where T : ProposalModel
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<T>.Empty;

		var items = new List<T>(pageSize);
		int totalItems = 0;

		IEnumerable<AutoId> reversed = site.Proposals.Reverse();
		foreach (AutoId proposalId in reversed)
		{
			if (cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<T>{Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			//if (!ProposalUtils.IsDiscussion(site, proposal))
			//{
			//	continue;
			//}
			if (!checkFunc(proposal) || !SearchUtils.IsMatch(proposal, query))
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				T model = createFunc(proposal, site);
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

	ModeratorProposalModel CreateModeratorProposalModel(Proposal proposal, Site site)
	{
		Author author = mcv.Authors.Latest(proposal.By);
		FairUser by = (FairUser) mcv.Users.Latest(author.Owners[0]); // TODO: handle multiple Authors.

		if(proposal.Options[0].Operation is SiteModeratorAddition addition)
		{
			IEnumerable<UserModel> moderators = McvUtils.LoadUsers(mcv, addition.Candidates, CancellationToken.None);
			return new ModeratorProposalModel(proposal, by)
			{
				Moderators = moderators,
			};
		}
		if(proposal.Options[0].Operation is SiteModeratorRemoval removal)
		{
			IEnumerable<UserModel> moderators = McvUtils.LoadUsers(mcv, [removal.Moderator], CancellationToken.None);
			return new ModeratorProposalModel(proposal, by)
			{
				Moderators = moderators,
			};
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

	PublisherProposalModel CreatePublisherProposalModel(Proposal proposal, Site site)
	{
		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		if(proposal.Options[0].Operation is SiteAuthorsRemoval removal)
		{
			IEnumerable<AuthorBaseAvatarModel> removals = McvUtils.LoadAuthors(mcv, removal.Authors, CancellationToken.None);
			return new PublisherProposalModel(proposal, by, removals);
		}

		return null;
	}

	public TotalItemsResult<ProposalModel> GetUserRegistrations
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(ModeratorProposalsService), nameof(GetUserRegistrations), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, null, ProposalUtils.IsUserRegistrationOperation, CreateUserProposalModel, cancellationToken);
	}

	public TotalItemsResult<UserUnregistrationProposalModel> GetUserUnregistrations
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(ModeratorProposalsService), nameof(GetUserUnregistrations), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, null, ProposalUtils.IsUserUnregistrationOperation, CreateUserUnregistrationModel, cancellationToken);
	}

	ProposalModel CreateUserProposalModel(Proposal proposal, Site site)
	{
		FairUser by = (FairUser)mcv.Users.Latest(proposal.By);
		return new ProposalModel(proposal, by);
	}

	UserUnregistrationProposalModel CreateUserUnregistrationModel(Proposal proposal, Site site)
	{
		UserUnregistration operation = proposal.Options[0].Operation as UserUnregistration;
		AutoId userId = operation.User;
		FairUser userToUnregister = (FairUser) mcv.Users.Latest(userId);

		FairUser by = (FairUser) mcv.Users.Latest(proposal.By);

		return new UserUnregistrationProposalModel(proposal, by)
		{
			UserId = userToUnregister.Id.ToString(),
			UserName = userToUnregister.Name
		};
	}
}
