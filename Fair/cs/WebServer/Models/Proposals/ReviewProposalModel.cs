namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, CreatorModel creator, PublicationImageBaseModel publication, string reviewText) : ProposalModel(proposal, creator)
{
	public PublicationImageBaseModel Publication { get; } = publication;

	public string? ReviewText { get; } = reviewText;
	public required byte Rating { get; set; }
}
