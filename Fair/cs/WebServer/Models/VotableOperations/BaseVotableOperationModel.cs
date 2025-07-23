namespace Uccs.Fair;

public abstract class BaseVotableOperationModel(VotableOperation operation)
{
	public string SiteId { get; set; } = operation.Site.Id.ToString();
}
