using Uccs.Fair;

namespace Uccs.Fair;

public class PublicationProposalModel(Proposal proposal, Product product, FairAccount publisher, PublicationImageBaseModel? publication = null) : BaseProposal(proposal)
{
	public int UpdationTime { get; } = product.Updated.Days;

	public PublicationImageBaseModel Publication { get; } = publication;
	public AccountBaseModel Author { get; } = new(publisher);
}
