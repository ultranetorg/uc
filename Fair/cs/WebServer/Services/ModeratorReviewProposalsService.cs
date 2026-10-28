using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ModeratorReviewProposalsService
(
#if DEBUG
	ILogger<ModeratorReviewProposalsService> logger,
#endif
	FairMcv mcv,
	ModeratorProposalsService moderatorProposalsService
)
{
	public TotalItemsResult<ReviewProposalModel> GetCreationsNotOptimized
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(ModeratorReviewProposalsService), nameof(GetCreationsNotOptimized), storeId, page, pageSize, search);
#endif

		return moderatorProposalsService.GetProposalsByTypeNotOptimized(storeId, page, pageSize, search, ProposalUtils.IsReviewCreationOrEditOperation, CreateReviewCreationEditProposalModel, cancellationToken);
	}

	public TotalItemsResult<ReviewProposalModel> GetDeletionsNotOptimized
		([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(ModeratorReviewProposalsService), nameof(GetDeletionsNotOptimized), storeId, page, pageSize, search);
#endif

		return moderatorProposalsService.GetProposalsByTypeNotOptimized(storeId, page, pageSize, search, ProposalUtils.IsReviewDeletionOperation, CreatereReviewDeletionModel, cancellationToken);
	}

	ReviewProposalModel CreateReviewCreationEditProposalModel(Proposal proposal, Store store)
	{
		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);

		if(proposal.Options[0].Operation is ReviewCreation reviewCreation)
		{
			Publication? publication = mcv.Publications.Latest(reviewCreation.Publication);
			return CreateReviewModel(proposal, store, creator, publication, reviewCreation.Text);
		}
		if(proposal.Options[0].Operation is ReviewEdit reviewEdit)
		{
			Review review = mcv.Reviews.Latest(reviewEdit.Review);
			Publication? publication = review != null ? mcv.Publications.Latest(review.Publication) : null;
			return CreateReviewModel(proposal, store, creator, publication, reviewEdit.Text);
		}

		throw new InvalidOperationException("Unexpected operation type for review proposal");
	}

	ReviewProposalModel CreatereReviewDeletionModel(Proposal proposal, Store store)
	{
		
		if(proposal.Options[0].Operation is ReviewDeletion reviewDeletion)
		{
			Review review = mcv.Reviews.Latest(reviewDeletion.Review);
			CreatorModel creator = McvUtils.LoadCreator(mcv, review.Creator, Role.User);
			Publication publication = mcv.Publications.Latest(review.Publication);
			return CreateReviewModel(proposal, store, creator, publication, review.Text);
		}

		throw new InvalidOperationException("Unexpected operation type for review proposal");
	}

	ReviewProposalModel CreateReviewModel(Proposal proposal, Store store, CreatorModel creator, Publication? publication, string reviewText)
	{
		PublicationImageBaseModel model = null;

		if (publication != null)
		{
			Product product = mcv.Products.Latest(publication.Product);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			model = new PublicationImageBaseModel(publication, product, category.Title, fileId);
		}

		return new ReviewProposalModel(proposal, creator, model, reviewText)
		{
			IsStalled = model == null
		};
	}
}
