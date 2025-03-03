using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class SiteBaseModel
{
	[JsonPropertyOrder(-2)]
	public string Id { get; set; }

	[JsonPropertyOrder(-1)]
	public string Title { get; set; }

	public SiteBaseModel(Site site)
	{
		Id = site.Id.ToString();
		Title = site.Title;
	}
}
