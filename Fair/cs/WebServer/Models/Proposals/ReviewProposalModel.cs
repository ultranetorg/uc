﻿ using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, FairAccount reviewer, PublicationImageBaseModel publication) : BaseProposal(proposal)
{
	public AccountBaseModel Reviewer { get; } = new(reviewer);
	public PublicationImageBaseModel Publication { get; } = publication;
}
