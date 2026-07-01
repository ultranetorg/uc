namespace Uccs.Fair;

public class CategoryModel(Category category) : CategoryParentBaseModel(category)
{
	public string SiteId { get; init; }

	public ProductType Type { get; set; } = category.Type;

	public IEnumerable<CategoryPathItem>? Path { get; init; }

	public int PublicationsCount { get; set; } = category.Publications.Length;

	public IEnumerable<CategoryBaseModel> Categories { get; set; }
}
