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

	public ProposalDetailsModel GetProposal(string siteId, string disputeId) =>
		GetProposalOrReferendum(siteId, disputeId, true);

	public TotalItemsResult<ProposalModel> GetProposals(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposalsOrReferendums(siteId, true, page, pageSize, search, cancellationToken);

	public ProposalDetailsModel GetReferendum(string siteId, string disputeId) =>
		GetProposalOrReferendum(siteId, disputeId, false);

	public TotalItemsResult<ProposalModel> GetReferendums(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetProposalsOrReferendums(siteId, false, page, pageSize, search, cancellationToken);

	/// <param name="disputesOrReferendums">`true` for Proposal, `false` for Referendum</param>
	ProposalDetailsModel GetProposalOrReferendum(string siteId, string disputeId, bool disputesOrReferendums)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalOrReferendum)} method called with {{SiteId}}, {{ProposalId}}, {{ProposalsOrReferendums}}", siteId, disputeId, disputesOrReferendums);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(disputeId);

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId disputeEntityId = AutoId.Parse(disputeId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			string entityName = disputesOrReferendums ? nameof(Proposal).ToLower() : ReferendumEntityName;
			if (!site.Proposals.Any(x => x == disputeEntityId))
			{
				throw new EntityNotFoundException(entityName, siteId);
			}

			Proposal dispute = mcv.Proposals.Latest(disputeEntityId);
			if (disputesOrReferendums != IsProposalIsProposal(site, dispute))
			{
				throw new EntityNotFoundException(entityName, disputeId);
			}

			return new ProposalDetailsModel(dispute)
			{
				Option = ToBaseVotableOperationModel(dispute.Operation)
			};
		}
	}

	TotalItemsResult<ProposalModel> GetProposalsOrReferendums(string siteId, bool disputesOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalService)}.{nameof(ProposalService.GetProposalsOrReferendums)} method called with {{SiteId}}, {{ProposalsOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, disputesOrReferendums, page, pageSize, search);

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

			return LoadProposalsOrReferendumsPaged(site, disputesOrReferendums, page, pageSize, search, cancellationToken);
		}
	}

	/// <param name="disputesOrReferendums">`true` for Proposal, `false` for Referendum</param>
	TotalItemsResult<ProposalModel> LoadProposalsOrReferendumsPaged(Site site, bool disputesOrReferendums, int page, int pageSize, string search,
		CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProposalModel>.Empty;

		var disputes = new List<Proposal>(pageSize);
		int totalItems = 0;

		foreach (var disputeId in site.Proposals)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(disputes, totalItems);

			Proposal dispute = mcv.Proposals.Latest(disputeId);

			if (disputesOrReferendums != IsProposalIsProposal(site, dispute))
			{
				continue;
			}

			if (!SearchUtils.IsMatch(dispute, search))
			{
				continue;
			}

			if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				disputes.Add(dispute);
			}

			++totalItems;
		}

		return ToTotalItemsResult(disputes, totalItems);
	}

	static TotalItemsResult<ProposalModel> ToTotalItemsResult(IList<Proposal> disputes, int totalItems)
	{
		IEnumerable<ProposalModel> items = disputes.Select(dispute =>
			new ProposalModel(dispute)
			{
				Option = ToBaseVotableOperationModel(dispute.Operation)
			});

		return new TotalItemsResult<ProposalModel>
		{
			Items = items,
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
							ReviewEditModeration operation => new ReviewEditModerationModel(operation),
							ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
							SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
							SiteAvatarChange operation => new SiteAvatarChangeModel(operation),
							SiteDescriptionChange operation => new SiteDescriptionChangeModel(operation),
							SiteModeratorsChange operation => new SiteModeratorsChangeModel(operation),
							SiteNicknameChange operation => new SiteNicknameChangeModel(operation),
							SitePolicyChange operation => new SitePolicyChangeModel(operation),
							_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
						};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsProposalIsProposal(Site site, Proposal dispute) =>
		site.ChangePolicies[Enum.Parse<FairOperationClass>(dispute.Operation.GetType().Name)] != ChangePolicy.ElectedByAuthorsMajority;
}