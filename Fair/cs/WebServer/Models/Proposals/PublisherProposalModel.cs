namespace Uccs.Fair;

public class PublisherProposalModel(Proposal proposal, CreatorModel creator, IEnumerable<AuthorBaseAvatarModel> authors) : ProposalModel(proposal, creator)
{
	public IEnumerable<AuthorBaseAvatarModel> Authors { get; } = authors;
}
