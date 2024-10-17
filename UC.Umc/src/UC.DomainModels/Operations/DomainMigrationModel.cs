namespace UO.DomainModels.Operations;

public class DomainMigrationModel : BaseOperationModel
{
	public string Name { get; set; } = null!;

	public string Tld { get; set; } = null!;

	public bool RankCheck { get; set; }

	public bool DnsApproved { get; set; }
	public bool RankApproved { get; set; }

	public string Generator { get; set; } = null!;
}
