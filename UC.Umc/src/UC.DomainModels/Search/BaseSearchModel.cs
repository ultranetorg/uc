namespace UO.DomainModels.Search;

public abstract class BaseSearchModel
{
	public string Id { get; set; } = null!;

	public string EntityId { get; set; } = null!;
}
