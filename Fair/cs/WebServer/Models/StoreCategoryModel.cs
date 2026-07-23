namespace Uccs.Fair;

public class StoreCategoryModel(Category category, byte[]? avatar) : CategoryBaseModel(category)
{
	public byte[]? Avatar { get; set; } = avatar;
}
