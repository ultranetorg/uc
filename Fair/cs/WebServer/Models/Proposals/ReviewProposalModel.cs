namespace Uccs.Fair;

public class ReviewProposalModel(Proposal proposal, FairUser by, PublicationImageBaseModel publication) : BaseOptionsProposalModel(proposal, by)
{
	public PublicationImageBaseModel Publication { get; } = publication;
}
