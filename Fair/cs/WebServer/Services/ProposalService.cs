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

		CreatorModel creator = McvUtils.LoadCreator(mcv, proposal);

		IEnumerable<ProposalOptionModel> options = LoadOptions(proposal);

		Policy proposalPolicy = store.Policies.FirstOrDefault(p => p.OperationClass == proposal.OptionClass);
		int votesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinProposal(proposalPolicy.Approval, store);

		return new ProposalDetailsModel(proposal, creator, votesRequiredToWin)
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
}