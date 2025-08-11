using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProposalService
(
	ILogger<ProposalService> logger,
	FairMcv mcv
) : IProposalService
{
	private const string ReferendumEntityName = "referendum";

	public ProposalDetailsModel GetDiscussion(string siteId, string proposalId) =>
		GetProposalDetails(siteId, proposalId, true);

	public TotalItemsResult<ProposalModel> GetDiscussions(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(siteId, true, page, pageSize, search, cancellationToken);

	public ProposalDetailsModel GetReferendum(string siteId, string proposalId) =>
		GetProposalDetails(siteId, proposalId, false);

	public TotalItemsResult<ProposalModel> GetReferendums(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(siteId, false, page, pageSize, search, cancellationToken);

	/// <param name="discussionOrReferendums">`true` for Discussion, `false` for Referendum</param>
	ProposalDetailsModel GetProposalDetails(string siteId, string proposalId, bool discussionOrReferendums)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalDetails)} method called with {{SiteId}}, {{ProposalId}}, {{ProposalsOrReferendums}}", siteId, proposalId, discussionOrReferendums);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(proposalId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			string entityName = discussionOrReferendums ? nameof(Proposal).ToLower() : ReferendumEntityName;
			if (!site.Proposals.Any(x => x == proposalEntityId))
			{
				throw new EntityNotFoundException(entityName, siteId);
			}

			Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
			if (discussionOrReferendums != ProposalUtils.IsDiscussion(site, proposal) ||
				ProposalUtils.IsPublicationOperation(proposal) || ProposalUtils.IsReviewOperation(proposal) || ProposalUtils.IsUserOperation(proposal))
			{
				throw new EntityNotFoundException(entityName, proposalId);
			}

			FairAccount account = (FairAccount) mcv.Accounts.Latest(proposal.By);

			IEnumerable<ProposalOptionModel> options = LoadOptions(proposal);
			return new ProposalDetailsModel(proposal, account)
			{
				Options = options
			};
		}
	}

	IEnumerable<ProposalOptionModel> LoadOptions(Proposal proposal)
	{
		IList<ProposalOptionModel> result = new List<ProposalOptionModel>(proposal.Options.Length);

		foreach (ProposalOption option in proposal.Options)
		{
			ProposalOptionModel model = new(option);

			IEnumerable<AccountBaseModel> yesAccounts = LoadYesAccounts(option.Yes);
			model.YesAccounts = yesAccounts;
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(option.Operation);

			result.Add(model);
		}

		return result;
	}

	IEnumerable<AccountBaseModel> LoadYesAccounts(AutoId[] accountsIds)
	{
		lock(mcv.Lock)
		{
			return accountsIds.Select(id =>
			{
				FairAccount account = (FairAccount) mcv.Accounts.Latest(id);
				return new AccountBaseModel(account);
			}).ToArray();
		}
	}

	TotalItemsResult<ProposalModel> GetProposals(string siteId, bool discussionOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposals)} method called with {{SiteId}}, {{ProposalsOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, discussionOrReferendums, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadProposalsOrReferendumsPaged(site, discussionOrReferendums, page, pageSize, search, cancellationToken);
		}
	}

	/// <param name="discussionsOrReferendums">`true` for Proposal, `false` for Referendum</param>
	TotalItemsResult<ProposalModel> LoadProposalsOrReferendumsPaged(Site site, bool discussionsOrReferendums, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProposalModel>.Empty;

		var proposals = new List<Proposal>(pageSize);
		int totalItems = 0;

		foreach (var proposalId in site.Proposals)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(proposals, totalItems);

			Proposal proposal = mcv.Proposals.Latest(proposalId);

			if (discussionsOrReferendums != ProposalUtils.IsDiscussion(site, proposal) ||
				ProposalUtils.IsReviewOperation(proposal) || ProposalUtils.IsPublicationOperation(proposal) || ProposalUtils.IsUserOperation(proposal))
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
			FairAccount account = (FairAccount) mcv.Accounts.Latest(proposal.By);
			ProposalModel model = new ProposalModel(proposal, account); ;
			result.Add(model);
		}

		return new TotalItemsResult<ProposalModel>
		{
			Items = result,
			TotalItems = totalItems
		};
	}
}