﻿namespace Uccs.Fair;

public class ProposalModel(Proposal dispute)
{
	public string Id { get; set; } = dispute.Id.ToString();

	public int YesCount { get; set; } = dispute.Yes.Count();
	public int NoCount { get; set; } = dispute.No.Count();
	public int AbsCount { get; set; } = dispute.Abs.Count();

	public int Expiration { get; set; } = dispute.Expiration.Days;

	public string Text { get; set; } = dispute.Text;

	public BaseVotableOperationModel Proposal { get; set; }

	public int CommentsCount { get; set; } = dispute.Comments.Count();
}
