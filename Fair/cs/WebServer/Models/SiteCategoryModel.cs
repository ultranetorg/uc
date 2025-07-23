namespace Uccs.Fair;

public class SiteCategoryModel(Category category, byte[]? avatar) : CategoryBaseModel(category)
{
	public byte[]? Avatar { get; set; } = avatar;
}
