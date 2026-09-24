namespace Uccs.Fair;

public class AuthorBaseAvatarModel(Author author)
{
	public AutoId Id { get; init; } = author.Id;

	public string Title { get; } = author.Title;
	public string? Name { get; } = author.Name;
	
	public AutoId? AvatarId { get; } = author.Avatar;

	public IEnumerable<AutoId> Owners { get; } = author.Owners;
}
