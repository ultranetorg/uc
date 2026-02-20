using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class SiteBaseModel(Site site)
{
	[JsonPropertyOrder(-5)]
	public string Id { get; } = site.Id.ToString();

	[JsonPropertyOrder(-4)]
	public string Nickname { get; } = site.Name;

	[JsonPropertyOrder(-3)]
	public string Title { get; } = site.Title;

	[JsonPropertyOrder(-2)]
	public string Description { get; } = site.Description;

	[JsonPropertyOrder(-1)]
	public string ImageFileId { get; } = site.Avatar?.ToString();
}
