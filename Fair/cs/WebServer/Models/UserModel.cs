namespace Uccs.Fair;

public class UserModel
{
	public string Id { get; set; }

	public IEnumerable<UserSiteModel> Sites { get; set; }

	public IEnumerable<UserAuthorModel> Authors { get; set; }

	public IEnumerable<UserPublicationModel> Publications { get; set; }
	public IEnumerable<UserProductModel> Products { get; set; }

	public UserModel(string id)
	{
		Id = id;
	}
}
