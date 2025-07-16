using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class CategoryBaseModel(Category category)
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; } = category.Id.ToString();

	[JsonPropertyOrder(-1)]
	public string Title { get; set; } = category.Title;
}
