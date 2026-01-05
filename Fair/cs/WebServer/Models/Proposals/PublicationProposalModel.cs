using Uccs.Fair;

namespace Uccs.Fair;

public class PublicationProposalModel(Proposal proposal, Product product, FairUser publisher, PublicationImageBaseModel? publication = null) : BaseProposal(proposal)
{
	public int UpdationTime { get; } = product.Updated.Days;

	public PublicationImageBaseModel Publication { get; } = publication;
	public AccountBaseAvatarModel Author { get; } = new(publisher);
}
