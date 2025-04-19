using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class SiteBaseModel
{
	[JsonPropertyOrder(-4)]
	public string Id { get; set; }

	[JsonPropertyOrder(-3)]
	public string Nickname { get; set; }

	[JsonPropertyOrder(-2)]
	public string Title { get; set; }

	[JsonPropertyOrder(-1)]
	public string Description { get; set; }

	public SiteBaseModel(Site site)
	{
		Id = site.Id.ToString();
		Nickname = site.Nickname.ToString();
		Title = site.Title;
		Description = site.Description;
	}
}
