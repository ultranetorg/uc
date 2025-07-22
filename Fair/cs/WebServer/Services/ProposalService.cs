using System.Runtime.CompilerServices;
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
		GetProposalOrReferendum(siteId, proposalId, true);

	public TotalItemsResult<ProposalModel> GetDiscussions(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposalsOrReferendums(siteId, true, page, pageSize, search, cancellationToken);

	public ProposalDetailsModel GetReferendum(string siteId, string proposalId) =>
		GetProposalOrReferendum(siteId, proposalId, false);

	public TotalItemsResult<ProposalModel> GetReferendums(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposalsOrReferendums(siteId, false, page, pageSize, search, cancellationToken);

	/// <param name="discussionOrReferendums">`true` for Discussion, `false` for Referendum</param>
	ProposalDetailsModel GetProposalOrReferendum(string siteId, string proposalId, bool discussionOrReferendums)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalOrReferendum)} method called with {{SiteId}}, {{ProposalId}}, {{ProposalsOrReferendums}}", siteId, proposalId, discussionOrReferendums);

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
			if (discussionOrReferendums != IsProposalIsDiscussion(site, proposal))
			{
				throw new EntityNotFoundException(entityName, proposalId);
			}

			FairAccount account = (FairAccount) mcv.Accounts.Latest(proposal.By);

			return new ProposalDetailsModel(proposal, account)
			{
				Option = ToBaseVotableOperationModel(proposal.Option)
			};
		}
	}

	TotalItemsResult<ProposalModel> GetProposalsOrReferendums(string siteId, bool discussionOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalsOrReferendums)} method called with {{SiteId}}, {{ProposalsOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, discussionOrReferendums, page, pageSize, search);

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
	TotalItemsResult<ProposalModel> LoadProposalsOrReferendumsPaged(Site site, bool discussionsOrReferendums, int page, int pageSize, string search,
		CancellationToken cancellationToken)
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

			if (discussionsOrReferendums != IsProposalIsDiscussion(site, proposal))
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
			ProposalModel model = new ProposalModel(proposal, account)
			{
				Option = ToBaseVotableOperationModel(proposal.Option)
			};
			result.Add(model);
		}

		return new TotalItemsResult<ProposalModel>
		{
			Items = result,
			TotalItems = totalItems
		};
	}

	static BaseVotableOperationModel ToBaseVotableOperationModel(VotableOperation proposal)
	{
		return proposal switch
						{
							CategoryAvatarChange operation => new CategoryAvatarChangeModel(operation),
							CategoryCreation operation => new CategoryCreationModel(operation),
							CategoryDeletion operation => new CategoryDeletionModel(operation),
							CategoryMovement operation => new CategoryMovementModel(operation),
							PublicationCreation operation => new PublicationCreationModel(operation),
							PublicationDeletion operation => new PublicationDeletionModel(operation),
							PublicationPublish operation => new PublicationPublishModel(operation),
							PublicationRemoveFromChanged operation => new PublicationRemoveFromChangedModel(operation),
							PublicationUpdation operation => new PublicationUpdationModel(operation),
							ReviewCreation operation => new ReviewCreationModel(operation),
							ReviewEditModeration operation => new ReviewEditModerationModel(operation),
							ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
							SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
							SiteAvatarChange operation => new SiteAvatarChangeModel(operation),
							SiteModeratorsChange operation => new SiteModeratorsChangeModel(operation),
							SiteNicknameChange operation => new SiteNicknameChangeModel(operation),
							SitePolicyChange operation => new SitePolicyChangeModel(operation),
							SiteTextChange operation => new SiteTextModel(operation),
							UserDeletion operation => new UserDeletionModel(operation),
							UserRegistration operation => new UserRegistrationModel(operation),
							_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
						};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsProposalIsDiscussion(Site site, Proposal proposal) =>
		site.ApprovalPolicies[Enum.Parse<FairOperationClass>(proposal.Option.GetType().Name)] != ApprovalPolicy.ElectedByAuthorsMajority;
}