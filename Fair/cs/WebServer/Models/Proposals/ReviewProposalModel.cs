namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, FairUser by, PublicationImageBaseModel publication, string reviewText) : BaseProposalModel(proposal, by)
{
	public PublicationImageBaseModel Publication { get; } = publication;

	public string? ReviewText { get; } = reviewText;
}
