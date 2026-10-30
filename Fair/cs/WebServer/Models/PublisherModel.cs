namespace Uccs.Fair;

public class PublisherModel(Author author, Publisher publisher)
{
	public AuthorBaseAvatarModel Author { get; } = new AuthorBaseAvatarModel(author);

	public int? VerifiedWebdomainRank { get; } = author.VerifiedWebdomain != null ? author.VerifiedWebdomainRank : null;

	public Time BannedTill { get; } = publisher.BannedTill;

	public long EnergyLimit { get; } = publisher.EnergyLimit;
	public long SpacetimeLimit { get; } = publisher.SpacetimeLimit;
}
