namespace Uccs.Fair;

public class PublisherProposalModel(Proposal proposal, FairUser by, IEnumerable<AccountBaseModel>? additions, IEnumerable<AccountBaseModel>? removals) : BaseProposalModel(proposal, by)
{
	public IEnumerable<AccountBaseModel>? Additions { get; } = additions;
	public IEnumerable<AccountBaseModel>? Removals { get; } = removals;
}
