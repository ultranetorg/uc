using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging.Abstractions;
using NativeImport;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ModeratorProposalsService
(
	ILogger<ModeratorProposalsService> logger,
	FairMcv mcv
)
{
	private const string UserEntityName = "user";

	public ReviewProposalDetailsModel GetReviewProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetReviewProposal)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		return GetProposalByType<ReviewProposalDetailsModel>(siteId, proposalId, nameof(Review).ToLower(), ProposalUtils.IsReviewOperation, p => CreateReviewProposalModel<ReviewProposalDetailsModel>(p));
	}

	public TotalItemsResult<ReviewProposalModel> GetReviewProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetReviewProposalsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		return GetProposalsByTypeNotOptimized<ReviewProposalModel>(siteId, page, pageSize, search, ProposalUtils.IsReviewOperation, (p) => CreateReviewProposalModel<ReviewProposalModel>(p), cancellationToken);
	}

	T CreateReviewProposalModel<T>(Proposal proposal) where T : ReviewProposalModel
	{
		FairAccount reviewer = (FairAccount) mcv.Accounts.Latest(proposal.By);

		if(proposal.Options[0].Operation is ReviewCreation reviewCreation)
		{
			Publication publication = mcv.Publications.Latest(reviewCreation.Publication);
			return CreateReviewModel<T>(proposal, reviewer, publication);
		}
		else if(proposal.Options[0].Operation is ReviewEdit reviewEdit)
		{
			Review review = mcv.Reviews.Latest(reviewEdit.Review);
			Publication publication = mcv.Publications.Latest(review.Publication);
			return CreateReviewModel<T>(proposal, reviewer, publication);
		}

		return null;
	}

	T CreateReviewModel<T>(Proposal proposal, FairAccount reviewer, Publication publication) where T : ReviewProposalModel
	{
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(publication.Category);
		AutoId? fileId = PublicationUtils.GetLogo(publication, product);
		byte[]? image = fileId != null ? mcv.Files.Latest(fileId)?.Data : null;

		PublicationImageBaseModel model = new PublicationImageBaseModel(publication, product, category.Title, image);

		T instance = (T) Activator.CreateInstance(typeof(T), proposal, reviewer, model);
		instance.Options = ProposalUtils.MapOptions(proposal.Options);
		return instance;
	}

	public UserProposalModel GetUserProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetUserProposal)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		return GetProposalByType<UserProposalModel>(siteId, proposalId, UserEntityName, ProposalUtils.IsUserOperation, CreateUserProposalModel);
	}

	public TotalItemsResult<UserProposalModel> GetUserProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetUserProposalsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsUserOperation, CreateUserProposalModel, cancellationToken);
	}

	UserProposalModel CreateUserProposalModel(Proposal proposal)
	{
		FairAccount? signer = proposal.Options.Length > 0 ? proposal.Options[0].Operation.Signer : null;
		var model = new UserProposalModel(proposal, signer);
		model.Options = ProposalUtils.MapOptions(proposal.Options);
		return model;
	}

	public PublicationProposalModel GetPublicationProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetPublicationProposal)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		return GetProposalByType<PublicationProposalModel>(siteId, proposalId, UserEntityName, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel);
	}

	public TotalItemsResult<PublicationProposalModel> GetPublicationsProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetPublicationsProposalsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		return GetProposalsByTypeNotOptimized(siteId, page, pageSize, search, ProposalUtils.IsPublicationOperation, CreatePublicationProposalModel, cancellationToken);
	}

	PublicationProposalModel CreatePublicationProposalModel(Proposal proposal)
	{
		if(proposal.Options[0].Operation is PublicationCreation publicationCreation)
		{
			return CreatePublicationModelFromProduct(proposal, publicationCreation.Product);
		}
		else if(proposal.Options[0].Operation is PublicationDeletion publicationDeletion)
		{
			return CreatePublicationModel(proposal, publicationDeletion.Publication);
		}
		else if(proposal.Options[0].Operation is PublicationPublish publicationPublish)
		{
			return CreatePublicationModel(proposal, publicationPublish.Publication);
		}
		else if(proposal.Options[0].Operation is PublicationRemoveFromChanged publicationRemoveFromChanged)
		{
			return CreatePublicationModel(proposal, publicationRemoveFromChanged.Publication);
		}
		else if(proposal.Options[0].Operation is PublicationUpdation publicationUpdation)
		{
			return CreatePublicationModel(proposal, publicationUpdation.Publication);
		}

		return null;
	}

	PublicationProposalModel CreatePublicationModel(Proposal proposal, AutoId publicationId)
	{
		Publication publication = mcv.Publications.Latest(publicationId);
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(publication.Category);
		FairAccount author = (FairAccount) mcv.Accounts.Latest(product.Author);

		AutoId? fileId = PublicationUtils.GetLogo(publication, product);
		byte[]? image = fileId != null ? mcv.Files.Latest(fileId)?.Data : null;

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(publication, product, category.Title, image);

		return new PublicationProposalModel(proposal, product, author, publicationImage)
		{
			Options = ProposalUtils.MapOptions(proposal.Options)
		};
	}

	PublicationProposalModel CreatePublicationModelFromProduct(Proposal proposal, AutoId productId)
	{
		Product product = mcv.Products.Latest(productId);
		FairAccount author = (FairAccount) mcv.Accounts.Latest(product.Author);
		AutoId? fileId = PublicationUtils.GetLatestLogo(product);
		byte[]? image = fileId != null ? mcv.Files.Latest(fileId)?.Data : null;

		PublicationImageBaseModel publicationImage = new PublicationImageBaseModel(product, image);

		return new PublicationProposalModel(proposal, product, author, publicationImage)
		{
			Options = ProposalUtils.MapOptions(proposal.Options)
		};
	}

	T GetProposalByType<T>(string siteId, string proposalId, string entityName, Predicate<Proposal> checkFunc, Func<Proposal, T> createFunc) where T : BaseProposal
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
			if (!site.Proposals.Any(x => x == proposalEntityId))
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
		where T : BaseProposal
	{
		AutoId siteEntityId = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadProposalsByType<T>(site, page, pageSize, search, checkFunc, createFunc, cancellationToken);
		}
	}

	TotalItemsResult<T> LoadProposalsByType<T>
		(Site site, int page, int pageSize, string? query, Predicate<Proposal> checkFunc, Func<Proposal, T> createFunc, CancellationToken cancellationToken) where T : BaseProposal
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<T>.Empty;

		var items = new List<T>(pageSize);
		int totalItems = 0;

		foreach (var proposalId in site.Proposals)
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
}


//public TotalItemsResult<PublicationProposalModel> GetModeratorPublicationsNotOptimized(string siteId, int page, int pageSize, string? search, CancellationToken canellationToken)
//{
//	logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublicationsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

//	Guard.Against.NullOrEmpty(siteId);
//	Guard.Against.Negative(page);
//	Guard.Against.NegativeOrZero(pageSize);

//	AutoId id = AutoId.Parse(siteId);

//	lock (mcv.Lock)
//	{
//		Site site = mcv.Sites.Latest(id);
//		if (site == null)
//		{
//			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
//		}

//		var context = new FilteredContext<PublicationProposalModel>
//		{
//			Page = page,
//			PageSize = pageSize,
//			Search = search,
//			Items = new List<PublicationProposalModel>(pageSize),
//		};

//		LoadModeratorsPendingPublications(site.UnpublishedPublications, context, canellationToken);

//		return new TotalItemsResult<PublicationProposalModel>
//		{
//			Items = context.Items,
//			TotalItems = context.TotalItems
//		};
//	}
//}

//void LoadModeratorsPendingPublications(IEnumerable<AutoId> pendingPublicationsIds, FilteredContext<PublicationProposalModel> context, CancellationToken cancellationToken)
//{
//	if (cancellationToken.IsCancellationRequested)
//		return;

//	foreach (var publicationId in pendingPublicationsIds)
//	{
//		if (cancellationToken.IsCancellationRequested)
//			return;


//		Publication publication = mcv.Publications.Latest(publicationId);

//		if (!SearchUtils.IsMatch(publication, context.Search))
//		{
//			continue;
//		}

//		if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
//		{
//			Product product = mcv.Products.Latest(publication.Product);
//			Author author = mcv.Authors.Latest(product.Author);
//			Category category = mcv.Categories.Latest(publication.Category);
//			var model = new PublicationProposalModel(publication, category, product, author);
//			context.Items.Add(model);
//		}

//		++context.TotalItems;
//	}
//}

//public PublicationProposalModel GetModeratorPublication(string publicationId)
//{
//	logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.GetModeratorPublication)} method called with {{PublicationId}}", publicationId);

//	Guard.Against.NullOrEmpty(publicationId);

//	AutoId id = AutoId.Parse(publicationId);

//	lock (mcv.Lock)
//	{
//		Publication publication = mcv.Publications.Latest(id);
//		if (publication == null)
//		{
//			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
//		}

//		Category category = mcv.Categories.Latest(publication.Category);
//		Product product = mcv.Products.Latest(publication.Product);
//		Author author = mcv.Authors.Latest(product.Author);

//		return new PublicationProposalModel(publication, category, product, author);
//	}
//}