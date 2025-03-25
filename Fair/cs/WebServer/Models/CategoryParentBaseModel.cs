namespace Uccs.Fair;

public class CategoryParentBaseModel : CategoryBaseModel
{
	public string ParentId { get; set; }

	public CategoryParentBaseModel(Category category) : base(category)
	{
		ParentId = category.Parent?.ToString();
	}
}
