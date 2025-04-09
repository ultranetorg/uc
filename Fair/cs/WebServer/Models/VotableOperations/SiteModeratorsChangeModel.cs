namespace Uccs.Fair;

public class SiteModeratorsChangeModel : BaseVotableOperationModel
{
	public string SiteId { get; set; }
	public IEnumerable<string> AdditionsIds { get; set; }
	public IEnumerable<string> RemovalsIds { get; set; }

	public SiteModeratorsChangeModel(SiteModeratorsChange operation)
	{
		SiteId = operation.Site.ToString();
		AdditionsIds = operation.Additions.Select(x => x.ToString());
		RemovalsIds = operation.Removals.Select(x => x.ToString());
	}
}
