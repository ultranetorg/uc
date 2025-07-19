namespace Uccs.Fair;

public class CategoryPublicationsModel(Category category, byte[]? avatar) : CategoryBaseModel(category)
{
	public byte[]? Avatar { get; set; } = avatar;

	public List<PublicationExtendedModel> Publications { get; set; }
}
