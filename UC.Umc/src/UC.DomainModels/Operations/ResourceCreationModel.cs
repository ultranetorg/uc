using UC.DomainModels;

namespace UO.DomainModels.Operations;

public class ResourceCreationModel : BaseOperationModel
{
	public string DomainId { get; set; } = null!;
	public string Name { get; set; } = null!;

	public Uccs.Rdn.ResourceChanges Changes { get; set; }

	public ResourceData? Data { get; set; }
}
