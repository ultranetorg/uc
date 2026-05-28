namespace Uccs.Fair;

public class AuthorDetailsModel(Author author) : AuthorBaseModel(author)
{
	public string Description { get; init; }

	public string? AvatarId { get; init; }

	public IEnumerable<UserModel> OwnersIds { get; init; }

	public IEnumerable<string> Links { get; set; } = author.Referrences.Select(x => x.ToString());
}
