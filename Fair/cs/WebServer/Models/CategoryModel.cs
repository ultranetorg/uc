namespace Uccs.Fair;

public class CategoryModel(Category category) : CategoryParentBaseModel(category)
{
	public string StoreId { get; init; }

	public ProductType Type { get; init; }

	public IEnumerable<CategoryPathItem>? Path { get; init; }

	public int PublicationsCount { get; set; } = category.Publications.Length;

	public IEnumerable<CategoryBaseModel> Categories { get; init; }
}
