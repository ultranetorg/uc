using System.Text.Json.Serialization;

namespace Uccs.Fair;

public sealed class SiteSearchLightModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public SiteSearchLightModel(Site site)
	{
		Id = site.Id.ToString();
		Title = site.Title;
	}
}
