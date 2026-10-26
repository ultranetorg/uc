using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProposalService
(
#if DEBUG
	ILogger<ProposalService> logger,
#endif
	FairMcv mcv
)
{
	/// <param name="discussionOrReferendums">`true` for Discussion, `false` for Referendum</param>
	public ProposalDetailsModel GetProposalDetails([NotEmpty][NotNull] string storeId, [NotEmpty][NotNull] string proposalId)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(proposalId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ProposalId}", nameof(ProposalService), nameof(GetProposalDetails), storeId, proposalId);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
		if(proposal == null || !store.Proposals.Any(x => x == proposalEntityId))
		{
			throw new EntityNotFoundException(nameof(Proposal).ToLower(), storeId);
		}

		FairUser account = (FairUser)mcv.Users.Latest(proposal.By);

		IEnumerable<ProposalOptionModel> options = LoadOptions(proposal);

		Policy proposalPolicy = store.Policies.FirstOrDefault(p => p.OperationClass == proposal.OptionClass);
		int votesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinProposal(proposalPolicy.Approval, store);

		return new ProposalDetailsModel(proposal, account, votesRequiredToWin)
		{
			Options = options
		};
	}

	IEnumerable<ProposalOptionModel> LoadOptions(Proposal proposal)
	{
		IList<ProposalOptionModel> result = new List<ProposalOptionModel>(proposal.Options.Length);

		foreach(ProposalOption option in proposal.Options)
		{
			ProposalOptionModel model = new(option);

			model.Yes = option.Yes;
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(mcv, option.Operation);

			result.Add(model);
		}

		return result;
	}

	public TotalItemsResult<ProposalModel> GetProposals([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(storeId, true, page, pageSize, search, cancellationToken);

	public TotalItemsResult<ProposalModel> GetReferendums([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(storeId, false, page, pageSize, search, cancellationToken);

	TotalItemsResult<ProposalModel> GetProposals(string storeId, bool isDiscussion, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {IsDiscussion}, {Page}, {PageSize}, {Search}", nameof(ProposalService), nameof(ProposalService.GetProposals), storeId, isDiscussion, page, pageSize, search);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadProposalsOrReferendumsPaged(store, isDiscussion, page, pageSize, search, cancellationToken);
	}

	/// <param name="isDiscussion">`true` for Proposal, `false` for Referendum</param>
	TotalItemsResult<ProposalModel> LoadProposalsOrReferendumsPaged(Store store, bool isDiscussion, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProposalModel>.Empty;

		var proposals = new List<Proposal>(pageSize);
		int totalItems = 0;

		IEnumerable<AutoId> reversedIds = store.Proposals.Reverse();
		foreach (var proposalId in reversedIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(proposals, totalItems);

			Proposal proposal = mcv.Proposals.Latest(proposalId);

			if (isDiscussion != ProposalUtils.IsDiscussion(store, proposal) ||
				ProposalUtils.IsReviewCreationOrEditOperation(proposal) || ProposalUtils.IsReviewDeletionOperation(proposal) ||
				ProposalUtils.IsPublicationOperation(proposal) ||
				ProposalUtils.IsUserRegistrationOperation(proposal) || ProposalUtils.IsUserUnregistrationOperation(proposal) ||
				ProposalUtils.IsModeratorOperation(proposal) ||
				ProposalUtils.IsPublisherOperation(proposal))
			{
				continue;
			}

			if (!SearchUtils.IsMatch(proposal, search))
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				proposals.Add(proposal);
			}

			++totalItems;
		}

		return ToTotalItemsResult(proposals, totalItems);
	}

	TotalItemsResult<ProposalModel> ToTotalItemsResult(IList<Proposal> proposals, int totalItems)
	{
		IList<ProposalModel> result = new List<ProposalModel>(proposals.Count);
		foreach(Proposal proposal in proposals)
		{
			FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
			ProposalModel model = new ProposalModel(proposal, by);
			result.Add(model);
		}

		return new TotalItemsResult<ProposalModel>
		{
			Items = result,
			TotalItems = totalItems
		};
	}

//	public TotalItemsResult<ProposalModel> GetProposals([NotEmpty][NotNull] string storeId, FairOperationClass? operationClass, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
//	{
//		ArgumentException.ThrowIfNullOrEmpty(storeId);
//		ArgumentOutOfRangeException.ThrowIfNegative(page);
//		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

//#if DEBUG
//		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {OperationClass}, {Page}, {PageSize}", nameof(ProposalService), nameof(ProposalService.GetProposals), storeId, operationClass, page, pageSize);
//#endif

//		AutoId storeEntityId = AutoId.Parse(storeId);

//		Store store = mcv.Stores.Latest(storeEntityId);
//		if(store == null)
//		{
//			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
//		}

//		return LoadProposalsPagedNotOptimized(store.Proposals, operationClass, page, pageSize, cancellationToken);
//	}

//	TotalItemsResult<ProposalModel> LoadProposalsPagedNotOptimized(IEnumerable<AutoId> proposalIds, FairOperationClass? operation, int page, int pageSize, CancellationToken cancellationToken)
//	{
//		if (cancellationToken.IsCancellationRequested)
//			return TotalItemsResult<ProposalModel>.Empty;

//		var items = new List<ProposalModel>(pageSize);
//		int totalItems = 0;

//		foreach (var proposalId in proposalIds)
//		{
//			if(cancellationToken.IsCancellationRequested)
//				return new TotalItemsResult<ProposalModel> {Items = items, TotalItems = totalItems};

//			Proposal proposal = mcv.Proposals.Latest(proposalId);
//			if (proposal.OptionClass != operation)
//			{
//				continue;
//			}

//			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
//			{
//				FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
//				ProposalModel model = CreateCorrespondedModel(operation, proposal, by);
//				items.Add(model);
//			}

//			++totalItems;
//		}

//		return new TotalItemsResult<ProposalModel> { Items = items, TotalItems = totalItems };
//	}

//	ProposalModel CreateCorrespondedModel(FairOperationClass? operation, Proposal proposal, FairUser by)
//	{
//		return null;
//	}
}