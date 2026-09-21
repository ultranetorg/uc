namespace Uccs.Fair;

public class PublisherModel(Author author, Publisher publisher)
{
	public AuthorBaseAvatarModel Author { get; } = new AuthorBaseAvatarModel(author);

	public Time BannedTill { get; } = publisher.BannedTill;

	public long EnergyLimit { get; } = publisher.EnergyLimit;
	public long SpacetimeLimit { get; } = publisher.SpacetimeLimit;
}
