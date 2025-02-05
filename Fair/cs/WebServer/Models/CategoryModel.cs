namespace Uccs.Fair;

public class CategoryModel : CategoryBaseModel
{
	public string SiteId { get; set; }

	public string ParentId { get; set; }
	public string ParentTitle { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; init; }
	public IEnumerable<PublicationBaseModel> Publications { get; init; }

	public CategoryModel(Category category, string parentTitlte) : base(category)
	{
		SiteId = category.Site.ToString();
		ParentId = category.Parent?.ToString();
		ParentTitle = parentTitlte;
	}
}
