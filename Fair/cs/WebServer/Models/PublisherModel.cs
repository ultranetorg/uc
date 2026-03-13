namespace Uccs.Fair;

public class PublisherModel(Author author, Publisher publisher)
{
	public AuthorBaseAvatarModel Author { get; } = new AuthorBaseAvatarModel(author);

	public int BannedTill { get; } = publisher.BannedTill.Hours;

	public long EnergyLimit { get; } = publisher.EnergyLimit;
	public long SpacetimeLimit { get; } = publisher.SpacetimeLimit;
}
