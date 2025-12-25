namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, FairUser reviewer, PublicationImageBaseModel publication) : BaseProposal(proposal)
{
	public AccountBaseAvatarModel Reviewer { get; } = new(reviewer);
	public PublicationImageBaseModel Publication { get; } = publication;
}
