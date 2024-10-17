namespace UO.DomainModels.Operations;

public class ResourceLinkDeletionModel : BaseOperationModel
{
	public string SourceResourceId { get; set; } = null!;
	public string DestinationResourceId { get; set; } = null!;
}
