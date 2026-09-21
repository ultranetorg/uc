namespace Uccs.Fair;

public class PublicationProposalModel(Proposal proposal, FairUser by, Product product, Author author, PublicationImageBaseModel? publication = null) : ProposalModel(proposal, by)
{
	public Time Updated { get; } = product.Updated;

	public PublicationImageBaseModel Publication { get; } = publication;

	public AutoId AuthorId { get; } = author.Id;
	public string AuthorTitle { get; } = author.Title;
	public AutoId? AuthorLogoId { get; } = author.Avatar;
}
