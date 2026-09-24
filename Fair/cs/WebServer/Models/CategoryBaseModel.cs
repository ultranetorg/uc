using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class CategoryBaseModel(Category category)
{
	[JsonPropertyOrder(-3)]
	public AutoId Id { get; } = category.Id;

	[JsonPropertyOrder(-2)]
	public string Title { get; } = category.Title;

	[JsonPropertyOrder(-1)]
	public AutoId? AvatarId { get; } = category.Avatar;
}
