namespace Uccs.Fair;

public class CategoryModel : CategoryParentBaseModel
{
	public string SiteId { get; set; }

	public string ParentTitle { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; init; }
	public IEnumerable<PublicationBaseModel> Publications { get; init; }

	public CategoryModel(Category category, string parentTitlte) : base(category)
	{
		SiteId = category.Site.ToString();
		ParentTitle = parentTitlte;
	}
}
