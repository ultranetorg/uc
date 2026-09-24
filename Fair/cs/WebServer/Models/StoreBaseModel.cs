using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class StoreBaseModel(Store store)
{
	[JsonPropertyOrder(-5)]
	public AutoId Id { get; } = store.Id;

	[JsonPropertyOrder(-4)]
	public string Nickname { get; } = store.Name;

	[JsonPropertyOrder(-3)]
	public string Title { get; } = store.Title;

	[JsonPropertyOrder(-2)]
	public string Description { get; } = store.Description;

	[JsonPropertyOrder(-1)]
	public AutoId? ImageFileId { get; } = store.Avatar;
}
