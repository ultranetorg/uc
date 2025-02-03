namespace Uccs.Smp;

public class UserModel
{
	public string Id { get; set; }

	public UserSiteModel[] Sites { get; set; }

	public AuthorBaseModel[] Authors { get; set; }

	public UserPublicationModel[] Publications { get; set; }
	public UserProductModel[] Products { get; set; }
}
