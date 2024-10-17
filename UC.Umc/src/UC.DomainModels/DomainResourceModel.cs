namespace UC.DomainModels;

public class DomainResourceModel
{
	public string Id { get; set; } = null!;

	public string Name { get; set; } = null!;

	public ResourceData? Data { get; set; }
}
