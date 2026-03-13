namespace Uccs.Fair;

public class UserProposalModel(Proposal proposal, FairUser by) : BaseOptionsProposalModel(proposal, by)
{
	public AccountBaseAvatarModel Signer { get; } = new(by);
}
