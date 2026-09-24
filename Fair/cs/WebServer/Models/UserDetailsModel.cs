namespace Uccs.Fair;

public class UserDetailsModel : UserModel
{
	public IEnumerable<StoreBaseModel> FavoriteStores { get; init; }

	public IEnumerable<AutoId> AuthorsIds { get; init; }

	public bool HasAvatar { get; init; }
}
