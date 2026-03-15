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
	public ProposalDetailsModel GetDiscussion([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string proposalId) =>
		GetProposalDetails(siteId, proposalId, true);

	public TotalItemsResult<ProposalModel> GetDiscussions([NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposals(siteId, true, page, pageSize, search, cancellationToken);

	public ProposalDetailsModel GetReferendum([NotEmpty][NotNull] string siteId, [NotEmpty][NotNull] string proposalId) =>
		GetProposalDetails(siteId, proposalId, false);

	public TotalItemsResult<ProposalModel> GetReferendums([NotEmpty][NotNull] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string search, CancellationToken cancellationToken) =>
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
			Site site = mcv.Sites.Latest(siteEntityId);
			
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			string entityName = discussionOrReferendums ? EntityNames.DiscussionEntityName : EntityNames.ReferendumEntityName;
			if (!site.Proposals.Any(x => x == proposalEntityId))
			{
				throw new EntityNotFoundException(entityName, siteId);
			}

			Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
			if (discussionOrReferendums != ProposalUtils.IsDiscussion(site, proposal)
				/* || ProposalUtils.IsPublicationOperation(proposal) || ProposalUtils.IsReviewOperation(proposal) || ProposalUtils.IsUserOperation(proposal) */)
			{
				throw new EntityNotFoundException(entityName, proposalId);
			}

			FairUser account = (FairUser) mcv.Users.Latest(proposal.By);

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

			//IEnumerable<AccountBaseModel> yesAccounts = LoadYesAccounts(option.Yes);
			model.Yes = option.Yes.Select(x => x.ToString());
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(option.Operation);

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

	TotalItemsResult<ProposalModel> GetProposals(string siteId, bool discussionOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposals)} method called with {{SiteId}}, {{ProposalsOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, discussionOrReferendums, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
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

	public TotalItemsResult<BaseProposalModel> GetProposals([NotEmpty][NotNull] string siteId, FairOperationClass? operationClass, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposals)} method called with {{SiteId}}, {{OperationClass}}, {{Page}}, {{PageSize}}", siteId, operationClass, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteEntityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadProposalsPagedNotOptimized(site.Proposals, operationClass, page, pageSize, cancellationToken);
		}
	}

	TotalItemsResult<BaseProposalModel> LoadProposalsPagedNotOptimized(IEnumerable<AutoId> proposalIds, FairOperationClass? operation, int page, int pageSize, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<BaseProposalModel>.Empty;

		var items = new List<BaseProposalModel>(pageSize);
		int totalItems = 0;

		foreach (var proposalId in proposalIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<BaseProposalModel> {Items = items, TotalItems = totalItems};

			Proposal proposal = mcv.Proposals.Latest(proposalId);
			if (proposal.OptionClass != operation)
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				FairUser by = (FairUser) mcv.Users.Latest(proposal.By);
				BaseProposalModel model = CreateCorrespondedModel(operation, proposal, by);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<BaseProposalModel> { Items = items, TotalItems = totalItems };
	}

	BaseProposalModel CreateCorrespondedModel(FairOperationClass? operation, Proposal proposal, FairUser by)
	{
		switch (operation)
		{
		}

		return null;
	}
}