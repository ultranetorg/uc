namespace Uccs.Fair;

public class CategoryParentBaseModel(Category category) : CategoryBaseModel(category)
{
	public AutoId? ParentId { get; set; } = category.Parent;

	public ProductType? Type { get; } = category.Type;
}
