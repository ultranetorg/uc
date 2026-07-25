using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class StoreBaseModel(Store store)
{
	[JsonPropertyOrder(-5)]
	public string Id { get; } = store.Id.ToString();

	[JsonPropertyOrder(-4)]
	public string Nickname { get; } = store.Name;

	[JsonPropertyOrder(-3)]
	public string Title { get; } = store.Title;

	[JsonPropertyOrder(-2)]
	public string Description { get; } = store.Description;

	[JsonPropertyOrder(-1)]
	public string ImageFileId { get; } = store.Avatar?.ToString();
}
