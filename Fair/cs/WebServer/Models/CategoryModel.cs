namespace Uccs.Fair;

public class CategoryModel : CategoryParentBaseModel
{
	public string ParentTitle { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; set; }

	public int PublicationsCount { get; set; }

	public CategoryModel(Category category, string parentTitle) : base(category)
	{
		ParentTitle = parentTitle;
		PublicationsCount = category.Publications.Length;
	}
}
