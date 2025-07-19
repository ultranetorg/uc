using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class SiteBaseModel(Site site, byte[]? avatar)
{
	[JsonPropertyOrder(-5)]
	public string Id { get; set; } = site.Id.ToString();

	[JsonPropertyOrder(-4)]
	public string Nickname { get; set; } = site.Nickname.ToString();

	[JsonPropertyOrder(-3)]
	public string Title { get; set; } = site.Title;

	[JsonPropertyOrder(-2)]
	public string Description { get; set; } = site.Description;

	[JsonPropertyOrder(-1)]
	public byte[]? Avatar { get; set; } = avatar;
}
