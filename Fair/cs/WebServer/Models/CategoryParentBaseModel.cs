namespace Uccs.Fair;

public class CategoryParentBaseModel(Category category) : CategoryBaseModel(category)
{
	public string ParentId { get; set; } = category.Parent?.ToString();
}
