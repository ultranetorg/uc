namespace Uccs.Fair;

public class UserAuthorsModel : UserModel
{
	public IEnumerable<AuthorBaseAvatarModel> Authors { get; set; }
}
