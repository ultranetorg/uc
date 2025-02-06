using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class CategoryBaseModel
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public CategoryBaseModel(Category category)
	{
		Id = category.Id.ToString();
		Title = category.Title;
	}
}
