using UC.DomainModels;

namespace UO.DomainModels.Operations;

public class ResourceUpdationModel : BaseOperationModel
{
	public string ResourceId { get; set; } = null!;

	public Uccs.Rdn.ResourceChanges Changes { get; set; }

	public ResourceData? Data { get; set; }
}
