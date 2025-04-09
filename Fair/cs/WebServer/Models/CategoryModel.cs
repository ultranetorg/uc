namespace Uccs.Fair;

public class CategoryModel : CategoryParentBaseModel
{
	public string ParentTitle { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; init; }

	public CategoryModel(Category category, string parentTitlte) : base(category)
	{
		ParentTitle = parentTitlte;
	}
}
