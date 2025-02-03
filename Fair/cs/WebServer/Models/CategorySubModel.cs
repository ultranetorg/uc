namespace Uccs.Fair;

public class CategorySubModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public CategorySubModel(Category category)
	{
		Id = category.Id.ToString();
		Title = category.Title;
	}
}
