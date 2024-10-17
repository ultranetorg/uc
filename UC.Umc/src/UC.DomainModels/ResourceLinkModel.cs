namespace UC.DomainModels;

public class ResourceLinkModel
{
	public string Id { get; set; } = null!;

	public string ResourceId { get; set; } = null!;

	public Uccs.Rdn.ResourceLinkFlag Flags { get; set; }
}
