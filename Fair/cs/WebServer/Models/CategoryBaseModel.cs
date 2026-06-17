using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class CategoryBaseModel(Category category)
{
	[JsonPropertyOrder(-3)]
	public string Id { get; } = category.Id.ToString();

	[JsonPropertyOrder(-2)]
	public string Title { get; } = category.Title;

	[JsonPropertyOrder(-1)]
	public string AvatarId { get; } = category.Avatar?.ToString();
}
