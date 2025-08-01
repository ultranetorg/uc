 using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, FairAccount reviewer, PublicationImageBaseModel publication)
{
	public string Id { get; } = proposal.Id.ToString();

	public int CreationTime { get; } = proposal.CreationTime.Days;

	public IEnumerable<ProposalOptionModel> Options { get; set; } = null!;

	public AccountBaseModel Reviewer { get; } = new(reviewer);
	public PublicationImageBaseModel Publication { get; } = publication;
}
