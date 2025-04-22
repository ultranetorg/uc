using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class DisputesService
(
	ILogger<DisputesService> logger,
	FairMcv mcv
) : IDisputesService
{
	private const string ReferendumEntityName = "referendum";

	public DisputeDetailsModel GetDispute(string siteId, string disputeId) =>
		GetDisputeOrReferendum(siteId, disputeId, true);

	public TotalItemsResult<DisputeModel> GetDisputes(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetDisputesOrReferendums(siteId, true, page, pageSize, search, cancellationToken);

	public DisputeDetailsModel GetReferendum(string siteId, string disputeId) =>
		GetDisputeOrReferendum(siteId, disputeId, false);

	public TotalItemsResult<DisputeModel> GetReferendums(string siteId, int page, int pageSize, string search, CancellationToken cancellationToken) =>
		GetDisputesOrReferendums(siteId, false, page, pageSize, search, cancellationToken);

	/// <param name="disputesOrReferendums">`true` for Dispute, `false` for Referendum</param>
	DisputeDetailsModel GetDisputeOrReferendum(string siteId, string disputeId, bool disputesOrReferendums)
	{
		logger.LogDebug($"GET {nameof(DisputesService)}.{nameof(DisputesService.GetDisputeOrReferendum)} method called with {{SiteId}}, {{DisputeId}}, {{DisputesOrReferendums}}", siteId, disputeId, disputesOrReferendums);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(disputeId);

		EntityId siteEntityId = EntityId.Parse(siteId);
		EntityId disputeEntityId = EntityId.Parse(disputeId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			string entityName = disputesOrReferendums ? nameof(Dispute).ToLower() : ReferendumEntityName;
			if (!site.Disputes.Any(x => x == disputeEntityId))
			{
				throw new EntityNotFoundException(entityName, siteId);
			}

			Dispute dispute = mcv.Disputes.Latest(disputeEntityId);
			if (disputesOrReferendums != IsProposalIsDispute(dispute))
			{
				throw new EntityNotFoundException(entityName, disputeId);
			}

			return new DisputeDetailsModel(dispute)
			{
				Proposal = ToBaseVotableOperationModel(dispute.Proposal)
			};
		}
	}

	TotalItemsResult<DisputeModel> GetDisputesOrReferendums(string siteId, bool disputesOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(DisputesService)}.{nameof(DisputesService.GetDisputesOrReferendums)} method called with {{SiteId}}, {{DisputesOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, disputesOrReferendums, page, pageSize, search);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		EntityId siteEntityId = EntityId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Find(siteEntityId, mcv.LastConfirmedRound.Id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadDisputesOrReferendumsPaged(site.Disputes, disputesOrReferendums, page, pageSize, search, cancellationToken);
		}
	}

	/// <param name="disputesOrReferendums">`true` for Dispute, `false` for Referendum</param>
	TotalItemsResult<DisputeModel> LoadDisputesOrReferendumsPaged(IEnumerable<EntityId> disputesIds, bool disputesOrReferendums, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<DisputeModel>.Empty;

		var disputes = new List<Dispute>(pageSize);
		int totalItems = 0;

		foreach (var disputeId in disputesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(disputes, totalItems);

			Dispute dispute = mcv.Disputes.Latest(disputeId);

			if (disputesOrReferendums != IsProposalIsDispute(dispute))
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

	static TotalItemsResult<DisputeModel> ToTotalItemsResult(IList<Dispute> disputes, int totalItems)
	{
		IEnumerable<DisputeModel> items = disputes.Select(dispute =>
			new DisputeModel(dispute)
			{
				Proposal = ToBaseVotableOperationModel(dispute.Proposal)
			});

		return new TotalItemsResult<DisputeModel>
		{
			Items = items,
			TotalItems = totalItems
		};
	}

	static BaseVotableOperationModel ToBaseVotableOperationModel(VotableOperation proposal)
	{
		return proposal switch
		{
			NicknameChange operation => new NicknameChangeModel(operation),
			PublicationProductChange operation => new PublicationProductChangeModel(operation),
			PublicationStatusChange operation => new PublicationStatusChangeModel(operation),
			PublicationUpdateModeration operation => new PublicationUpdateModerationModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			ReviewTextModeration operation => new ReviewTextModerationModel(operation),
			SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
			SiteDescriptionChange operation => new SiteDescriptionChangeModel(operation),
			SiteModeratorsChange operation => new SiteModeratorsChangeModel(operation),
			SitePolicyChange operation => new SitePolicyChangeModel(operation),
			_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsProposalIsDispute(Dispute dispute) => dispute.Proposal is not SitePolicyChange change || change.Policy != ChangePolicy.ElectedByAuthorsMajority;
}
