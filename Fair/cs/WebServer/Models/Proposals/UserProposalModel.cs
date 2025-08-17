namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal, FairAccount signer) : BaseProposal(proposal)
{
	public AccountBaseModel Signer { get; } = new(signer);
}
