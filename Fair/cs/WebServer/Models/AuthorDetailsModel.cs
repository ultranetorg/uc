namespace Uccs.Fair;

public class AuthorDetailsModel(Author author) : AuthorBaseModel(author)
{
	public string Description { get; init; }

	public AutoId? AvatarId { get; init; }

	public IEnumerable<UserModel> OwnersIds { get; init; }

	public IEnumerable<string> References { get; } = author.References.Select(x => x.Uri);
}
