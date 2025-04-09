namespace Uccs.Fair;

public class SitePolicyChangeModel : BaseVotableOperationModel
{
	public string SiteId { get; set; }

	public FairOperationClass Change { get; set; }
	public ChangePolicy Policy { get; set; }

	public SitePolicyChangeModel(SitePolicyChange operation)
	{
		SiteId = operation.Site.ToString();
		Change = operation.Change;
		Policy = operation.Policy;
	}
}
