namespace Uccs.Fair;

public class PublicationProposalModel(Proposal proposal, FairUser by, Product product, Author author, PublicationImageBaseModel? publication = null) : BaseProposalModel(proposal, by)
{
	public int UpdationTime { get; } = product.Updated.Days;

	public PublicationImageBaseModel Publication { get; } = publication;

	public string AuthorId { get; } = author.Id.ToString();
	public string AuthorTitle { get; } = author.Title;
	public string? AuthorLogoId { get; } = author.Avatar?.ToString();
}
