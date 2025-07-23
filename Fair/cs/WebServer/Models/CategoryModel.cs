namespace Uccs.Fair;

public class CategoryModel(Category category, string parentTitle) : CategoryParentBaseModel(category)
{
	public CategoryType Type { get; set; } = category.Type;

	public string ParentTitle { get; set; } = parentTitle;

	public int PublicationsCount { get; set; } = category.Publications.Length;

	public IEnumerable<CategoryBaseModel> Categories { get; set; }
}
