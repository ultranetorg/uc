namespace Uccs.Fair;

public class UserDetailsModel : UserModel
{
	public IEnumerable<SiteBaseModel> FavoriteSites { get; init; }

	public IEnumerable<string> AuthorsIds { get; init; }
}
