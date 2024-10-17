namespace UO.DomainModels.Operations;

public class DomainRegistrationModel : BaseOperationModel
{
	public string Name { get; set; } = null!;

	public byte Years { get; set; }

	public string Owner { get; set; } = null!;

	public Uccs.Rdn.DomainChildPolicy Policy { get; set; }
}
