namespace Uccs.Fair;

public abstract class BaseVotableOperationModel(SiteOperation operation)
{
	public string SiteId { get; set; } = operation.Site.Id.ToString();
}
