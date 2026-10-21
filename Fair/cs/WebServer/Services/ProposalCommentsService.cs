using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProposalCommentsService
(
#if DEBUG
	ILogger<ProposalCommentsService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<ProposalCommentModel> GetProposalComments([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string proposalId,
		[NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(proposalId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ProposalId}, {Page}, {PageSize}", nameof(ProposalCommentsService), nameof(ProposalCommentsService.GetProposalComments), storeId, proposalId, page, pageSize);
#endif

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

			string creatorName = null;
			AutoId creatorAvatarId = null;
			if(comment.As == Role.Moderator)
			{
				FairUser account = (FairUser)mcv.Users.Latest(comment.Creator);
				creatorName = account.Name;
				creatorAvatarId = comment.Creator;
			}
			else if(comment.As == Role.Publisher)
			{
				Author author = mcv.Authors.Latest(comment.Creator);
				creatorName = author.Name;
				creatorAvatarId = author.Avatar;
			}
			else
			{
				throw new InvalidOperationException($"Unexpected comment role: {comment.As}");
			}

			ProposalCommentModel model = new ProposalCommentModel
			{
				Id = comment.Id,
				Text = comment.Text,
				CreatorId = comment.Creator,
				CreatorName = creatorName,
				CreatorAs = comment.As,
				CreatorAvatarId = creatorAvatarId,
				Created = comment.Created,
				ProposalId = comment.Proposal
			};

			context.Items.Add(model);
		}
	}
}
