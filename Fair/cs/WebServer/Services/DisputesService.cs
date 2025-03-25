using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class DisputesService
(
	ILogger<DisputesService> logger,
	FairMcv mcv
) : IDisputesService
{
	public DisputeDetailsModel GetDispute(string siteId, string disputeId, bool disputesOrReferendums)
	{
		logger.LogDebug($"GET {nameof(DisputesService)}.{nameof(DisputesService.GetDispute)} method called with {{SiteId}}, {{DisputeId}}, {{DisputesOrReferendums}}", siteId, disputeId, disputesOrReferendums);

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

			string entityName = disputesOrReferendums ? nameof(Dispute).ToLower() : nameof(DisputeFlags.Referendum).ToLower();
			if (!site.Disputes.Any(x => x == disputeEntityId))
			{
				throw new EntityNotFoundException(entityName, siteId);
			}

			Dispute dispute = mcv.Disputes.Find(disputeEntityId, mcv.LastConfirmedRound.Id);
			if (dispute == null || disputesOrReferendums == dispute.Flags.HasFlag(DisputeFlags.Referendum))
			{
				throw new EntityNotFoundException(entityName, disputeId);
			}

			return new DisputeDetailsModel(dispute);
		}
	}

	public TotalItemsResult<DisputeModel> GetDisputes(string siteId, bool disputesOrReferendums, int page, int pageSize, string? search, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(DisputesService)}.{nameof(DisputesService.GetDisputes)} method called with {{SiteId}}, {{DisputesOrReferendums}}, {{Page}}, {{PageSize}}, {{Search}}", siteId, disputesOrReferendums, page, pageSize, search);

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

			return LoadReferendumsPaged(site.Disputes, disputesOrReferendums, page, pageSize, search, cancellationToken);
		}
	}

	public TotalItemsResult<DisputeModel> LoadReferendumsPaged(IEnumerable<EntityId> disputesIds, bool disputesOrReferendums, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return TotalItemsResult<DisputeModel>.Empty;

		var disputes = new List<Dispute>(pageSize);
		int totalItems = 0;

		foreach (var disputeId in disputesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return ToTotalItemsResult(disputes, totalItems);

			Dispute dispute = mcv.Disputes.Find(disputeId, mcv.LastConfirmedRound.Id);
			if (disputesOrReferendums == !dispute.Flags.HasFlag(DisputeFlags.Referendum) && SearchUtils.IsMatch(dispute, search))
			{
				if (totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
				{
					disputes.Add(dispute);
				}

				++totalItems;
			}
		}

		return ToTotalItemsResult(disputes, totalItems);
	}

	private static TotalItemsResult<DisputeModel> ToTotalItemsResult(IList<Dispute> disputes, int totalItems)
	{
		IEnumerable<DisputeModel> items = disputes.Select(items => new DisputeModel(items));
		return new TotalItemsResult<DisputeModel>
		{
			Items = items,
			TotalItems = totalItems
		};
	}
}
