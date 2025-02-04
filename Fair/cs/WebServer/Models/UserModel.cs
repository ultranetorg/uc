namespace Uccs.Fair;

public class UserModel
{
	public string Id { get; set; }

	public IEnumerable<UserSiteModel> Sites { get; set; }

	public IEnumerable<AuthorBaseModel> Authors { get; set; }

	public UserPublicationModel[] Publications { get; set; }
	public UserProductModel[] Products { get; set; }

	public UserModel(string id)
	{
		Id = id;
	}
}
