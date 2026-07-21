using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProposalCommentsService
(
	ILogger<ProposalCommentsService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<ProposalCommentModel> GetProposalComments([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string proposalId,
		[NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(ProposalCommentsService)}.{nameof(ProposalCommentsService.GetProposalComments)} method called with {{StoreId}}, {{ProposalId}}, {{Page}}, {{PageSize}}", storeId, proposalId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(proposalId);
		Guard.Against.Negative(page, nameof(page));
		Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId proposalEntityId = AutoId.Parse(proposalId);

		Proposal proposal = mcv.Proposals.Latest(proposalEntityId);
		if(proposal == null || proposal.Store != storeEntityId)
		{
			throw new EntityNotFoundException(nameof(Proposal).ToLower(), proposalId);
		}

		var context = new SearchContext<ProposalCommentModel>
		{
			Page = page,
			PageSize = pageSize,
			Items = new List<ProposalCommentModel>(pageSize)
		};
		LoadProposalComments(context, proposal.Comments, cancellationToken);

		return new TotalItemsResult<ProposalCommentModel>
		{
			Items = context.Items,
			TotalItems = context.TotalItems,
		};
	}

	private void LoadProposalComments(SearchContext<ProposalCommentModel> context, AutoId[] commentsIds, CancellationToken cancellationToken)
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
			FairUser account = (FairUser) mcv.Users.Latest(comment.Creator);
			ProposalCommentModel model = new ProposalCommentModel(comment, account);
			context.Items.Add(model);
		}
	}
}
