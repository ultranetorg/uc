namespace UC.DomainModels;

public class ResourceModel
{
	public string Id { get; set; } = null!;

	public string DomainId { get; set; } = null!;
	public string Name { get; set; } = null!;

	public Uccs.Rdn.ResourceFlags Flags { get; set; }

	public ResourceData? Data { get; set; }

	public int Updated { get; set; }

	public IEnumerable<ResourceLinkModel> Inbounds { get; set; } = null!;
	public IEnumerable<ResourceLinkModel> Outbounds { get; set; } = null!;
}
