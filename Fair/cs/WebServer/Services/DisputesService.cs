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
			if (disputesOrReferendums != IsProposalIsDispute(dispute))
			{
				throw new EntityNotFoundException(entityName, disputeId);
			}

			return new DisputeDetailsModel(dispute)
			{
				Proposal = ToBaseVotableOperationModel(dispute.Operation)
			};
		}
	}

	TotalItemsResult<DisputeModel> GetDisputesOrReferendums(string siteId, bool disputesOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(DisputesService)}.{nameof(DisputesService.GetDisputesOrReferendums)} method called with {{SiteId}}, {{DisputesOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, disputesOrReferendums, page, pageSize, search);

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

			return LoadDisputesOrReferendumsPaged(site.Proposals, disputesOrReferendums, page, pageSize, search, cancellationToken);
		}
	}

	/// <param name="disputesOrReferendums">`true` for Dispute, `false` for Referendum</param>
	TotalItemsResult<DisputeModel> LoadDisputesOrReferendumsPaged(IEnumerable<AutoId> disputesIds, bool disputesOrReferendums, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<DisputeModel>.Empty;

		var disputes = new List<Proposal>(pageSize);
		int totalItems = 0;

		foreach (var disputeId in disputesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(disputes, totalItems);

			Proposal dispute = mcv.Proposals.Latest(disputeId);

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

	static TotalItemsResult<DisputeModel> ToTotalItemsResult(IList<Proposal> disputes, int totalItems)
	{
		IEnumerable<DisputeModel> items = disputes.Select(dispute =>
			new DisputeModel(dispute)
			{
				Proposal = ToBaseVotableOperationModel(dispute.Operation)
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
			///TODO new NicknameChange operation => new NicknameChangeModel(operation),
			//PublicationApproval operation => new PublicationApprovalModel(operation),
			PublicationPublish operation => new PublicationPublishModel(operation),
			PublicationUpdation operation => new PublicationUpdationModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			ReviewEditModeration operation => new ReviewTextModerationModel(operation),
			SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
			SiteDescriptionChange operation => new SiteDescriptionChangeModel(operation),
			SiteModeratorsChange operation => new SiteModeratorsChangeModel(operation),
			SitePolicyChange operation => new SitePolicyChangeModel(operation),
			_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsProposalIsDispute(Proposal dispute) => dispute.Operation is not SitePolicyChange change || change.Policy != ChangePolicy.ElectedByAuthorsMajority;
}
