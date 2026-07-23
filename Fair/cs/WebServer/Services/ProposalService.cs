using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProposalService
(
	ILogger<ProposalService> logger,
	FairMcv mcv
)
{
	public ProposalDetailsModel GetDiscussion([NotEmpty][NotNull] string storeId, [NotEmpty][NotNull] string proposalId) =>
		GetProposalDetails(storeId, proposalId, true);

	public TotalItemsResult<ProposalModel> GetDiscussions([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(storeId, true, page, pageSize, search, cancellationToken);

	public ProposalDetailsModel GetReferendum([NotEmpty][NotNull] string storeId, [NotEmpty][NotNull] string proposalId) =>
		GetProposalDetails(storeId, proposalId, false);

	public TotalItemsResult<ProposalModel> GetReferendums([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(storeId, false, page, pageSize, search, cancellationToken);

	/// <param name="discussionOrReferendums">`true` for Discussion, `false` for Referendum</param>
	ProposalDetailsModel GetProposalDetails(string storeId, string proposalId, bool discussionOrReferendums)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalDetails)} method called with {{StoreId}}, {{ProposalId}}, {{ProposalsOrReferendums}}", storeId, proposalId, discussionOrReferendums);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(proposalId);

		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		string entityName = discussionOrReferendums ? EntityNames.DiscussionEntityName : EntityNames.ReferendumEntityName;
		if (!store.Proposals.Any(x => x == proposalEntityId))
		{
			throw new EntityNotFoundException(entityName, storeId);
		}

		Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
		if (discussionOrReferendums != ProposalUtils.IsDiscussion(store, proposal)
			/* || ProposalUtils.IsPublicationOperation(proposal) || ProposalUtils.IsReviewOperation(proposal) || ProposalUtils.IsUserOperation(proposal) */)
		{
			throw new EntityNotFoundException(entityName, proposalId);
		}

		FairUser account = (FairUser) mcv.Users.Latest(proposal.By);

		IEnumerable<ProposalOptionModel> options = LoadOptions(proposal);

		Policy proposalPolicy = store.Policies.FirstOrDefault(p => p.OperationClass == proposal.OptionClass);
		int votesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinProposal(proposalPolicy.Approval, store);

		return new ProposalDetailsModel(proposal, account, votesRequiredToWin)
		{
			Options = options
		};
	}

	//bool won(AutoId[] votes)
	//{
	//	return policy.Approval switch
	//	{
	//		ApprovalRequirement.AnyModerator => votes.Length + p.Any.Length == 1,
	//		ApprovalRequirement.ModeratorsMajority => votes.Length + p.Any.Length >= s.Moderators.Length / 2 + (s.Moderators.Length & 1),
	//		ApprovalRequirement.AllModerators => votes.Length + p.Any.Length == s.Moderators.Length,
	//		ApprovalRequirement.PublishersMajority => votes.Length + p.Any.Length >= s.Publishers.Length / 2 + (s.Publishers.Length & 1),
	//		_ => throw new IntegrityException()
	//	};
	//}

	IEnumerable<ProposalOptionModel> LoadOptions(Proposal proposal)
	{
		IList<ProposalOptionModel> result = new List<ProposalOptionModel>(proposal.Options.Length);

		foreach (ProposalOption option in proposal.Options)
		{
			ProposalOptionModel model = new(option);

			//IEnumerable<AccountBaseModel> yesAccounts = LoadYesAccounts(option.Yes);
			model.Yes = option.Yes.Select(x => x.ToString());
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(mcv, option.Operation);

			result.Add(model);
		}

		return result;
	}

	//IEnumerable<AccountBaseModel> LoadYesAccounts(AutoId[] accountsIds)
	//{
	//	lock(mcv.Lock)
	//	{
	//		return accountsIds.Select(id =>
	//		{
	//			FairUser account = (FairUser) mcv.Users.Latest(id);
	//			return new AccountBaseModel(account);
	//		}).ToArray();
	//	}
	//}

	TotalItemsResult<ProposalModel> GetProposals(string storeId, bool discussionOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposals)} method called with {{StoreId}}, {{ProposalsOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", storeId, discussionOrReferendums, page, pageSize, search);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId storeEntityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadProposalsOrReferendumsPaged(store, discussionOrReferendums, page, pageSize, search, cancellationToken);
	}

	/// <param name="discussionsOrReferendums">`true` for Proposal, `false` for Referendum</param>
	TotalItemsResult<ProposalModel> LoadProposalsOrReferendumsPaged(Store store, bool discussionsOrReferendums, int page, int pageSize, string search, CancellationToken cancellationToken)
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

			if (discussionsOrReferendums != ProposalUtils.IsDiscussion(store, proposal) ||
				ProposalUtils.IsReviewOperation(proposal) || ProposalUtils.IsPublicationOperation(proposal) || ProposalUtils.IsUserRegistrationOperation(proposal) ||
				ProposalUtils.IsUserUnregistrationOperation(proposal) || ProposalUtils.IsModeratorOperation(proposal) || ProposalUtils.IsPublisherOperation(proposal))
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

	public TotalItemsResult<ProposalModel> GetProposals([NotEmpty][NotNull] string storeId, FairOperationClass? operationClass, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposals)} method called with {{StoreId}}, {{OperationClass}}, {{Page}}, {{PageSize}}", storeId, operationClass, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId storeEntityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadProposalsPagedNotOptimized(store.Proposals, operationClass, page, pageSize, cancellationToken);
	}

	TotalItemsResult<ProposalModel> LoadProposalsPagedNotOptimized(IEnumerable<AutoId> proposalIds, FairOperationClass? operation, int page, int pageSize, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProposalModel>.Empty;

		var items = new List<ProposalModel>(pageSize);
		int totalItems = 0;

		foreach (var proposalId in proposalIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<ProposalModel> {Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if (proposal.OptionClass != operation)
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
				ProposalModel model = CreateCorrespondedModel(operation, proposal, by);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<ProposalModel> { Items = items, TotalItems = totalItems };
	}

	ProposalModel CreateCorrespondedModel(FairOperationClass? operation, Proposal proposal, FairUser by)
	{
		return null;
	}
}