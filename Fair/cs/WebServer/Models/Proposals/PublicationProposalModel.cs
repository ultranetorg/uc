namespace Uccs.Fair;

public class PublicationProposalModel(Proposal proposal, FairUser by, Product product, FairUser publisher, PublicationImageBaseModel? publication = null) : BaseOptionsProposalModel(proposal, by)
{
	public int UpdationTime { get; } = product.Updated.Days;

	public PublicationImageBaseModel Publication { get; } = publication;
	public AccountBaseAvatarModel Author { get; } = new(publisher);
}
