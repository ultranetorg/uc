namespace Uccs.Fair;

public class AuthorBaseAvatarModel(Author author)
{
	public string Id { get; init; } = author.Id.ToString();

	public string Name { get; } = author.Name;

	public IEnumerable<string> Owners { get; } = author.Owners.Select(x => x.ToString());

	public string? AvatarId { get; } = author.Avatar?.ToString();
}
