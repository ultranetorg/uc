namespace UO.DomainModels.Operations;

public class DomainUpdationModel : BaseOperationModel
{
	public string DomainId { get; set; } = null!;

	public Uccs.Rdn.DomainAction Action { get; set; }

	public byte Years { get; set; }

	public string? Owner { get; set; }

	public Uccs.Rdn.DomainChildPolicy Policy { get; set; }
}
