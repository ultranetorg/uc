using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class DisputeCommentsService
(
	ILogger<DisputeCommentsService> logger,
	FairMcv mcv
) : IProposalCommentsService
{
	public TotalItemsResult<ProposalCommentModel> GetDisputeComments(string siteId, string disputeId, int page, int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(DisputeCommentsService)}.{nameof(DisputeCommentsService.GetDisputeComments)} method called with {{SiteId}}, {{DisputeId}}, {{Page}}, {{PageSize}}", siteId, disputeId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(disputeId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId siteEntityId = AutoId.Parse(siteId);
		AutoId disputeEntityId = AutoId.Parse(disputeId);

		lock(mcv.Lock)
		{
			Proposal dispute = mcv.Proposals.Latest(disputeEntityId);
			if(dispute == null || dispute.Site != siteEntityId)
			{
				throw new EntityNotFoundException(nameof(Proposal).ToLower(), disputeId);
			}

			var context = new SearchContext<ProposalCommentModel>
			{
				Page = page,
				PageSize = pageSize,
				Items = new List<ProposalCommentModel>(pageSize)
			};
			LoadDisputeComments(context, dispute.Comments, cancellationToken);

			return new TotalItemsResult<ProposalCommentModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems,
			};
		}
	}

	private void LoadDisputeComments(SearchContext<ProposalCommentModel> context, AutoId[] commentsIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		IEnumerable<AutoId> pagedItems = commentsIds.Skip(context.Page *  context.PageSize).Take(context.PageSize);
		context.TotalItems = commentsIds.Length;

		foreach (AutoId commentId in pagedItems)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			ProposalComment comment = mcv.ProposalComments.Latest(commentId);
			FairAccount account = (FairAccount) mcv.Accounts.Latest(comment.Creator);
			ProposalCommentModel model = new ProposalCommentModel(comment, account);
			context.Items.Add(model);
		}
	}
}
