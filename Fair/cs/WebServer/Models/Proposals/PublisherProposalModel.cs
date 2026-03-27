namespace Uccs.Fair;

public class PublisherProposalModel(Proposal proposal, FairUser by, IEnumerable<AccountBaseModel>? authors) : BaseProposalModel(proposal, by)
{
	public IEnumerable<AccountBaseModel>? Authors { get; } = authors;
}
