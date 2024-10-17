namespace UO.DomainModels.Operations;

public class ResourceLinkCreationModel : BaseOperationModel
{
	public string SourceResourceId { get; set; } = null!;
	public string DestinationResourceId { get; set; } = null!;

	public Uccs.Rdn.ResourceLinkChanges Changes { get; set; }
}
