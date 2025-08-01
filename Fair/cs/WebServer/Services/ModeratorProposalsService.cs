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
	private const string UserEntityName = "user";

	public TotalItemsResult<ReviewProposalModel> GetReviewProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetReviewProposalsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadReviewProposalsPaged(site, page, pageSize, search, cancellationToken);
		}
	}

	TotalItemsResult<ReviewProposalModel> LoadReviewProposalsPaged(Site site, int page, int pageSize, string? query, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ReviewProposalModel>.Empty;

		var items = new List<ReviewProposalModel>(pageSize);
		int totalItems = 0;

		foreach(var proposalId in site.Proposals)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<ReviewProposalModel> {Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if(!ProposalUtils.IsDiscussion(site, proposal))
			{
				continue;
			}
			if(!ProposalUtils.IsReviewOperation(proposal) || !SearchUtils.IsMatch(proposal, query))
			{
				continue;
			}

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				ReviewProposalModel model = ToModeratorReviewModel<ReviewProposalModel>(proposal);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<ReviewProposalModel> { Items = items, TotalItems = totalItems };
	}

	T ToModeratorReviewModel<T>(Proposal proposal) where T : ReviewProposalModel
	{
		FairAccount reviewer = (FairAccount)mcv.Accounts.Latest(proposal.By);

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

		T instance = (T)Activator.CreateInstance(typeof(T), proposal, reviewer, model);
		instance.Options = MapOptions(proposal.Options);
		return instance;
	}

	IEnumerable<ProposalOptionModel> MapOptions(ProposalOption[] options)
	{
		IList<ProposalOptionModel> result = new List<ProposalOptionModel>(options.Length);

		foreach(ProposalOption option in options)
		{
			ProposalOptionModel model = new(option);
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(option.Operation);

			result.Add(model);
		}

		return result;
	}

	public ReviewProposalDetailsModel GetReviewProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetReviewProposal)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(!site.Proposals.Any(x => x == proposalEntityId))
			{
				throw new EntityNotFoundException(nameof(Review), proposalId);
			}

			Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
			if(!ProposalUtils.IsDiscussion(site, proposal) || !ProposalUtils.IsReviewOperation(proposal))
			{
				throw new EntityNotFoundException(nameof(Review), proposalId);
			}

			return ToModeratorReviewModel<ReviewProposalDetailsModel>(proposal);
		}
	}

	public TotalItemsResult<UserProposalModel> GetUserProposalsNotOptimized
		([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetUserProposalsNotOptimized)} method called with {{SiteId}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadUserProposalsPaged(site, page, pageSize, search, cancellationToken);
		}
	}

	TotalItemsResult<UserProposalModel> LoadUserProposalsPaged(Site site, int page, int pageSize, string? query, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<UserProposalModel>.Empty;

		var items = new List<UserProposalModel>(pageSize);
		int totalItems = 0;

		foreach(var proposalId in site.Proposals)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<UserProposalModel> {Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if(!ProposalUtils.IsDiscussion(site, proposal))
			{
				continue;
			}
			if(!ProposalUtils.IsUserOperation(proposal) || !SearchUtils.IsMatch(proposal, query))
			{
				continue;
			}

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				UserProposalModel model = new(proposal);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<UserProposalModel> {Items = items, TotalItems = totalItems};
	}

	public UserProposalModel GetUserProposal([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string proposalId)
	{
		logger.LogDebug($"GET {nameof(ModeratorProposalsService)}.{nameof(ModeratorProposalsService.GetUserProposal)} method called with {{SiteId}}, {{ProposalId}}", siteId, proposalId);

		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(!site.Proposals.Any(x => x == proposalEntityId))
			{
				throw new EntityNotFoundException(UserEntityName, proposalId);
			}

			Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
			if(!ProposalUtils.IsDiscussion(site, proposal) || !ProposalUtils.IsUserOperation(proposal))
			{
				throw new EntityNotFoundException(UserEntityName, proposalId);
			}

			return new UserProposalModel(proposal);
		}
	}
}
