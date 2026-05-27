namespace Uccs.Fair;

public class UserAuthorsModel : UserModel
{
	public bool IsRemoved { get; init; }

	public IEnumerable<AuthorBaseAvatarModel> Authors { get; init; } = null!;
}
