namespace Uccs.Fair;

public class SiteAuthorsChangeModel : BaseVotableOperationModel
{
	public string SiteId { get; set; }

	public IEnumerable<string> AdditionsIds { get; set; }
	public IEnumerable<string> RemovalsIds { get; set; }

	public SiteAuthorsChangeModel(SiteAuthorsChange operation) : base(operation)
	{
		SiteId = operation.Site.ToString();
		AdditionsIds = operation.Additions.Select(x => x.ToString());
		RemovalsIds = operation.Removals.Select(x => x.ToString());
	}
}
