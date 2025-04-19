namespace Uccs.Fair;

public class SiteDescriptionChangeModel : BaseVotableOperationModel
{
	public string SiteId { get; set; }

	public string Description { get; set; }

	public SiteDescriptionChangeModel(SiteDescriptionChange operation)
	{
		SiteId = operation.Site.ToString();
		Description = operation.Description;
	}
}
